using System;
using System.Threading;
using System.Windows;

using Help.UI.View;

namespace Help
{
    public partial class App
    {
        private const string UniqueEventName = "{GUID_HELP_EVENT_123}";
        private const string UniqueMutexName = "{GUID_HELP_MUTEX_321}";
        private EventWaitHandle _eventWaitHandle;
        private Mutex _mutex;

        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            _mutex = new Mutex(true, UniqueMutexName, out var isOwned);
            _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

            GC.KeepAlive(_mutex);

            if (isOwned)
            {
                new Thread(() =>
                {
                    while (_eventWaitHandle.WaitOne())
                    {
                        Current.Dispatcher.Invoke(() =>
                        {
                            ((MainWindow)Current.MainWindow)?.BringToForeground();
                        });
                    }
                })
                {
                    IsBackground = true
                }.Start();
                return;
            }

            _eventWaitHandle.Set();
            Shutdown();
        }
    }
}
