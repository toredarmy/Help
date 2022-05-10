using System;
using System.Threading.Tasks;

using Help.UI.Model;

namespace Help.UI.ViewModel
{
    internal partial class ViewModel : BaseNotifyPropertyChanged
    {
        private string _title = "Help";
        public string Title { get => _title; set => Set(ref _title, value); }

        public ViewModel()
        {
            Log("Test");
            Except("Test");
            Except(new Exception("Test"));

            Task.Run(async () =>
            {
                while (true)
                {
                    Log("Other therad test");
                    await Task.Delay(1000);
                }
            });
        }
    }
}
