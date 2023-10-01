using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection.Emit;
using static switchDesktops.Keys;
using static switchDesktops.InterceptInput;

namespace switchDesktops
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private void updateDataContext(){
            this.DataContext = new { lastActuralKeyOperationContext = "実際のキー操作：" + Keys.curActualKeyOperation + Keys.prevActualKeyOperation };

        }

        public MainWindow()
        {
            InitializeComponent();
            AddKeyDownEvent(InterceptKeyboard_KeyDownEvent);
            AddKeyUpEvent(InterceptKeyboard_KeyUpEvent);
            updateDataContext();
        }


        private void showLog(string log)
        {
            Log.Text = log + " " + Log.Text;
            if (Log.Text.Length > 255) Log.Text = Log.Text[..255];
            updateDataContext();
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




        private void showWinBtnState()
        {
            IsWinBtnPressed.Background = winKeyIsDown ? System.Windows.Media.Brushes.Lime : System.Windows.Media.Brushes.DarkGreen;
        }

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
        }

        private IntPtr InterceptKeyboard_KeyUpEvent(object sender, InterceptKeyboard.OriginalKeyEventArg e)
        {
            recordKeyEvent(e, KeyEvent.Up);
            var log = String.Format("{0}up ", e.KeyCode);
            showLog(log);
            if (e.KeyCode == VK_WIN) winKeyIsDown = false;
            showWinBtnState();
            judgeWinSingleUpDownEvent();
            return IntPtr.Zero;
            return (IntPtr)200;
        }

        private IntPtr InterceptKeyboard_KeyDownEvent(object sender, InterceptKeyboard.OriginalKeyEventArg e)
        {
            recordKeyEvent(e, KeyEvent.Down);
            if (prevKeyCode == curKeyCode && prevKeyEvent == curKeyEvent)
            {
                showLog(".");
            }
            else
            {
                showLog(String.Format("{0} ", e.KeyCode));
            }
            if (e.KeyCode == VK_WIN) winKeyIsDown = true;
            showWinBtnState();
            judgeWinSingleUpDownEvent();
            return IntPtr.Zero;
            return (IntPtr)200;
        }

        ~MainWindow()
        {
            EndKeyListen();
        }


        private void WinButton_Click(object sender, RoutedEventArgs e)
        {
            var winInput = InterceptInput.KeyDown(VK_WIN);
            InterceptInput.KeyUp(winInput);
        }

        private void DesktopMoveLeftButton_Click(object sender, RoutedEventArgs e)
        {
            MoveDesktopLeft();
        }

        private void DesktopMoveRightButton_Click(object sender, RoutedEventArgs e)
        {
            MoveDesktopRight();
        }
    }
}
