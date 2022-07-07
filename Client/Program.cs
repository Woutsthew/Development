using System;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string ip = args[0];
            int port = Convert.ToInt32(args[1]);
            int udpPort = Convert.ToInt32(args[2]);
            string filePath = args[3];
            int timeout = Convert.ToInt32(args[4]);

            TCPClient client = new TCPClient(ip, port, udpPort, filePath, timeout);
            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;
            client.OnMessage += Client_OnMessage;
            client.OnError += Client_OnError;

            while (client.Connect() == false) { }

            string msg = $"start:{udpPort} {filePath}";
            client.Send(msg);
            client.udpClient = new UdpClient();

            Console.ReadKey();
        }

        private static void Client_OnConnected(TCPClient client) { Console.WriteLine("Connected..."); }

        private static void Client_OnDisconnected(TCPClient client) { Console.WriteLine("Disconnected..."); }

        private static void Client_OnError(TCPClient client, Exception e) { Console.WriteLine(e.Message + e.Source + e.TargetSite + e.StackTrace + e.InnerException); }

        private static void Client_OnMessage(TCPClient client, string message)
        {
            if (Int32.TryParse(message, out int x) == false) { Console.WriteLine(message); return; }

            client.retrySend?.Dispose();
            client.currentId = Convert.ToInt32(message);

            if (client.currentId == client.byteFile.Length)
            {
                client.Send("end");
                client.Disconnect();
                return;
            }

            client.UdpSend();
            client.retrySend = new Timer(t => client.UdpSend(), null, client.timeout, client.timeout);
        }
    }
}
