using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Help.Main.Server
{
    internal sealed class Server : Base
    {
        public event Action<Msg> MsgEvent;

        public void Start()
        {
            Task.Run(() =>
            {
                Log("Start");

                var str = Settings.Mode == Settings.CLIENT ? Settings.ClientIp : Settings.ServerIp;
                if (!IPAddress.TryParse(str, out var ip))
                {
                    Except($"Invalid IP address [ {str} ]");
                    run = false;
                }

                // try run server
                TcpListener listener = null;
                try
                {
                    listener = new TcpListener(ip, Settings.Port);
                    listener.Start();
                }
                catch (Exception ex)
                {
                    Except(ex);
                    Except($"Start listening on [ {ip}:{Settings.Port} ] FAIL");
                    run = false;
                }

                while (run)
                {
                    TcpClient client = null;
                    try
                    {
                        client = listener.AcceptTcpClient();
                    }
                    catch (SocketException ex) // ???
                    {
                        if (ex.SocketErrorCode == SocketError.Interrupted)
                        {
                            break;
                        }
                    }

                    if (client != null)
                    {
                        Task.Run(() => RecvMsg(client));
                    }
                }

                Except("Stopped");
            });
        }

        private void RecvMsg(TcpClient client)
        {
            try
            {
                using (var stream = client.GetStream())
                {
                    var data = new BinaryFormatter().Deserialize(stream);
                    if (data is Msg msg)
                    {
                        msg.From = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                        Log($"[ {msg.DataType} ] incoming from [ {msg.From} ]");
                        MsgEvent?.Invoke(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Except(ex);
            }

            client.Close();
        }
    }
}
