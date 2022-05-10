using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;

namespace Help.UI.View
{
    public partial class MainWindow
    {
        private readonly NotifyIcon _tray;

        public MainWindow()
        {
            InitializeComponent();

            _tray = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                Text = "Help",
                ContextMenu = new ContextMenu(new[]
                {
                    new MenuItem("Show", OnTrayShowClick),
                    new MenuItem("Hide", OnTrayHideClick),
                    new MenuItem("-"),
                    new MenuItem("Exit", OnTrayExitClick),
                }),
                Visible = true,
            };
            _tray.MouseClick += OnTrayClick;
        }

        public void BringToForeground()
        {
            if (WindowState == WindowState.Minimized || Visibility == Visibility.Hidden)
            {
                Show();
                WindowState = WindowState.Normal;
            }
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
        }

        private void OnTrayShowClick(object sender, EventArgs e)
        {
            BringToForeground();
        }

        private void OnTrayHideClick(object sender, EventArgs e)
        {
            Hide();
        }

        private void OnTrayExitClick(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void OnTrayClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (Visibility == Visibility.Visible)
                {
                    Hide();
                }
                else
                {
                    BringToForeground();
                }
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
#if !DEBUG
            Hide();
            e.Cancel = true;
#endif
        }

        protected override void OnClosed(EventArgs e)
        {
            _tray?.Dispose();
            base.OnClosed(e);
        }
    }
}
