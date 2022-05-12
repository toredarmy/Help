using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;

using Help.UI.Model;

namespace Help.UI.ViewModel
{
    internal sealed partial class ViewModel
    {
        private bool _logAutoscroll = true;
        public bool LogAutoscroll { get => _logAutoscroll; set => Set(ref _logAutoscroll, value); }

        private string _logStatus = "Ready";
        public string LogStatus { get => _logStatus; private set => Set(ref _logStatus, value); }

        public ObservableCollection<LogItem> LogItems { get; } = new ObservableCollection<LogItem>();

        private void Log(LogItem item)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (LogItems.Count == 500)
                {
                    LogItems.Clear();
                }
                LogItems.Add(item);
                LogStatus = $"[ {LogItems.Count} ] {item.Text}";

                File.AppendAllText($"{Settings.Path}\\Help.log", $"{item.Time:dd-MM-yyyy HH:mm:ss:fff} - {item.Text}{Environment.NewLine}");
            });
        }

        private void Log(string text)
        {
            Log(new LogItem(text));
        }

        private void Except(string text)
        {
            Log(new LogItem(text, Brushes.IndianRed));
        }

        private void Except(Exception ex, [CallerMemberName] string method = null)
        {
            Except($"[ {ex.GetType().Name} in {method}() ]: {ex.Message}");
        }
    }
}
