using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Help.Main.Client
{
    internal sealed class Client : Base
    {
        public event Action<Msg> MsgEvent;

        private bool run = true;
        private readonly Queue<Msg> queue = new Queue<Msg>();

        public void Start()
        {
            Task.Run(async () =>
            {
                Log("Start");

                if (Settings.Mode == Settings.CLIENT)
                {
                    if (!IPAddress.TryParse(Settings.ServerIp, out _))
                    {
                        Except($"Invalid destination IP address [ {Settings.ServerIp} ]");
                        run = false;
                    }
                }

                while (run)
                {
                    while (queue.Count > 0)
                    {
                        var msg = queue.Peek();
                        if (SendMsg(msg))
                        {
                            queue.Dequeue();
                            MsgEvent?.Invoke(msg);
                        }
                        else
                        {
                            break;
                        }
                    }
                    await Task.Delay(1000);
                }

                Except("Stopped");
            });
        }

        public void SendAlarms(List<Alarm> alarms)
        {
            if (!run)
            {
                return;
            }
            queue.Enqueue(new Msg
            {
                To = Settings.ServerIp,
                Data = alarms,
                DataType = "Alarms",
            });
            Log("Added [ Message with Alarms ] to queue");
        }

        private bool SendMsg(Msg msg)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    if (client.ConnectAsync(msg.To, Settings.Port).Wait(1000))
                    {
                        using (var stream = client.GetStream())
                            new BinaryFormatter().Serialize(stream, msg);
                        Log($"Message [ {msg.DataType} ] has been sent");
                        return true;
                    }
                    Except($"Connection to [ {msg.To}:{Settings.Port} ] FAIL");
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
