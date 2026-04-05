using QuizServer.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace QuizServer
{
    public class ClientHandler
    {
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;

        public string Username { get; set; } = "";

        public ClientHandler(TcpClient tcp)
        {
            client = tcp;
            NetworkStream stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            writer.AutoFlush = true;
        }

        public void HandleClient()
        {
            try
            {
                while (true)
                {
                    string msg = reader.ReadLine();
                    if (msg == null) break;

                    Console.WriteLine("Received: " + msg);
                    ProcessMessage(msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Client lỗi: " + ex.Message);
            }
            finally
            {
                HandleDisconnect();
            }
        }

        private void ProcessMessage(string msg)
        {
            string[] parts = msg.Split('|');
            string cmd = parts[0];

            switch (cmd)
            {
                case "LOGIN":
                    HandleLogin(parts);
                    break;
                case "REGISTER":
                    HandleRegister(parts);
                    break;
                case "FORGOT":
                    HandleForgot(parts);
                    break;
                case "CREATE_ROOM":
                    HandleCreateRoom(parts);
                    break;
                case "JOIN_ROOM":
                    HandleJoinRoom(parts);
                    break;
                case "LEAVE_ROOM":
                    HandleLeaveRoom(parts);
                    break;
                case "READY":
                    HandleReady(parts);
                    break;
                case "SET_DIFFICULTY":
                    HandleSetDifficulty(parts);
                    break;
                case "KICK":
                    HandleKick(parts);
                    break;
                case "START_GAME":
                    HandleStartGame(parts);
                    break;
                case "ANSWER":
                    HandleAnswer(parts);
                    break;
            }
        }

        private void HandleLogin(string[] parts)
        {
            if (parts.Length < 3)
            {
                Send("LOGIN_FAIL|Sai dữ liệu");
                return;
            }

            string username = parts[1];
            string password = parts[2];

            bool ok = ServerManager.AuthService.Login(username, password, out string email);
            if (!ok)
            {
                Send("LOGIN_FAIL|Sai tài khoản hoặc mật khẩu");
                return;
            }

            Username = username;

            lock (ServerManager.ConnectedClients)
            {
                ServerManager.ConnectedClients[username] = this;
            }

            Send($"LOGIN_SUCCESS|{username}|{email}");
        }

        private void HandleRegister(string[] parts)
        {
            if (parts.Length < 4)
            {
                Send("REGISTER_FAIL|Thiếu dữ liệu");
                return;
            }

            string username = parts[1];
            string password = parts[2];
            string email = parts[3];

            bool ok = ServerManager.AuthService.Register(username, password, email, out string message);
            if (ok) Send("REGISTER_SUCCESS|" + message);
            else Send("REGISTER_FAIL|" + message);
        }

        private void HandleForgot(string[] parts)
        {
            if (parts.Length < 2)
            {
                Send("FORGOT_FAIL|Thiếu email");
                return;
            }

            string email = parts[1];
            bool ok = ServerManager.AuthService.ForgotPassword(email, out string password);

            if (ok) Send("FORGOT_SUCCESS|" + password);
            else Send("FORGOT_FAIL|Không tìm thấy email");
        }

        private void HandleCreateRoom(string[] parts)
        {
            if (parts.Length < 2)
            {
                Send("ROOM_CREATE_FAIL|Thiếu tên đăng nhập");
                return;
            }

            string username = parts[1];
            string roomId = ServerManager.GenerateRoomId();

            Room room = new Room
            {
                RoomId = roomId,
                Owner = username,
                Difficulty = 1,
                IsPlaying = false
            };

            Player player = new Player
            {
                TenDangNhap = username,
                IsOwner = true,
                IsReady = true,
                IsAlive = true,
                CurrentRoomId = roomId
            };

            room.Players.Add(player);

            lock (ServerManager.Rooms)
            {
                ServerManager.Rooms[roomId] = room;
            }

            Send($"ROOM_CREATED|{roomId}|{username}");
            BroadcastRoomUpdate(room);
        }

        private void HandleJoinRoom(string[] parts)
        {
            if (parts.Length < 3)
            {
                Send("JOIN_FAIL|Thiếu dữ liệu");
                return;
            }

            string roomId = parts[1];
            string username = parts[2];

            if (!ServerManager.Rooms.ContainsKey(roomId))
            {
                Send("JOIN_FAIL|Không tìm thấy phòng");
                return;
            }

            Room room = ServerManager.Rooms[roomId];

            if (room.IsPlaying)
            {
                Send("JOIN_FAIL|Phòng đang chơi");
                return;
            }

            if (room.Players.Any(p => p.TenDangNhap == username))
            {
                Send("JOIN_FAIL|Bạn đã có trong phòng");
                return;
            }

            room.Players.Add(new Player
            {
                TenDangNhap = username,
                IsOwner = false,
                IsReady = false,
                IsAlive = true,
                CurrentRoomId = roomId
            });

            Send($"JOIN_SUCCESS|{roomId}");
            BroadcastRoomUpdate(room);
        }

        private void HandleLeaveRoom(string[] parts)
        {
            if (parts.Length < 3) return;

            string roomId = parts[1];
            string username = parts[2];

            if (!ServerManager.Rooms.ContainsKey(roomId)) return;

            Room room = ServerManager.Rooms[roomId];
            Player p = room.Players.FirstOrDefault(x => x.TenDangNhap == username);

            if (p == null) return;

            room.Players.Remove(p);

            if (room.Players.Count == 0)
            {
                ServerManager.Rooms.Remove(roomId);
                return;
            }

            if (room.Owner == username)
            {
                Player newOwner = room.Players[0];
                room.Owner = newOwner.TenDangNhap;

                foreach (var item in room.Players)
                    item.IsOwner = false;

                newOwner.IsOwner = true;
                newOwner.IsReady = true;
            }

            BroadcastRoomUpdate(room);
        }

        private void HandleReady(string[] parts)
        {
            if (parts.Length < 3) return;

            string roomId = parts[1];
            string username = parts[2];

            if (!ServerManager.Rooms.ContainsKey(roomId)) return;

            Room room = ServerManager.Rooms[roomId];
            Player p = room.Players.FirstOrDefault(x => x.TenDangNhap == username);

            if (p == null) return;
            if (p.IsOwner) return;

            p.IsReady = !p.IsReady;
            BroadcastRoomUpdate(room);
        }

        private void HandleSetDifficulty(string[] parts)
        {
            if (parts.Length < 4) return;

            string roomId = parts[1];
            string username = parts[2];
            int difficulty = int.Parse(parts[3]);

            if (!ServerManager.Rooms.ContainsKey(roomId)) return;

            Room room = ServerManager.Rooms[roomId];
            if (room.Owner != username)
            {
                Send("SET_DIFFICULTY_FAIL|Chỉ chủ phòng mới được chọn độ khó");
                return;
            }

            room.Difficulty = difficulty;
            BroadcastRoomUpdate(room);
        }

        private void HandleKick(string[] parts)
        {
            if (parts.Length < 4) return;

            string roomId = parts[1];
            string owner = parts[2];
            string target = parts[3];

            if (!ServerManager.Rooms.ContainsKey(roomId)) return;

            Room room = ServerManager.Rooms[roomId];
            if (room.Owner != owner)
            {
                Send("KICK_FAIL|Chỉ chủ phòng mới được đuổi");
                return;
            }

            Player targetPlayer = room.Players.FirstOrDefault(x => x.TenDangNhap == target);
            if (targetPlayer == null) return;
            if (targetPlayer.TenDangNhap == room.Owner) return;

            room.Players.Remove(targetPlayer);
            SendToUser(target, "KICKED|Bạn đã bị đuổi khỏi phòng");
            BroadcastRoomUpdate(room);
        }

        private void HandleStartGame(string[] parts)
        {
            if (parts.Length < 4)
            {
                Send("START_FAIL|Thiếu dữ liệu");
                return;
            }

            string roomId = parts[1];
            string username = parts[2];
            int difficulty = int.Parse(parts[3]);

            if (!ServerManager.Rooms.ContainsKey(roomId))
            {
                Send("START_FAIL|Không tìm thấy phòng");
                return;
            }

            Room room = ServerManager.Rooms[roomId];

            if (room.Owner != username)
            {
                Send("START_FAIL|Chỉ chủ phòng mới được bắt đầu");
                return;
            }

            room.Difficulty = difficulty;

            bool allReady = ServerManager.GameService.AllPlayersReady(room);
            if (!allReady)
            {
                Send("START_FAIL|Chưa phải tất cả người chơi đều sẵn sàng");
                return;
            }

            room.Questions = ServerManager.QuestionService.GetQuestionsForDifficulty(room.Difficulty);
            room.TimePerQuestion = ServerManager.QuestionService.GetTimeByDifficulty(room.Difficulty);

            ServerManager.GameService.PrepareGame(room);

            Broadcast(room, "START_OK");
            SendCurrentQuestion(room);
        }

        private void HandleAnswer(string[] parts)
        {
            if (parts.Length < 4) return;

            string roomId = parts[1];
            string username = parts[2];
            string answer = parts[3];

            if (!ServerManager.Rooms.ContainsKey(roomId)) return;

            Room room = ServerManager.Rooms[roomId];
            if (!room.IsPlaying) return;
            if (room.CurrentQuestionIndex >= room.Questions.Count) return;

            Player player = room.Players.FirstOrDefault(x => x.TenDangNhap == username);
            if (player == null) return;
            if (!player.IsAlive) return;
            if (!string.IsNullOrEmpty(player.LastAnswer)) return;

            player.LastAnswer = answer;

            bool allAnswered = room.Players.Where(p => p.IsAlive).All(p => !string.IsNullOrEmpty(p.LastAnswer));
            if (allAnswered)
            {
                ProcessCurrentQuestion(room);
            }
        }

        private void SendCurrentQuestion(Room room)
        {
            if (room.CurrentQuestionIndex >= room.Questions.Count)
            {
                EndGame(room);
                return;
            }

            foreach (var p in room.Players)
            {
                p.LastAnswer = "";
            }

            Question q = room.Questions[room.CurrentQuestionIndex];

            string msg = $"QUESTION|{q.MaCauHoi}|{q.NoiDung}|{q.A}|{q.B}|{q.C}|{q.D}|{room.TimePerQuestion}|{q.MucDo}";
            Broadcast(room, msg);

            Thread timerThread = new Thread(() =>
            {
                Thread.Sleep(room.TimePerQuestion * 1000);
                if (room.IsPlaying)
                {
                    ProcessCurrentQuestion(room);
                }
            });

            timerThread.IsBackground = true;
            timerThread.Start();
        }

        private void ProcessCurrentQuestion(Room room)
        {
            if (!room.IsPlaying) return;
            if (room.CurrentQuestionIndex >= room.Questions.Count) return;

            Question q = room.Questions[room.CurrentQuestionIndex];

            foreach (Player p in room.Players)
            {
                if (!p.IsAlive) continue;

                if (string.IsNullOrEmpty(p.LastAnswer))
                {
                    p.IsAlive = false;
                    SendToUser(p.TenDangNhap, $"ANSWER_RESULT|TIMEOUT|0|{p.TongDiem}");
                    continue;
                }

                if (p.LastAnswer.Equals(q.DapAnDung, StringComparison.OrdinalIgnoreCase))
                {
                    int plus = ServerManager.QuestionService.GetScoreByQuestionLevel(q.MucDo);
                    p.TongDiem += plus;
                    SendToUser(p.TenDangNhap, $"ANSWER_RESULT|CORRECT|{plus}|{p.TongDiem}");
                }
                else
                {
                    p.IsAlive = false;
                    SendToUser(p.TenDangNhap, $"ANSWER_RESULT|WRONG|0|{p.TongDiem}");
                }
            }

            BroadcastScoreBoard(room);

            room.CurrentQuestionIndex++;

            if (room.CurrentQuestionIndex >= room.Questions.Count)
            {
                EndGame(room);
                return;
            }

            if (ServerManager.GameService.CountAlivePlayers(room) <= 0)
            {
                EndGame(room);
                return;
            }

            Thread.Sleep(1500);
            SendCurrentQuestion(room);
        }

        private void BroadcastScoreBoard(Room room)
        {
            string data = string.Join(";", room.Players.Select(p =>
                $"{p.TenDangNhap},{p.TongDiem},{(p.IsAlive ? 1 : 0)}"));

            Broadcast(room, $"SCORE_UPDATE|{data}");
        }

        private void EndGame(Room room)
        {
            room.IsPlaying = false;

            var ranked = room.Players
                .OrderByDescending(p => p.TongDiem)
                .ThenBy(p => p.TenDangNhap)
                .ToList();

            for (int i = 0; i < ranked.Count; i++)
            {
                ServerManager.AuthService.SaveFinalResult(ranked[i].TenDangNhap, ranked[i].TongDiem);
                SendToUser(ranked[i].TenDangNhap, $"GAME_OVER|{ranked[i].TongDiem}|{i + 1}");
            }

            BroadcastScoreBoard(room);
        }

        private void BroadcastRoomUpdate(Room room)
        {
            string players = string.Join(";", room.Players.Select(p =>
                $"{p.TenDangNhap},{(p.IsReady ? 1 : 0)},{(p.IsOwner ? 1 : 0)}"));

            string msg = $"ROOM_UPDATE|{room.RoomId}|{room.Owner}|{room.Difficulty}|{players}";
            Broadcast(room, msg);
        }

        private void Broadcast(Room room, string message)
        {
            foreach (Player p in room.Players)
            {
                SendToUser(p.TenDangNhap, message);
            }
        }

        private void SendToUser(string username, string message)
        {
            lock (ServerManager.ConnectedClients)
            {
                if (ServerManager.ConnectedClients.ContainsKey(username))
                {
                    ServerManager.ConnectedClients[username].Send(message);
                }
            }
        }

        public void Send(string msg)
        {
            try
            {
                writer.WriteLine(msg);
            }
            catch
            {
            }
        }

        private void HandleDisconnect()
        {
            try
            {
                if (!string.IsNullOrEmpty(Username))
                {
                    lock (ServerManager.ConnectedClients)
                    {
                        if (ServerManager.ConnectedClients.ContainsKey(Username))
                            ServerManager.ConnectedClients.Remove(Username);
                    }

                    foreach (var room in ServerManager.Rooms.Values.ToList())
                    {
                        Player p = room.Players.FirstOrDefault(x => x.TenDangNhap == Username);
                        if (p != null)
                        {
                            room.Players.Remove(p);

                            if (room.Players.Count == 0)
                            {
                                ServerManager.Rooms.Remove(room.RoomId);
                                break;
                            }

                            if (room.Owner == Username)
                            {
                                Player newOwner = room.Players[0];
                                room.Owner = newOwner.TenDangNhap;

                                foreach (var item in room.Players)
                                    item.IsOwner = false;

                                newOwner.IsOwner = true;
                                newOwner.IsReady = true;
                            }

                            BroadcastRoomUpdate(room);
                            break;
                        }
                    }
                }

                client.Close();
            }
            catch
            {
            }
        }
    }
}