using System.Threading.Tasks;

using Help.Main.Database;
using Help.Main.Orion;
using Help.Main.Telegram;
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
                Settings.LoadRegistry();
                Settings.Save();

                Title += $" - {Settings.Mode} mode";

                var database = new Database();
                database.LogEvent += Log;
                database.Init();

                switch (Settings.Mode)
                {
                    case "Local":
                        var orion = new Orion();
                        orion.LogEvent += Log;
                        orion.GetLastEvent += database.GetLast;
                        orion.AlarmsEvent += database.SaveAlarms;

                        var telegram = new Telegram();
                        telegram.LogEvent += Log;
                        telegram.AlarmEvent += database.UpdateAlarm;

                        database.LastEvent += orion.SetLast;
                        database.AlarmsEvent += telegram.SendAlarms;
                        database.GetNotSend();

                        orion.Start();
                        telegram.Start();

                        break;
                    case "Client":
                        // client
                        // server
                        // orion
                        break;
                    case "Server":
                        // client
                        // server
                        // telegram
                        break;
                }
            });
        }
    }
}
