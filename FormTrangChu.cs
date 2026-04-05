using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuizClient
{
    public class FormTrangChu : Form
    {
        private Label lblTitle;
        private Label lblWelcome;
        private Button btnTaoPhong;
        private Label lblRoom;
        private TextBox txtRoomId;
        private Button btnVaoPhong;

        public FormTrangChu()
        {
            InitializeUI();
            FormClosed += (s, e) => Application.Exit();
            Program.Client.MessageReceived += OnServerMessage;
        }

        private void InitializeUI()
        {
            Text = "Trang chủ";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(520, 420);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 10F);

            lblTitle = new Label
            {
                Text = "AI LÀ TRIỆU PHÚ - LOẠN ĐẤU",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 60, 120),
                AutoSize = true,
                Location = new Point(65, 40)
            };

            lblWelcome = new Label
            {
                Text = "Xin chào, " + Session.Username,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(190, 90)
            };

            btnTaoPhong = new Button
            {
                Text = "Tạo phòng",
                Size = new Size(220, 50),
                Location = new Point(145, 145),
                BackColor = Color.FromArgb(52, 104, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTaoPhong.FlatAppearance.BorderSize = 0;
            btnTaoPhong.Click += (s, e) =>
            {
                Program.Client.Send($"CREATE_ROOM|{Session.Username}");
            };

            lblRoom = new Label
            {
                Text = "Nhập ID phòng",
                AutoSize = true,
                Location = new Point(145, 225)
            };

            txtRoomId = new TextBox
            {
                Size = new Size(220, 30),
                Location = new Point(145, 250)
            };

            btnVaoPhong = new Button
            {
                Text = "Vào phòng",
                Size = new Size(220, 45),
                Location = new Point(145, 295),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(52, 104, 246),
                FlatStyle = FlatStyle.Flat
            };
            btnVaoPhong.FlatAppearance.BorderColor = Color.FromArgb(52, 104, 246);
            btnVaoPhong.FlatAppearance.BorderSize = 1;
            btnVaoPhong.Click += (s, e) =>
            {
                string roomId = txtRoomId.Text.Trim();
                if (string.IsNullOrWhiteSpace(roomId))
                {
                    MessageBox.Show("Vui lòng nhập ID phòng.");
                    return;
                }

                Program.Client.Send($"JOIN_ROOM|{roomId}|{Session.Username}");
            };

            Controls.Add(lblTitle);
            Controls.Add(lblWelcome);
            Controls.Add(btnTaoPhong);
            Controls.Add(lblRoom);
            Controls.Add(txtRoomId);
            Controls.Add(btnVaoPhong);
        }

        private void OnServerMessage(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnServerMessage(msg)));
                return;
            }

            string[] parts = msg.Split('|');

            switch (parts[0])
            {
                case "ROOM_CREATED":
                    Session.CurrentRoomId = parts[1];
                    FormPhong f1 = new FormPhong();
                    f1.Show();
                    Hide();
                    break;

                case "JOIN_SUCCESS":
                    Session.CurrentRoomId = parts[1];
                    FormPhong f2 = new FormPhong();
                    f2.Show();
                    Hide();
                    break;

                case "JOIN_FAIL":
                    MessageBox.Show(parts.Length > 1 ? parts[1] : "Không vào được phòng.");
                    break;
            }
        }
    }
}