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
        public static bool[] isDown = new bool[512];
        // 直近の有効なキーボード操作
        public static (int code, KeyEvent e) curActualKeyOperation = (-1, KeyEvent.Up);
        public static (int code, KeyEvent e) prevActualKeyOperation = (-1, KeyEvent.Up);

        public static (int code, KeyEvent e) curKey = (-1, KeyEvent.Up);
        public static (int code, KeyEvent e) prevKey = (-1, KeyEvent.Up);


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
            prevKey = curKey;
            curKey = (keyCode, keyEvent);

            // 実際のキー操作を記憶 押しっぱなし以外が該当
            if (!(isDown[curKey.code] && curKey.e == KeyEvent.Down))
            {
                prevActualKeyOperation = curActualKeyOperation;
                curActualKeyOperation = curKey;
            }

            // キー状態
            isDown[curKey.code] = Convert.ToBoolean(curKey.e);
        }

        static InterceptKeyboard interceptKeyboard;
        /// <summary>
        /// フック開始とイベント登録
        /// </summary>
        public static void BeginKeyListen()
        {
            interceptKeyboard = new InterceptKeyboard();
            interceptKeyboard.KeyDownEvent += InterceptKeyboard_KeyDownEvent;
            interceptKeyboard.KeyUpEvent += InterceptKeyboard_KeyUpEvent;
            interceptKeyboard.Hook();
        }

        /// <summary>
        /// 外部からキーイベントを追加する用の関数 AddKeyDownEvent, AddKeyUpEvent
        /// </summary>
        /// <param name="e"></param>
        public static void AddKeyDownEvent(InterceptKeyboard.KeyEventHandler e)
        {
            interceptKeyboard.KeyDownEvent += e;
        }
        public static void AddKeyUpEvent(InterceptKeyboard.KeyEventHandler e)
        {
            interceptKeyboard.KeyUpEvent += e;
        }

        /// <summary>
        /// キーフック終了
        /// </summary>
        public static void EndKeyListen()
        {
            interceptKeyboard.UnHook();
        }

        private static bool wasPressedJustNow(int keyCode)
        {
            return curActualKeyOperation == (keyCode, KeyEvent.Up) && prevActualKeyOperation == (keyCode, KeyEvent.Down);
        }

        private static bool CtrlIsDown()
        {
            return isDown[VK_CTRL_R] || isDown[VK_CTRL_L];
        }

        private static bool AltIsDown()
        {
            return isDown[VK_ALT_R] || isDown[VK_ALT_L];
        }

        private static bool ShiftIsDown()
        {
            return isDown[VK_SHIFT_R] || isDown[VK_SHIFT_L];
        }

        /// <summary>
        /// windowsキー単独押下を判定する関数
        /// </summary>
        private static bool convertWinSingleDownUp()
        {
            // winキーが押下されたらデスクトップを一つ左に切り替える(win + ctrl + ←)。 
            // デスクトップ２であそび、緊急時にデスクトップ１で仕事に切り替える運用を想定。

            if (wasPressedJustNow(VK_WIN) && !CtrlIsDown() && !AltIsDown() && !ShiftIsDown())
            {
                MoveDesktopLeft();
                // ここでゼロ以外を返せばフックされた文字は出力されない。
                // 現段階では不都合もないので、ゼロを返すことで、
                // Winキーもそのまま出力してしまっている。
                return true;
            }
            return false;
        }

        /// <summary>
        /// Ctrl + Win 押下を検知してWin+Ctrl+Rightをソフト入力する関数
        /// </summary>
        /// <returns></returns>
        private static bool convertWinCtrlCombination()
        {
            if (!AltIsDown() && !ShiftIsDown())
            {
                if ((!CtrlIsDown() && isDown[VK_WIN])
                    && (wasPressedJustNow(VK_CTRL_L) || wasPressedJustNow(VK_CTRL_R)))
                {
                    MoveDesktopRight();
                }
                else if (CtrlIsDown() && wasPressedJustNow(VK_WIN))
                {
                    MoveDesktopRight();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 入力内容を変換する関数
        /// </summary>
        /// <returns></returns>
        private static IntPtr convertInput()
        {
            if (convertWinSingleDownUp()) return IntPtr.Zero;
            if (convertWinCtrlCombination()) return IntPtr.Zero;
            return IntPtr.Zero;
        }

        private static IntPtr InterceptKeyboard_KeyUpEvent(object sender, InterceptKeyboard.OriginalKeyEventArg e)
        {
            RecordKeyEvent(e.KeyCode, KeyEvent.Up);
            return convertInput();
        }

        private static IntPtr InterceptKeyboard_KeyDownEvent(object sender, InterceptKeyboard.OriginalKeyEventArg e)
        {
            RecordKeyEvent(e.KeyCode, KeyEvent.Down);
            return convertInput();
        }

    }
}
