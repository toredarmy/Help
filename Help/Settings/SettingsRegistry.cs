using System;

using Microsoft.Win32;

namespace Help
{
    internal partial class Settings
    {
        internal static bool LoadRegistry()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey("software\\wow6432node\\bolid\\orion\\cso\\dbparams12"))
                    if (key != null)
                    {
                        var server = key.GetValue("server name").ToString();
                        var database = key.GetValue("database name").ToString();
                        if (server.Length != 0 && database.Length != 0 && database != "master" && database != "root")
                        {
                            if (server != OrionServer | database != OrionDatabase)
                            {
                                Log($"Found [ {server}, {database} ] in Windows registry");
                                Server = server;
                                if (ObjectName.Length == 0)
                                {
                                    ObjectName = database.IndexOf('_') >= 0 ? database.Split('_')[0] : database;
                                    Log($"Change ObjectName to [ {Settings.ObjectName} ]");
                                }
                                OrionServer = server;
                                OrionDatabase = database;

                                Log($"Change Server to [ {Settings.Server} ]");
                                Log($"Change OrionServer to [ {Settings.OrionServer} ]");
                                Log($"Change OrionDatabase to [ {Settings.OrionDatabase} ]");
                                return true;
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                Except(ex);
            }
            return false;
        }
    }
}
