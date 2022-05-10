using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Help.UI.Model
{
    internal class BaseNotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        protected void Set<T>(ref T field, T value, [CallerMemberName] string prop = null)
        {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged(prop);
        }
    }
}
