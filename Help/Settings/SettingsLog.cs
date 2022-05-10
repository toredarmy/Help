using System;
using System.Runtime.CompilerServices;
using System.Windows.Media;

using Help.UI.Model;

namespace Help
{
    internal partial class Settings
    {
        public static event Action<LogItem> LogEvent;

        private static void Log(LogItem item)
        {
            LogEvent?.Invoke(item);
        }

        private static void Log(string text)
        {
            Log(new LogItem($"Settings - {text}"));
        }

        private static void Except(string text)
        {
            Log(new LogItem($"Settings - {text}", Brushes.IndianRed));
        }

        private static void Except(Exception ex, [CallerMemberName] string method = null)
        {
            Except($"[ {ex.GetType().Name} in {method}() ]: {ex.Message}");
        }
    }
}
