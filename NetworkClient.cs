using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace QuizClient
{
    public class NetworkClient
    {
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread listenThread;

        public event Action<string> MessageReceived;

        public bool IsConnected => client != null && client.Connected;

        public bool Connect()
        {
            try
            {
                MessageBox.Show("Đang kết nối tới 192.168.1.186:8888");

                client = new TcpClient("192.168.1.186", 8888);
                NetworkStream stream = client.GetStream();

                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                writer.AutoFlush = true;

                listenThread = new Thread(ListenLoop);
                listenThread.IsBackground = true;
                listenThread.Start();

                MessageBox.Show("Kết nối server thành công");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
                return false;
            }
        }

        private void ListenLoop()
        {
            try
            {
                while (true)
                {
                    string msg = reader.ReadLine();

                    if (msg == null)
                        break;

                    if (!string.IsNullOrEmpty(msg))
                    {
                        MessageReceived?.Invoke(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mất kết nối tới server: " + ex.Message);
            }
        }

        public void Send(string msg)
        {
            try
            {
                writer?.WriteLine(msg);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi dữ liệu: " + ex.Message);
            }
        }
    }
}