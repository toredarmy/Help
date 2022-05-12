using System.Collections.Generic;
using System.Threading.Tasks;

using Help.Main;
using Help.Main.Client;
using Help.Main.Database;
using Help.Main.Orion;
using Help.Main.Server;
using Help.Main.Telegram;
using Help.Main.Updater;
using Help.UI.Model;

namespace Help.UI.ViewModel
{
    internal sealed partial class ViewModel : BaseNotifyPropertyChanged
    {
        private string _title = "Help";
        public string Title { get => _title; set => Set(ref _title, value); }

        private Database database;

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

                database = new Database();
                database.LogEvent += Log;
                if (!database.Init())
                {
                    Except("Database initialization FAIL");
                    Except("Program has been stopped");
                    return;
                }

                switch (Settings.Mode)
                {
                    case Settings.LOCAL:
                        LocalMode();
                        break;
                    case Settings.CLIENT:
                        ClientMode();
                        break;
                    case Settings.SERVER:
                        ServerMode();
                        break;
                }
            });
        }

        private void LocalMode()
        {
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
        }

        private void ClientMode()
        {
            var client = new Client();
            client.LogEvent += Log;
            client.MsgEvent += Client_MsgEvent;

            var server = new Server();
            server.LogEvent += Log;
            server.MsgEvent += Server_MsgEvent;

            var orion = new Orion();
            orion.LogEvent += Log;
            orion.GetLastEvent += database.GetLast;
            orion.AlarmsEvent += database.SaveAlarms;

            database.LastEvent += orion.SetLast;
            database.AlarmsEvent += client.SendAlarms;
            database.GetNotSend();

            client.Start();
            server.Start();
            orion.Start();
        }

        private void ServerMode()
        {
            var client = new Client();
            client.LogEvent += Log;
            client.MsgEvent += Client_MsgEvent;

            var server = new Server();
            server.LogEvent += Log;
            server.MsgEvent += Server_MsgEvent;

            var telegram = new Telegram();
            telegram.LogEvent += Log;
            telegram.AlarmEvent += database.UpdateAlarm;

            database.AlarmsEvent += telegram.SendAlarms;
            database.GetNotSend();

            client.Start();
            server.Start();
            telegram.Start();
        }

        private void Client_MsgEvent(Msg msg)
        {
            // message sent
            if (Settings.Mode == Settings.CLIENT)
            {
                if (msg.DataType == "Alarms")
                {
                    database.UpdateAlarms((List<Alarm>)msg.Data);
                }
            }
        }

        private void Server_MsgEvent(Msg msg)
        {
            // incoming message
            switch (Settings.Mode)
            {
                case Settings.CLIENT:
                    switch (msg.DataType)
                    {
                        case "UpdateFile":
                            var updater = new Updater();
                            updater.LogEvent += Log;
                            updater.Update((byte[])msg.Data);
                            break;
                        case "Stop":
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                App.Current.Shutdown();
                            });
                            break;
                    }
                    break;
                case Settings.SERVER:
                    switch (msg.DataType)
                    {
                        case "Alarms":
                            database.SaveAlarms((List<Alarm>)msg.Data);
                            break;
                    }
                    break;
            }
        }
    }
}
