using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            string ip = args[0];
            int port = Convert.ToInt32(args[1]);
            string saveDir = args[2];

            TCPServer srv = new TCPServer(ip, port, saveDir);
            srv.Start();

            Console.ReadKey();
        }
    }
}
