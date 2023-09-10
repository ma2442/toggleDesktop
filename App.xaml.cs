using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace toggleDesktop
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {


        /// <summary>
        /// アプリケーションが終了する時のイベント。
        /// </summary>
        /// <param name="e">イベント データ。</param>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            interceptKeyboard.UnHook();
        }

        /// <summary>
        /// アプリケーションが開始される時のイベント。
        /// </summary>
        /// <param name="e">イベント データ。</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainFunc();
            var icon = GetResourceStream(new Uri("icon.ico", UriKind.Relative)).Stream;
            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("<-", null, (object sender, EventArgs e) => MoveDesktopLeft());
            menu.Items.Add("->", null, (object sender, EventArgs e) => MoveDesktopRight());
            menu.Items.Add("ウィンドウ表示", null, (object sender, EventArgs e) => Show_MainWindow());
            menu.Items.Add("終了", null, Exit_Click);
            var notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Visible = true,
                Icon = new System.Drawing.Icon(icon),
                Text = "デスクトップきりかえ～る",
                ContextMenuStrip = menu
            };
            notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(NotifyIcon_Click);
        }

        private void NotifyIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                MoveDesktopLeft();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                MoveDesktopRight();
            }
        }

        private void Show_MainWindow()
        {
            var wnd = new MainWindow();
            wnd.Show();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Shutdown();
        }

        InterceptKeyboard interceptKeyboard;

        private void MainFunc()
        {
            interceptKeyboard = new InterceptKeyboard();
            interceptKeyboard.KeyDownEvent += InterceptKeyboard_KeyDownEvent;
            interceptKeyboard.KeyUpEvent += InterceptKeyboard_KeyUpEvent;
            interceptKeyboard.Hook();
        }


        #region CONST
        private const int VK_WIN = 91;
        private const int VK_TAB = 9;
        private const int VK_CONTROL = 0x11;
        private const int VK_ALT = 164;
        private const int VK_LEFT = 0x25;
        private const int VK_RIGHT = 0x27;

        private enum KeyEvent { Up, Down };
        #endregion

        private bool winKeyIsDown = false;
        private KeyEvent prevKeyEvent = KeyEvent.Up;
        private KeyEvent curKeyEvent = KeyEvent.Up;
        private int prevKeyCode = -1;
        private int curKeyCode = -1;


        /// <summary>
        /// キーのイベント種類とキーコードの記録を更新する関数
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ke"></param>
        private void recordKeyEvent(InterceptKeyboard.OriginalKeyEventArg e, KeyEvent ke)
        {
            prevKeyCode = curKeyCode;
            curKeyCode = e.KeyCode;

            prevKeyEvent = curKeyEvent;
            curKeyEvent = ke;
        }

        /// <summary>
        /// windowsキー単独押下を判定する関数
        /// </summary>
        private void judgeWinSingleUpDownEvent()
        {
            // winキーが押下されたらデスクトップを一つ左に切り替える(win + ctrl + ←)。 
            // デスクトップ２であそび、緊急時にデスクトップ１で仕事に切り替える運用を想定。
            if (prevKeyEvent == KeyEvent.Down && prevKeyCode == VK_WIN
                && curKeyEvent == KeyEvent.Up && curKeyCode == VK_WIN)
            {
                var interceptInput = new InterceptInput();
                var winInput = interceptInput.KeyDown(VK_WIN);
                var ctrlInput = interceptInput.KeyDown(VK_CONTROL);
                var arrowInput = interceptInput.KeyDown(VK_LEFT);
                interceptInput.KeyUp(arrowInput);
                interceptInput.KeyUp(ctrlInput);
                interceptInput.KeyUp(winInput);
            }
        }

        private void InterceptKeyboard_KeyUpEvent(object sender, InterceptKeyboard.OriginalKeyEventArg e)
        {
            recordKeyEvent(e, KeyEvent.Up);
            winKeyIsDown = !(e.KeyCode == VK_WIN);
            judgeWinSingleUpDownEvent();

        }

        private void InterceptKeyboard_KeyDownEvent(object sender, InterceptKeyboard.OriginalKeyEventArg e)
        {
            recordKeyEvent(e, KeyEvent.Down);
            winKeyIsDown = e.KeyCode == VK_WIN;
            judgeWinSingleUpDownEvent();
        }

        private void MoveDesktopLeft()
        {
            var interceptInput = new InterceptInput();
            var winInput = interceptInput.KeyDown(VK_WIN);
            var ctrlInput = interceptInput.KeyDown(VK_CONTROL);
            var arrowInput = interceptInput.KeyDown(VK_LEFT);
            interceptInput.KeyUp(arrowInput);
            interceptInput.KeyUp(ctrlInput);
            interceptInput.KeyUp(winInput);
        }

        private void MoveDesktopRight()
        {
            var interceptInput = new InterceptInput();
            var winInput = interceptInput.KeyDown(VK_WIN);
            var ctrlInput = interceptInput.KeyDown(VK_CONTROL);
            var arrowInput = interceptInput.KeyDown(VK_RIGHT);
            interceptInput.KeyUp(arrowInput);
            interceptInput.KeyUp(ctrlInput);
            interceptInput.KeyUp(winInput);
        }
    }

    /// <summary>
    /// 多重起動を防止する為のミューテックス。
    /// </summary>
    // private static Mutex _mutex;
}
