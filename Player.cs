using System.IO;
using System.Net.Sockets;

namespace QuizServer.Models
{
    public class Player
    {
        public string TenDangNhap { get; set; }
        public string Email { get; set; }
        public int TongDiem { get; set; }
        public bool IsReady { get; set; }
        public bool IsOwner { get; set; }
        public bool IsAlive { get; set; }
        public string CurrentRoomId { get; set; }
        public string LastAnswer { get; set; }

        public TcpClient Client { get; set; }
        public StreamReader Reader { get; set; }
        public StreamWriter Writer { get; set; }
    }
}