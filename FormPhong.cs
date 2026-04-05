using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuizClient
{
    public class FormPhong : Form
    {
        private Label lblTitle;
        private Label lblRoomId;
        private Label lblOwner;
        private ComboBox cboDifficulty;
        private ListView lvPlayers;
        private Button btnReady;
        private Button btnStart;
        private Button btnKick;
        private Button btnLeave;

        public FormPhong()
        {
            InitializeUI();
            Program.Client.MessageReceived += OnServerMessage;
            FormClosed += FormPhong_FormClosed;
        }

        private void FormPhong_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.Client.MessageReceived -= OnServerMessage;
        }

        private void InitializeUI()
        {
            Text = "Phòng chờ";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(760, 520);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 10F);

            lblTitle = new Label
            {
                Text = "PHÒNG CHỜ",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 60, 120),
                AutoSize = true,
                Location = new Point(290, 20)
            };

            lblRoomId = new Label
            {
                Text = "Mã phòng: " + Session.CurrentRoomId,
                AutoSize = true,
                Location = new Point(40, 80),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            lblOwner = new Label
            {
                Text = "Chủ phòng: ",
                AutoSize = true,
                Location = new Point(40, 110)
            };

            cboDifficulty = new ComboBox
            {
                Location = new Point(520, 80),
                Size = new Size(180, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboDifficulty.Items.Add("Dễ");
            cboDifficulty.Items.Add("Trung bình");
            cboDifficulty.Items.Add("Khó");
            cboDifficulty.SelectedIndex = 0;
            cboDifficulty.SelectedIndexChanged += CboDifficulty_SelectedIndexChanged;

            lvPlayers = new ListView
            {
                Location = new Point(40, 150),
                Size = new Size(660, 250),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvPlayers.Columns.Add("Tên người chơi", 220);
            lvPlayers.Columns.Add("Sẵn sàng", 150);
            lvPlayers.Columns.Add("Chủ phòng", 150);
            lvPlayers.Columns.Add("Trạng thái", 120);

            btnReady = new Button
            {
                Text = "Sẵn sàng",
                Size = new Size(140, 42),
                Location = new Point(40, 425),
                BackColor = Color.FromArgb(52, 104, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnReady.FlatAppearance.BorderSize = 0;
            btnReady.Click += (s, e) =>
            {
                Program.Client.Send($"READY|{Session.CurrentRoomId}|{Session.Username}");
            };

            btnStart = new Button
            {
                Text = "Bắt đầu",
                Size = new Size(140, 42),
                Location = new Point(200, 425),
                BackColor = Color.FromArgb(37, 166, 91),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += (s, e) =>
            {
                Program.Client.Send($"START_GAME|{Session.CurrentRoomId}|{Session.Username}|{Session.Difficulty}");
            };

            btnKick = new Button
            {
                Text = "Đuổi người chơi",
                Size = new Size(160, 42),
                Location = new Point(360, 425),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnKick.FlatAppearance.BorderSize = 0;
            btnKick.Click += BtnKick_Click;

            btnLeave = new Button
            {
                Text = "Rời phòng",
                Size = new Size(140, 42),
                Location = new Point(540, 425),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(52, 104, 246),
                FlatStyle = FlatStyle.Flat
            };
            btnLeave.FlatAppearance.BorderColor = Color.FromArgb(52, 104, 246);
            btnLeave.FlatAppearance.BorderSize = 1;
            btnLeave.Click += (s, e) =>
            {
                Program.Client.Send($"LEAVE_ROOM|{Session.CurrentRoomId}|{Session.Username}");
                FormTrangChu home = new FormTrangChu();
                home.Show();
                Hide();
            };

            Controls.Add(lblTitle);
            Controls.Add(lblRoomId);
            Controls.Add(lblOwner);
            Controls.Add(cboDifficulty);
            Controls.Add(lvPlayers);
            Controls.Add(btnReady);
            Controls.Add(btnStart);
            Controls.Add(btnKick);
            Controls.Add(btnLeave);
        }

        private void CboDifficulty_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session.Difficulty = cboDifficulty.SelectedIndex + 1;
            Program.Client.Send($"SET_DIFFICULTY|{Session.CurrentRoomId}|{Session.Username}|{Session.Difficulty}");
        }

        private void BtnKick_Click(object sender, EventArgs e)
        {
            if (lvPlayers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Chọn người chơi cần đuổi.");
                return;
            }

            string target = lvPlayers.SelectedItems[0].SubItems[0].Text;

            if (target == Session.Username)
            {
                MessageBox.Show("Không thể tự đuổi chính mình.");
                return;
            }

            Program.Client.Send($"KICK|{Session.CurrentRoomId}|{Session.Username}|{target}");
        }

        private void OnServerMessage(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnServerMessage(msg)));
                return;
            }

            string[] parts = msg.Split('|');
            string cmd = parts[0];

            switch (cmd)
            {
                case "ROOM_UPDATE":
                    UpdateRoom(parts);
                    break;

                case "START_OK":
                    FormGame game = new FormGame();
                    game.Show();
                    Hide();
                    break;

                case "SET_DIFFICULTY_FAIL":
                case "START_FAIL":
                case "KICK_FAIL":
                    MessageBox.Show(parts.Length > 1 ? parts[1] : "Thao tác thất bại.");
                    break;

                case "KICKED":
                    MessageBox.Show(parts.Length > 1 ? parts[1] : "Bạn đã bị đuổi khỏi phòng.");
                    new FormTrangChu().Show();
                    Hide();
                    break;
            }
        }

        private void UpdateRoom(string[] parts)
        {
            if (parts.Length < 5) return;

            Session.CurrentRoomId = parts[1];
            string owner = parts[2];
            int difficulty = int.Parse(parts[3]);
            string playersData = parts[4];

            Session.IsOwner = owner == Session.Username;
            Session.Difficulty = difficulty;

            lblRoomId.Text = "Mã phòng: " + Session.CurrentRoomId;
            lblOwner.Text = "Chủ phòng: " + owner;
            cboDifficulty.SelectedIndex = difficulty - 1;

            cboDifficulty.Enabled = Session.IsOwner;
            btnStart.Enabled = Session.IsOwner;
            btnKick.Enabled = Session.IsOwner;
            btnReady.Enabled = !Session.IsOwner;

            lvPlayers.Items.Clear();

            string[] players = playersData.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string p in players)
            {
                string[] info = p.Split(',');
                if (info.Length < 3) continue;

                string name = info[0];
                bool isReady = info[1] == "1";
                bool isOwner = info[2] == "1";

                ListViewItem item = new ListViewItem(name);
                item.SubItems.Add(isReady ? "Sẵn sàng" : "Chưa sẵn sàng");
                item.SubItems.Add(isOwner ? "Có" : "Không");
                item.SubItems.Add(name == Session.Username ? "Bạn" : "Khác");
                lvPlayers.Items.Add(item);
            }
        }
    }
}