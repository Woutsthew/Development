using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    public class TCPClient : IDisposable
    {
        #region Variables

        public IPAddress IPAddress { get; private set; }
        public int Port { get; private set; }

        private NetworkStream Stream { get; set; }
        private TcpClient client { get; set; }
        private Thread receiveThread { get; set; }

        public UdpClient udpClient { get; set; }

        public int udpPort { get; private set; }
        public string filePath { get; private set; }

        public int timeout { get; private set; }

        public int currentId { get; set; } = 0;
        public byte[] byteFile { get; set; }

        private Queue<string> QueueMessages { get; set; } = new Queue<string>();

        public Timer retrySend { get; set; }

        #endregion

        #region Handler and Event

        public delegate void InfoHandler(TCPClient client);

        public event InfoHandler OnConnected;

        public event InfoHandler OnDisconnected;

        public delegate void MessageHandler(TCPClient client, string message);

        public event MessageHandler OnMessage;

        public delegate void ErrorHandler(TCPClient client, Exception e);

        public event ErrorHandler OnError;

        #endregion

        public TCPClient(string address, int port, int udpPort, string filePath, int timeout) :
            this(IPAddress.Parse(address), port, udpPort, filePath, timeout) { }

        public TCPClient(IPAddress address, int port, int udpPort, string filePath, int timeout)
        {
            IPAddress = address;
            Port = port;

            this.udpPort = udpPort;
            this.filePath = filePath;
            this.timeout = timeout;
        }

        #region Connect/Disconnect

        public bool isConnected { get; private set; }

        public bool Connect()
        {
            try
            {
                client = new TcpClient();
                client.Connect(IPAddress, Port);
                Stream = client.GetStream();
                isConnected = true;

                receiveThread = new Thread(new ThreadStart(ReceiveProcess));
                receiveThread.Start();
                OnConnected?.Invoke(this);

                byteFile = File.ReadAllBytes(filePath);

                return true;
            }
            catch { return false; }
        }

        public void Disconnect()
        {
            Send(CommandMessage.DisconnectMessage);
            Abort();
        }

        private void Abort()
        {
            OnDisconnected?.Invoke(this); isConnected = false;
            if (Stream != null) Stream.Close();
            if (client != null) client.Close();
        }

        #endregion

        #region Receive/Send

        private void ReceiveProcess()
        {
            try
            {
                while (isConnected == true)
                {
                    Receive();

                    while (QueueMessages.Count != 0)
                    {
                        var request = QueueMessages.Dequeue();
                        if (request == CommandMessage.DisconnectMessage) { Abort(); break; }
                        OnMessage?.Invoke(this, request);
                    }
                }
            }
            catch (Exception e)
            {
                if (e is ObjectDisposedException) { }
                else if (e is IOException) { Abort(); }
                else { OnError?.Invoke(this, e); Disconnect(); }
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

        public void UdpSend()
        {
            if (isConnected == false) return;
            string responce = $"{currentId} {byteFile[currentId]}";
            byte[] data = Encoding.Unicode.GetBytes(responce);
            udpClient.Send(data, data.Length, IPAddress.ToString(), udpPort);
        }

        #endregion

        public void Dispose() { Disconnect(); }
    }
}