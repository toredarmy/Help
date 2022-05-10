using System.Runtime.InteropServices;
using System.Text;

namespace Help
{
    internal static partial class Settings
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private static void IniWriteValue(string section, string key, string val)
        {
            WritePrivateProfileString(section, key, val, IniFilename);
        }

        private static void IniWriteValue(string section, string key, int val)
        {
            IniWriteValue(section, key, val.ToString());
        }

        private static string IniReadValue(string section, string key, string def = "")
        {
            var sb = new StringBuilder(255);
            GetPrivateProfileString(section, key, def, sb, 255, IniFilename);
            return sb.ToString();
        }

        private static int IniReadValue(string section, string key, int def = 0)
        {
            return int.TryParse(IniReadValue(section, key, def.ToString()), out var result) ? result : def;
        }
    }
}
