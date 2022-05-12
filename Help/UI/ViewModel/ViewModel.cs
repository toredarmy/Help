using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Interop;

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
        private Telegram telegram;

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

            telegram = new Telegram();
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

            telegram = new Telegram();
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
            if (Settings.Mode == Settings.CLIENT)
            {
                if (msg.DataType == "UpdateFile")
                {
                    var updater = new Updater();
                    updater.Update((byte[])msg.Data);
                    // update programm
                    // save data to temp file
                    // run update.bat
                    //    if not busy: saves alarms ... etc
                    //    if ...
                    // close app
                }
            }
            else if (Settings.Mode == Settings.SERVER)
            {
                if (msg.DataType == "Alarms")
                {
                    telegram.SendAlarms((List<Alarm>)msg.Data);
                }
            }
        }
    }
}
