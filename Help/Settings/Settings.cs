using System.IO;
using System.Reflection;

namespace Help
{
    internal static partial class Settings
    {
        public const string LOCAL = "Local";
        public const string CLIENT = "Client";
        public const string SERVER = "Server";

        public static string Exe { get; } = Assembly.GetExecutingAssembly().Location;
        public static string Path { get; } = System.IO.Path.GetDirectoryName(Exe);
        public static string IniFilename { get; } = $"{Path}\\Help.ini";

        public static string Mode { get; private set; } = LOCAL;
        public static string Server { get; private set; } = ".\\SQLSERVER2008";
        public static string Database { get; private set; } = "HelpDatabase_v1";
        public static string ObjectName { get; private set; }

        public static string OrionServer { get; private set; } = ".\\SQLSERVER2008";
        public static string OrionUid { get; private set; } = "sa";
        public static string OrionPwd { get; private set; } = "123456";
        public static string OrionDatabase { get; private set; }

        public static string ClientIp { get; private set; }
        public static string ServerIp { get; private set; }
        public static int Port { get; private set; } = 23432;

        public static string TelegramToken { get; private set; }
        public static string TelegramChatId { get; private set; }

        public static string ConnectionString =>
            $"server={Server};database={Database};integrated security=true";
        public static string ConnectionStringMaster =>
            $"server={Server};database=master;integrated security=true";
        public static string ConnectionStringOrion =>
            $"server={OrionServer};uid={OrionUid};pwd={OrionPwd};database={OrionDatabase}";

        public static void Delete()
        {
            try
            {
                if (File.Exists(IniFilename))
                {
                    File.Delete(IniFilename);
                    Log($"[ {IniFilename} ] delete");
                }
            }
            catch { }
        }

        public static void Load()
        {
            Mode = IniReadValue("Global", "Mode", Mode);
            if (Mode != LOCAL & Mode != CLIENT & Mode != SERVER)
            {
                Except($"Undefined Mode [ {Mode} ] change to [ { LOCAL } ]");
                Mode = LOCAL;
            }
            Server = IniReadValue("Global", "Server", Server);
            Database = IniReadValue("Global", "Database", Database);
            ObjectName = IniReadValue("Global", "ObjectName", ObjectName);

            OrionServer = IniReadValue("Orion", "Server", OrionServer);
            OrionUid = IniReadValue("Orion", "Uid", OrionUid);
            OrionPwd = IniReadValue("Orion", "Pwd", OrionPwd);
            OrionDatabase = IniReadValue("Orion", "Database", OrionDatabase);

            ClientIp = IniReadValue("Net", "ClientIP", ClientIp);
            ServerIp = IniReadValue("Net", "ServerIp", ServerIp);
            Port = IniReadValue("Net", "Port", Port);

            TelegramToken = IniReadValue("Telegram", "Token", TelegramToken);
            TelegramChatId = IniReadValue("Telegram", "ChatId", TelegramChatId);

            if (Mode != SERVER)
            {
                LoadRegistry();
            }

            Log($"[ {IniFilename} ] load");
        }

        public static void Save()
        {
            IniWriteValue("Global", "Mode", Mode);
            IniWriteValue("Global", "Server", Server);
            IniWriteValue("Global", "Database", Database);
            IniWriteValue("Global", "ObjectName", ObjectName);

            IniWriteValue("Orion", "Server", OrionServer);
            IniWriteValue("Orion", "Uid", OrionUid);
            IniWriteValue("Orion", "Pwd", OrionPwd);
            IniWriteValue("Orion", "Database", OrionDatabase);

            IniWriteValue("Net", "ClientIP", ClientIp);
            IniWriteValue("Net", "ServerIP", ServerIp);
            IniWriteValue("Net", "Port", Port);

            IniWriteValue("Telegram", "TelegramToken", TelegramToken);
            IniWriteValue("Telegram", "TelegramChatId", TelegramChatId);

            Log($"[ {IniFilename} ] save");
        }
    }
}
