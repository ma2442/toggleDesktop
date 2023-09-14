using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static switchDesktops.InterceptInput;


namespace switchDesktops
{
    public static class Keys
    {
        #region CONST
        public const int VK_TAB = 9;
        public const int VK_WIN = 91;

        public const int VK_SHIFT_L = 160;
        public const int VK_SHIFT_R = 161;
        public const int VK_CTRL_L = 162;
        public const int VK_CTRL_R = 163;
        public const int VK_ALT_L = 164;
        public const int VK_ALT_R = 165;

        public const int VK_LEFT = 0x25;
        public const int VK_RIGHT = 0x27;

        public enum KeyEvent { Up, Down };
        #endregion

        public static bool winKeyIsDown = false;
        //public static bool altKeyIsDown = false;
        //public static bool shiftKeyIsDown = false;
        //public static bool ctrlKeyIsDown = false;
        public static KeyEvent prevKeyEvent = KeyEvent.Up;
        public static KeyEvent curKeyEvent = KeyEvent.Up;
        public static int prevKeyCode = -1;
        public static int curKeyCode = -1;


        /// <summary>
        /// 離すまで入力を待ってもよいキーか
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsDelayKey(int key)
        {
            switch (key)
            {
                //case VK_SHIFT_L:
                //case VK_SHIFT_R:
                //case VK_CTRL_L:
                //case VK_CTRL_R:
                //case VK_ALT_L:
                //case VK_ALT_R:
                case VK_WIN:
                    return true;
            }
            return false;
        }

        public static void MoveDesktopLeft()
        {
            var winInput = KeyDown(VK_WIN);
            var ctrlInput = KeyDown(VK_CTRL_L);
            var arrowInput = KeyDown(VK_LEFT);
            KeyUp(arrowInput);
            KeyUp(ctrlInput);
            KeyUp(winInput);
        }

        public static void MoveDesktopRight()
        {
            var winInput = KeyDown(VK_WIN);
            var ctrlInput = KeyDown(VK_CTRL_L);
            var arrowInput = KeyDown(VK_RIGHT);
            KeyUp(arrowInput);
            KeyUp(ctrlInput);
            KeyUp(winInput);
        }

        /// <summary>
        /// キーのイベント種類とキーコードの記録を更新する関数
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ke"></param>
        public static void RecordKeyEvent(int keyCode, Keys.KeyEvent keyEvent)
        {
            prevKeyCode = curKeyCode;
            curKeyCode = keyCode;

            prevKeyEvent = curKeyEvent;
            curKeyEvent = keyEvent;

            // キー状態
            switch (curKeyCode)
            {
                case VK_WIN:
                    if (curKeyEvent == KeyEvent.Down) winKeyIsDown = true;
                    if (curKeyEvent == KeyEvent.Up) winKeyIsDown = false;
                    break;
                //case VK_ALT_L:
                //case VK_ALT_R:
                //    if (curKeyEvent == KeyEvent.Down) altKeyIsDown = true;
                //    if (curKeyEvent == KeyEvent.Up) altKeyIsDown = false;
                //break;
                //case VK_SHIFT_L:
                //case VK_SHIFT_R:
                //    if (curKeyEvent == KeyEvent.Down) shiftKeyIsDown = true;
                //    if (curKeyEvent == KeyEvent.Up) shiftKeyIsDown = false;
                //    break;
                //case VK_CTRL_L:
                //case VK_CTRL_R:
                //    if (curKeyEvent == KeyEvent.Down) ctrlKeyIsDown = true;
                //    if (curKeyEvent == KeyEvent.Up) ctrlKeyIsDown = false;
                //    break;
                default:
                    break;
            }
        }

        static InterceptKeyboard interceptKeyboard;
        public static void BeginFunc()
        {
            interceptKeyboard = new InterceptKeyboard();
            interceptKeyboard.KeyDownEvent += InterceptKeyboard_KeyDownEvent;
            interceptKeyboard.KeyUpEvent += InterceptKeyboard_KeyUpEvent;
            interceptKeyboard.Hook();
        }

        public static void EndFunc()
        {
            interceptKeyboard.UnHook();
        }

        /// <summary>
        /// windowsキー単独押下を判定する関数
        /// </summary>
        private static IntPtr judgeWinSingleUpDownEvent()
        {
            // winキーが押下されたらデスクトップを一つ左に切り替える(win + ctrl + ←)。 
            // デスクトップ２であそび、緊急時にデスクトップ１で仕事に切り替える運用を想定。
            if (curKeyEvent == KeyEvent.Up && curKeyCode == VK_WIN)
            {
                if (prevKeyEvent == KeyEvent.Down && prevKeyCode == VK_WIN)
                {
                    MoveDesktopLeft();
                    // ここでゼロ以外を返せばフックされた文字は出力されない。
                    // 現段階では不都合もないので、ゼロを返すことで、
                    // Winキーもそのまま出力してしまっている。
                    return IntPtr.Zero;
                }
            }
            return IntPtr.Zero;
        }

        private static IntPtr InterceptKeyboard_KeyUpEvent(object sender, InterceptKeyboard.OriginalKeyEventArg e)
        {
            RecordKeyEvent(e.KeyCode, KeyEvent.Up);
            return judgeWinSingleUpDownEvent();
        }

        private static IntPtr InterceptKeyboard_KeyDownEvent(object sender, InterceptKeyboard.OriginalKeyEventArg e)
        {
            RecordKeyEvent(e.KeyCode, KeyEvent.Down);
            return judgeWinSingleUpDownEvent();
        }

    }
}
