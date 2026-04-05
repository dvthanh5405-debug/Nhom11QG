using QuizServer.Models;
using QuizServer.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace QuizServer
{
    public class ServerManager
    {
        private readonly int port;
        private TcpListener listener;

        public static Dictionary<string, ClientHandler> ConnectedClients = new Dictionary<string, ClientHandler>();
        public static Dictionary<string, Room> Rooms = new Dictionary<string, Room>();

        public static AuthService AuthService = new AuthService();
        public static QuestionService QuestionService = new QuestionService();
        public static GameService GameService = new GameService();

        private static readonly object lockObj = new object();

        public ServerManager(int port)
        {
            this.port = port;
        }

        public void Start()
        {
            Console.WriteLine("Server dang mo cong: " + port);

            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Console.WriteLine("Server da start thanh cong");

            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.IsBackground = true;
            acceptThread.Start();
        }

        private void AcceptClients()
        {
            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Có client kết nối tới server.");

                    ClientHandler handler = new ClientHandler(client);
                    Thread t = new Thread(handler.HandleClient);
                    t.IsBackground = true;
                    t.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi accept client: " + ex.Message);
                }
            }
        }

        public static string GenerateRoomId()
        {
            Random rd = new Random();
            string id;

            lock (lockObj)
            {
                do
                {
                    id = rd.Next(100000, 999999).ToString();
                }
                while (Rooms.ContainsKey(id));
            }

            return id;
        }
    }
}