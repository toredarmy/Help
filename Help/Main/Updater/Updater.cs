using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Help.Main.Updater
{
    internal sealed class Updater : Base
    {
        private readonly string tmpFilename = $"{Settings.Path}\\Help.tmp";
        private readonly string batFilename = $"{Settings.Path}\\update.bat";

        public void Update(byte[] bytes)
        {
            Task.Run(() =>
            {
                if (Save(bytes))
                {
                    if (CreateBatFile())
                    {
                        RunBatFile();
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            App.Current.Shutdown();
                        });
                    }
                }
            });
        }

        private bool Save(byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(tmpFilename, bytes);
                Log("Save data to [ Help.tmp ] file");
                return true;
            }
            catch (Exception ex)
            {
                Except(ex);
            }
            Except("Error save update data, updating abort");
            return false;
        }

        private bool CreateBatFile()
        {
            const string contents =
                "@echo\r\n" +
                "timeout 3\r\n" +
                "del Help.exe\r\n" +
                "ren Help.tmp Help.exe\r\n" +
                "timeout 1\r\n" +
                "start Help.exe\r\n";

            try
            {
                File.WriteAllText(batFilename, contents);
                Log("Create [ update.bat ]");
                return true;
            }
            catch (Exception ex)
            {
                Except(ex);
            }
            Except("Error create update.bat file, updating abort");
            return false;
        }

        private void RunBatFile()
        {
            var process = new Process();
            process.StartInfo.FileName = batFilename;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            Log("Run [ update.bat ]");
        }
    }
}
