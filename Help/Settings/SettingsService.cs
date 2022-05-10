using System;
using System.ServiceProcess;

namespace Help
{
    internal partial class Settings
    {
        public static string ServerService =>
            Server.IndexOf('\\') >= 0 ? $"MSSQL${Server.Split('\\')[1]}" : $"MSSQL${Server}";
        public static string OrionServerService =>
            OrionServer.IndexOf('\\') >= 0 ? $"MSSQL${OrionServer.Split('\\')[1]}" : $"MSSQL${OrionServer}";

        public static bool IsServiceRunning(string service)
        {
            try
            {
                using (var sc = new ServiceController(service))
                    if (sc.Status == ServiceControllerStatus.Running)
                        return true;
            }
            catch (Exception ex)
            {
                Except(ex);
            }
            return false;
        }
    }
}
