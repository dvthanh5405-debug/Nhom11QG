using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuizClient
{
    public class FormQuenMatKhau : Form
    {
        private Panel pnlMain;
        private Label lblTitle;
        private Label lblEmail;
        private TextBox txtEmail;
        private Button btnXacNhan;
        private Button btnDong;

        public FormQuenMatKhau()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Quên mật khẩu";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(420, 280);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 10F);

            pnlMain = new Panel
            {
                Size = new Size(300, 150),
                Location = new Point(45, 30),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblTitle = new Label
            {
                Text = "NHẬP EMAIL",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.FromArgb(40, 60, 120),
                Location = new Point(75, 15)
            };

            lblEmail = new Label { Text = "Email", AutoSize = true, Location = new Point(25, 60) };
            txtEmail = new TextBox { Size = new Size(240, 30), Location = new Point(25, 85) };

            btnXacNhan = new Button
            {
                Text = "Xác nhận",
                Size = new Size(110, 35),
                Location = new Point(45, 190),
                BackColor = Color.FromArgb(52, 104, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnXacNhan.FlatAppearance.BorderSize = 0;
            btnXacNhan.Click += BtnXacNhan_Click;

            btnDong = new Button
            {
                Text = "Đóng",
                Size = new Size(110, 35),
                Location = new Point(200, 190),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDong.Click += (s, e) => Close();

            pnlMain.Controls.Add(lblTitle);
            pnlMain.Controls.Add(lblEmail);
            pnlMain.Controls.Add(txtEmail);

            Controls.Add(pnlMain);
            Controls.Add(btnXacNhan);
            Controls.Add(btnDong);
        }

        private void BtnXacNhan_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Vui lòng nhập email.");
                return;
            }

            Program.Client.Send($"FORGOT|{email}");
            Close();
        }
    }
}