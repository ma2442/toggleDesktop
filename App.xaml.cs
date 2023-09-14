using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static switchDesktops.Keys;

namespace switchDesktops
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
        /// 
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            EndFunc();
        }

        /// <summary>
        /// アプリケーションが開始される時のイベント。
        /// </summary>
        /// <param name="e">イベント データ。</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            BeginFunc();
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
    }

    /// <summary>
    /// 多重起動を防止する為のミューテックス。
    /// </summary>
    // private static Mutex _mutex;
}
