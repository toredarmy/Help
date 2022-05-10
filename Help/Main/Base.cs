using System;
using System.Runtime.CompilerServices;
using System.Windows.Media;

using Help.UI.Model;

namespace Help.Main
{
    internal class Base
    {
        public event Action<LogItem> LogEvent;

        private void Log(LogItem item)
        {
            LogEvent?.Invoke(item);
        }

        protected void Log(string text)
        {
            Log(new LogItem($"{GetType().Name} - {text}"));
        }

        protected void Except(string text)
        {
            Log(new LogItem($"{GetType().Name} - {text}", Brushes.IndianRed));
        }

        protected void Except(Exception ex, [CallerMemberName] string method = null)
        {
            Except($"[ {ex.GetType().Name} in {method}() ]: {ex.Message}");
        }
    }
}
