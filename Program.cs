using System;
using System.Text;

namespace QuizServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            ServerManager server = new ServerManager(8888);
            server.Start();

            Console.WriteLine("======================================");
            Console.WriteLine("   QUIZ SERVER - AI LA TRIEU PHU");
            Console.WriteLine("   Server đang chạy ở cổng 8888");
            Console.WriteLine("======================================");
            Console.WriteLine("Nhấn ENTER để thoát...");

            Console.ReadLine();
        }
    }
}