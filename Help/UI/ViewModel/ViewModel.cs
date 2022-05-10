using System.Threading.Tasks;

using Help.UI.Model;

namespace Help.UI.ViewModel
{
    internal sealed partial class ViewModel : BaseNotifyPropertyChanged
    {
        private string _title = "Help";
        public string Title { get => _title; set => Set(ref _title, value); }

        public ViewModel()
        {
            Task.Run(() =>
            {
                Settings.LogEvent += Log;
#if DEBUG
                Settings.Delete();
#endif
                Settings.Load();
                Settings.Save();

                Title += $" - {Settings.Mode} mode";
            });
        }
    }
}
