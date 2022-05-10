using System.Collections.Generic;
using System.Threading.Tasks;

namespace Help.Main.Telegram
{
    internal sealed class Telegram : Base
    {
        public void Start()
        {
            Task.Run(async () =>
            {
                Log("Start");

                while (true)
                {
                    await Task.Delay(1000);
                }
            });
        }

        public void SendAlarms(List<Alarm> alarms)
        {
        }
    }
}
