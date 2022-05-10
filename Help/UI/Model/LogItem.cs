using System;
using System.Windows.Media;

namespace Help.UI.Model
{
    internal struct LogItem
    {
        public DateTime Time { get; }
        public string Text { get; set; }
        public Brush Foreground { get; }

        public LogItem(string text, Brush foreground = null)
        {
            if (foreground == null)
            {
                foreground = Brushes.Black;
            }

            Time = DateTime.Now;
            Text = text;
            Foreground = foreground;
        }
    }
}
