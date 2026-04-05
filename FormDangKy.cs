using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuizClient
{
    public class FormDangKy : Form
    {
        private Panel pnlMain;
        private Label lblTitle;
        private Label lblUser;
        private Label lblPass;
        private Label lblEmail;
        private TextBox txtUser;
        private TextBox txtPass;
        private TextBox txtEmail;
        private CheckBox chkShowPass;
        private Button btnDangKy;
        private Button btnDong;

        public FormDangKy()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Đăng ký tài khoản";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(480, 470);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 10F);

            pnlMain = new Panel
            {
                Size = new Size(340, 340),
                Location = new Point(60, 40),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblTitle = new Label
            {
                Text = "ĐĂNG KÝ",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 60, 120),
                AutoSize = true,
                Location = new Point(105, 20)
            };

            lblUser = new Label { Text = "Tài khoản", AutoSize = true, Location = new Point(35, 80) };
            txtUser = new TextBox { Size = new Size(260, 30), Location = new Point(35, 105) };

            lblPass = new Label { Text = "Mật khẩu", AutoSize = true, Location = new Point(35, 145) };
            txtPass = new TextBox
            {
                Size = new Size(260, 30),
                Location = new Point(35, 170),
                UseSystemPasswordChar = true
            };

            chkShowPass = new CheckBox
            {
                Text = "Hiển thị mật khẩu",
                AutoSize = true,
                Location = new Point(35, 205)
            };
            chkShowPass.CheckedChanged += (s, e) =>
            {
                txtPass.UseSystemPasswordChar = !chkShowPass.Checked;
            };

            lblEmail = new Label { Text = "Email", AutoSize = true, Location = new Point(35, 230) };
            txtEmail = new TextBox { Size = new Size(260, 30), Location = new Point(35, 255) };

            btnDangKy = new Button
            {
                Text = "Đăng ký",
                Size = new Size(260, 38),
                Location = new Point(35, 295),
                BackColor = Color.FromArgb(52, 104, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDangKy.FlatAppearance.BorderSize = 0;
            btnDangKy.Click += BtnDangKy_Click;

            btnDong = new Button
            {
                Text = "Đóng",
                Size = new Size(100, 36),
                Location = new Point(170, 390),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDong.Click += (s, e) => Close();

            pnlMain.Controls.Add(lblTitle);
            pnlMain.Controls.Add(lblUser);
            pnlMain.Controls.Add(txtUser);
            pnlMain.Controls.Add(lblPass);
            pnlMain.Controls.Add(txtPass);
            pnlMain.Controls.Add(chkShowPass);
            pnlMain.Controls.Add(lblEmail);
            pnlMain.Controls.Add(txtEmail);
            pnlMain.Controls.Add(btnDangKy);

            Controls.Add(pnlMain);
            Controls.Add(btnDong);
        }

        private void BtnDangKy_Click(object sender, EventArgs e)
        {
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(pass) ||
                string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            Program.Client.Send($"REGISTER|{user}|{pass}|{email}");
            Close();
        }
    }
}