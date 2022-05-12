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
                            delay = 100;
                            queue.Dequeue();
                            MsgEvent?.Invoke(msg);
                        }
                        else
                        {
                            delay += 500;
                            break;
                        }
                    }

                    await Task.Delay(delay);
                    if (delay > 10000)
                    {
                        delay = 100;
                    }
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
            delay = 100;
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
                    var timeout = delay;
                    if (timeout < 1000)
                    {
                        timeout = 1000;
                    }
                    if (client.ConnectAsync(msg.To, Settings.Port).Wait(timeout))
                    {
                        using (var stream = client.GetStream())
                        {
                            new BinaryFormatter().Serialize(stream, msg);
                        }
                        Log($"[ {msg.DataType} ] sent to [ {msg.To} ]");
                        return true;
                    }
                    Except($"Connection to [ {msg.To}:{Settings.Port}, {delay} ] FAIL");
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
