using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuizClient
{
    public class FormDangNhap : Form
    {
        private Panel pnlMain;
        private Label lblTitle;
        private Label lblUser;
        private Label lblPass;
        private TextBox txtUser;
        private TextBox txtPass;
        private CheckBox chkShowPass;
        private Button btnLogin;
        private Button btnOpenRegister;
        private Label lblForgot;

        public FormDangNhap()
        {
            InitializeUI();
            Load += FormDangNhap_Load;
            FormClosed += FormDangNhap_FormClosed;
        }

        private void FormDangNhap_Load(object sender, EventArgs e)
        {
            if (!Program.Client.IsConnected)
            {
                bool ok = Program.Client.Connect();
                if (!ok)
                {
                    MessageBox.Show("Không kết nối được tới server.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            Program.Client.MessageReceived += OnServerMessage;
        }

        private void FormDangNhap_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.Client.MessageReceived -= OnServerMessage;
        }

        private void InitializeUI()
        {
            Text = "Đăng nhập - Ai Là Triệu Phú";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(520, 500);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 10F);

            pnlMain = new Panel
            {
                Size = new Size(380, 360),
                Location = new Point(60, 50),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblTitle = new Label
            {
                Text = "ĐĂNG NHẬP",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 60, 120),
                AutoSize = true,
                Location = new Point(115, 25)
            };

            lblUser = new Label
            {
                Text = "Tài khoản",
                AutoSize = true,
                Location = new Point(45, 90)
            };

            txtUser = new TextBox
            {
                Size = new Size(280, 30),
                Location = new Point(45, 115)
            };

            lblPass = new Label
            {
                Text = "Mật khẩu",
                AutoSize = true,
                Location = new Point(45, 160)
            };

            txtPass = new TextBox
            {
                Size = new Size(280, 30),
                Location = new Point(45, 185),
                UseSystemPasswordChar = true
            };

            chkShowPass = new CheckBox
            {
                Text = "Hiển thị mật khẩu",
                AutoSize = true,
                Location = new Point(45, 225)
            };
            chkShowPass.CheckedChanged += (s, e) =>
            {
                txtPass.UseSystemPasswordChar = !chkShowPass.Checked;
            };

            lblForgot = new Label
            {
                Text = "Quên mật khẩu?",
                AutoSize = true,
                Location = new Point(210, 225),
                ForeColor = Color.RoyalBlue,
                Cursor = Cursors.Hand
            };
            lblForgot.Click += (s, e) =>
            {
                new FormQuenMatKhau().ShowDialog();
            };

            btnLogin = new Button
            {
                Text = "Đăng nhập",
                Size = new Size(280, 40),
                Location = new Point(45, 265),
                BackColor = Color.FromArgb(52, 104, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            btnOpenRegister = new Button
            {
                Text = "Đăng ký",
                Size = new Size(280, 40),
                Location = new Point(45, 315),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(52, 104, 246),
                FlatStyle = FlatStyle.Flat
            };
            btnOpenRegister.FlatAppearance.BorderColor = Color.FromArgb(52, 104, 246);
            btnOpenRegister.FlatAppearance.BorderSize = 1;
            btnOpenRegister.Click += (s, e) =>
            {
                new FormDangKy().ShowDialog();
            };

            pnlMain.Controls.Add(lblTitle);
            pnlMain.Controls.Add(lblUser);
            pnlMain.Controls.Add(txtUser);
            pnlMain.Controls.Add(lblPass);
            pnlMain.Controls.Add(txtPass);
            pnlMain.Controls.Add(chkShowPass);
            pnlMain.Controls.Add(lblForgot);
            pnlMain.Controls.Add(btnLogin);
            pnlMain.Controls.Add(btnOpenRegister);

            Controls.Add(pnlMain);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text.Trim();

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu.");
                return;
            }

            Program.Client.Send($"LOGIN|{user}|{pass}");
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
                case "LOGIN_SUCCESS":
                    Session.Username = parts[1];
                    Session.Email = parts.Length > 2 ? parts[2] : "";

                    MessageBox.Show("Đăng nhập thành công.");
                    Hide();
                    FormTrangChu home = new FormTrangChu();
                    home.Show();
                    break;

                case "LOGIN_FAIL":
                    MessageBox.Show(parts.Length > 1 ? parts[1] : "Đăng nhập thất bại.");
                    break;

                case "REGISTER_SUCCESS":
                    MessageBox.Show(parts.Length > 1 ? parts[1] : "Đăng ký thành công.");
                    break;

                case "REGISTER_FAIL":
                    MessageBox.Show(parts.Length > 1 ? parts[1] : "Đăng ký thất bại.");
                    break;

                case "FORGOT_SUCCESS":
                    MessageBox.Show("Mật khẩu của bạn là: " + (parts.Length > 1 ? parts[1] : ""));
                    break;

                case "FORGOT_FAIL":
                    MessageBox.Show(parts.Length > 1 ? parts[1] : "Không tìm thấy email.");
                    break;
            }
        }
    }
}