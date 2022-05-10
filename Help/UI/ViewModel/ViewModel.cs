using Help.UI.Model;

namespace Help.UI.ViewModel
{
    internal class ViewModel : BaseNotifyPropertyChanged
    {
        private string _title = "Help";
        public string Title { get => _title; set => Set(ref _title, value); }

        public ViewModel()
        {
        }
    }
}
