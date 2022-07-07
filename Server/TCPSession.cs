using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Net;

namespace Server
{
    public class TCPSession : IDisposable
    {
        #region Variables

        public Guid Id { get; private set; }
        public TCPServer server { get; private set; }
        protected internal TcpClient client { get; set; }
        public NetworkStream Stream { get; private set; }

        private UdpClient udpClient { get; set; }

        private Thread udpReceiveThread { get; set; }

        private int udpPort { get; set; }
        private string fileName { get; set; }

        private int currentId { get; set; } = 0;
        private List<byte> byteFile { get; set; } = new List<byte>();

        private Queue<string> QueueMessages { get; set; } = new Queue<string>();

        #endregion

        #region Event

        protected virtual void OnConnected() { Console.WriteLine("New connected: " + Id); }

        protected virtual void OnDisconnected() { Console.WriteLine("Session disconnected: " + Id); }

        protected virtual void OnError(Exception e) { Console.WriteLine(e.Message + e.Source + e.TargetSite + e.StackTrace + e.InnerException); }

        protected virtual void OnMessage(string message)
        {
            if (message.Split(":").First() == "start")
            {
                string[] commands = message.Split(":").Last().Split(" ");

                udpPort = Convert.ToInt32(commands.First());

                fileName = commands.Last();

                udpReceiveThread = new Thread(new ThreadStart(UdpReceiveMessage));
                udpReceiveThread.Start();

                Send(currentId.ToString());
            }

            if (message.Split(":").First() == "end")
            {
                Directory.CreateDirectory(server.SaveDirectory);
                File.WriteAllBytes(Path.Combine(server.SaveDirectory, fileName), byteFile.ToArray());
            }
        }

        private void UdpReceiveMessage()
        {
            try
            {
                udpClient = new UdpClient(udpPort);
                IPEndPoint remoteIp = null;
                while (true)
                {
                    byte[] data = udpClient.Receive(ref remoteIp);
                    string message = Encoding.Unicode.GetString(data);

                    string[] commands = message.Split(" ");

                    int curr = Convert.ToInt32(commands.First());
                    if (currentId == curr)
                    {
                        byteFile.Add(Convert.ToByte(commands.Last()));
                        Send((++currentId).ToString());
                    }
                }
            }
            catch(Exception e)
            {
                if (e is SocketException) { Send(e.Message); Disconnect(); }
            }
        }

        #endregion

        public TCPSession(TCPServer serverObject)
        {
            Id = Guid.NewGuid();
            server = serverObject;
            serverObject.AddSession(this);
        }

        #region Connect/Disconnect

        public bool isConnected { get; private set; }

        public void Disconnect()
        {
            Send(CommandMessage.DisconnectMessage);
            Abort();
        }

        private void Abort()
        {
            server.RemoveSession(this.Id);
            OnDisconnected(); isConnected = false;
            if (Stream != null) Stream.Close();
            if (client != null) client.Close();
        }

        #endregion

        #region Receive/Send

        protected internal void ProcessReceive()
        {
            try
            {
                Stream = client.GetStream();

                isConnected = true;
                OnConnected();

                while (isConnected == true)
                {
                    Receive();

                    while (QueueMessages.Count != 0)
                    {
                        var request = QueueMessages.Dequeue();
                        if (request == CommandMessage.DisconnectMessage) { Abort(); break; }
                        OnMessage(request);
                    }
                }
            }
            catch (Exception e)
            {
                if (e is ObjectDisposedException) { }
                else if (e is IOException) { Abort(); }
                else { OnError(e); Disconnect(); }
            }
        }

        private void Receive()
        {
            byte[] data = new byte[8192];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            foreach (string message in builder.ToString().Split(
                new string[] { CommandMessage.EndMessage }, StringSplitOptions.RemoveEmptyEntries))
            {
                QueueMessages.Enqueue(message);
            }
        }

        public void Send(string message)
        {
            if (isConnected == false) return;
            byte[] data = Encoding.Unicode.GetBytes(message + CommandMessage.EndMessage);
            Stream.Write(data, 0, data.Length);
        }

        #endregion

        public void Dispose() { Disconnect(); }
    }
}