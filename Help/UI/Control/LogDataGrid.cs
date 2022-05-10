using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Help.UI.Control
{
    internal class LogDataGrid : DataGrid
    {
        public static readonly DependencyProperty AutoscrollProperty
            = DependencyProperty.Register("Autoscroll", typeof(bool), typeof(LogDataGrid), new UIPropertyMetadata(null));

        public bool Autoscroll
        {
            get => (bool)GetValue(AutoscrollProperty);
            set => SetValue(AutoscrollProperty, value);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (Autoscroll)
                if (e.NewItems != null)
                    if (e.NewItems.Count > 0)
                        ScrollIntoView(e.NewItems[e.NewItems.Count - 1]);
            base.OnItemsChanged(e);
        }
    }
}
