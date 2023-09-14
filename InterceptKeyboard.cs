using System;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using static switchDesktops.InterceptInput;

abstract class AbstractInterceptKeyboard
{
    #region Win32 Constants
    protected const int WH_KEYBOARD_LL = 0x000D;
    protected const int WM_KEYDOWN = 0x0100;
    protected const int WM_KEYUP = 0x0101;
    protected const int WM_SYSKEYDOWN = 0x0104;
    protected const int WM_SYSKEYUP = 0x0105;
    #endregion

    #region WIn32API Structures
    [StructLayout(LayoutKind.Sequential)]
    public class KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public KBDLLHOOKSTRUCTFlags flags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [Flags]
    public enum KBDLLHOOKSTRUCTFlags : uint
    {
        KEYEVENTF_EXTENDEDKEY = 0x0001,
        KEYEVENTF_KEYUP = 0x0002,
        KEYEVENTF_UNICODE = 0x0004,
        KEYEVENTF_SANCODE = 0x0008,
    }
    #endregion

    #region Win32 Methods
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    #endregion

    #region Delegate
    private delegate IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    #endregion

    #region Fields
    private KeyboardProc proc;
    private IntPtr hookId = IntPtr.Zero;
    #endregion

    public void Hook()
    {
        if (hookId == IntPtr.Zero)
        {
            proc = HookProcedure;
            using (var curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    hookId = SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }

        }
    }

    public void UnHook()
    {
        UnhookWindowsHookEx(hookId);
        hookId = IntPtr.Zero;
    }

    public virtual IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
    {
        return CallNextHookEx(hookId, nCode, wParam, lParam);
    }


}

class InterceptKeyboard : AbstractInterceptKeyboard
{
    #region InputEvent
    public class OriginalKeyEventArg : EventArgs
    {
        public int KeyCode { get; }

        public OriginalKeyEventArg(int keyCode)
        {
            KeyCode = keyCode;
        }

    }
    public delegate IntPtr KeyEventHandler(object sender, OriginalKeyEventArg e);
    public event KeyEventHandler KeyDownEvent;
    public event KeyEventHandler KeyUpEvent;

    protected IntPtr OnKeyDownEvent(int keyCode)
    {
        return (IntPtr)KeyDownEvent?.Invoke(this, new OriginalKeyEventArg(keyCode));
    }
    protected IntPtr OnKeyUpEvent(int keyCode)
    {
        return (IntPtr)KeyUpEvent?.Invoke(this, new OriginalKeyEventArg(keyCode));
    }
    #endregion


    public override IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
    {
        var kb = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
        var vkCode = (int)kb.vkCode;

        // nCodeマイナスは何を意味する？
        // ハードキーボードによる入力でない場合フック素通りさせる
        if (nCode < 0 || kb.dwExtraInfo != (UIntPtr)0)
            return base.HookProcedure(nCode, wParam, lParam);

        var retCode = IntPtr.Zero;
        if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
            retCode = OnKeyDownEvent(vkCode);
        else if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
            retCode = OnKeyUpEvent(vkCode);
        
        if (retCode != IntPtr.Zero)
            return retCode;

        return base.HookProcedure(nCode, wParam, lParam);
    }

}