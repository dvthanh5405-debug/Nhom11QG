using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuizClient
{
    public class FormGame : Form
    {
        private Label lblTitle;
        private Label lblQuestion;
        private Label lblTimer;
        private Label lblScore;
        private Label lblScoreTitle;
        private Button btnA;
        private Button btnB;
        private Button btnC;
        private Button btnD;
        private ListView lvScore;

        private Panel pnlTop;
        private Panel pnlQuestion;
        private Panel pnlAnswers;
        private Panel pnlScoreBoard;

        private bool answered = false;

        public FormGame()
        {
            InitializeUI();
            Program.Client.MessageReceived += OnServerMessage;
            FormClosed += FormGame_FormClosed;
        }

        private void FormGame_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.Client.MessageReceived -= OnServerMessage;
        }

        private void InitializeUI()
        {
            Text = "Game - Ai Là Triệu Phú";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1100, 680);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(10, 14, 45);
            Font = new Font("Segoe UI", 10F);

            pnlTop = new Panel
            {
                Location = new Point(20, 15),
                Size = new Size(1040, 90),
                BackColor = Color.FromArgb(28, 36, 90)
            };

            lblTitle = new Label
            {
                Text = "AI LÀ TRIỆU PHÚ",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.Gold,
                AutoSize = true,
                Location = new Point(390, 22),
                BackColor = Color.Transparent
            };

            lblTimer = new Label
            {
                Text = "Thời gian: 0 giây",
                AutoSize = true,
                Location = new Point(25, 18),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White
            };

            lblScore = new Label
            {
                Text = "Điểm: 0",
                AutoSize = true,
                Location = new Point(25, 50),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White
            };

            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(lblTimer);
            pnlTop.Controls.Add(lblScore);

            pnlQuestion = new Panel
            {
                Location = new Point(35, 130),
                Size = new Size(700, 140),
                BackColor = Color.FromArgb(22, 27, 74),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblQuestion = new Label
            {
                Text = "Câu hỏi sẽ hiển thị ở đây",
                Location = new Point(18, 18),
                Size = new Size(660, 100),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlQuestion.Controls.Add(lblQuestion);

            pnlAnswers = new Panel
            {
                Location = new Point(35, 290),
                Size = new Size(700, 300),
                BackColor = Color.Transparent
            };

            btnA = CreateAnswerButton(new Point(0, 0), "A. Đáp án A");
            btnB = CreateAnswerButton(new Point(360, 0), "B. Đáp án B");
            btnC = CreateAnswerButton(new Point(0, 150), "C. Đáp án C");
            btnD = CreateAnswerButton(new Point(360, 150), "D. Đáp án D");

            btnA.Click += (s, e) => SendAnswer("A");
            btnB.Click += (s, e) => SendAnswer("B");
            btnC.Click += (s, e) => SendAnswer("C");
            btnD.Click += (s, e) => SendAnswer("D");

            pnlAnswers.Controls.Add(btnA);
            pnlAnswers.Controls.Add(btnB);
            pnlAnswers.Controls.Add(btnC);
            pnlAnswers.Controls.Add(btnD);

            pnlScoreBoard = new Panel
            {
                Location = new Point(770, 130),
                Size = new Size(290, 460),
                BackColor = Color.FromArgb(22, 27, 74),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblScoreTitle = new Label
            {
                Text = "BẢNG ĐIỂM",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Gold,
                AutoSize = true,
                Location = new Point(75, 18),
                BackColor = Color.Transparent
            };

            lvScore = new ListView
            {
                Location = new Point(15, 60),
                Size = new Size(255, 375),
                View = View.Details,
                GridLines = true,
                FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };

            lvScore.Columns.Add("Người chơi", 110);
            lvScore.Columns.Add("Điểm", 55);
            lvScore.Columns.Add("Còn", 55);

            pnlScoreBoard.Controls.Add(lblScoreTitle);
            pnlScoreBoard.Controls.Add(lvScore);

            Controls.Add(pnlTop);
            Controls.Add(pnlQuestion);
            Controls.Add(pnlAnswers);
            Controls.Add(pnlScoreBoard);
        }

        private Button CreateAnswerButton(Point location, string text)
        {
            Button btn = new Button
            {
                Size = new Size(320, 115),
                Location = location,
                Text = text,
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };

            btn.FlatAppearance.BorderSize = 3;
            btn.FlatAppearance.BorderColor = Color.Gold;

            btn.MouseEnter += (s, e) =>
            {
                if (btn.Enabled)
                    btn.BackColor = Color.FromArgb(255, 170, 40);
            };

            btn.MouseLeave += (s, e) =>
            {
                if (btn.Enabled)
                    btn.BackColor = Color.FromArgb(255, 140, 0);
            };

            return btn;
        }

        private void SendAnswer(string answer)
        {
            if (answered) return;

            answered = true;
            Program.Client.Send($"ANSWER|{Session.CurrentRoomId}|{Session.Username}|{answer}");

            btnA.Enabled = false;
            btnB.Enabled = false;
            btnC.Enabled = false;
            btnD.Enabled = false;
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
                case "QUESTION":
                    ShowQuestion(parts);
                    break;

                case "ANSWER_RESULT":
                    ShowAnswerResult(parts);
                    break;

                case "SCORE_UPDATE":
                    UpdateScoreBoard(parts);
                    break;

                case "GAME_OVER":
                    int finalScore = int.Parse(parts[1]);
                    string rank = parts.Length > 2 ? parts[2] : "?";
                    MessageBox.Show($"Game kết thúc.\nĐiểm: {finalScore}\nXếp hạng: {rank}");
                    new FormTrangChu().Show();
                    Hide();
                    break;
            }
        }

        private void ShowQuestion(string[] parts)
        {
            if (parts.Length < 9) return;

            answered = false;

            lblQuestion.Text = parts[2];
            btnA.Text = "A. " + parts[3];
            btnB.Text = "B. " + parts[4];
            btnC.Text = "C. " + parts[5];
            btnD.Text = "D. " + parts[6];
            lblTimer.Text = "Thời gian: " + parts[7] + " giây";

            btnA.Enabled = true;
            btnB.Enabled = true;
            btnC.Enabled = true;
            btnD.Enabled = true;

            btnA.BackColor = Color.FromArgb(255, 140, 0);
            btnB.BackColor = Color.FromArgb(255, 140, 0);
            btnC.BackColor = Color.FromArgb(255, 140, 0);
            btnD.BackColor = Color.FromArgb(255, 140, 0);
        }

        private void ShowAnswerResult(string[] parts)
        {
            if (parts.Length < 4) return;

            string status = parts[1];
            int tongDiem = int.Parse(parts[3]);
            Session.CurrentScore = tongDiem;

            lblScore.Text = "Điểm: " + tongDiem;

            if (status == "CORRECT")
                MessageBox.Show("Bạn trả lời đúng!");
            else if (status == "WRONG")
                MessageBox.Show("Bạn trả lời sai!");
            else if (status == "TIMEOUT")
                MessageBox.Show("Hết thời gian!");
        }

        private void UpdateScoreBoard(string[] parts)
        {
            if (parts.Length < 2) return;

            lvScore.Items.Clear();
            string[] players = parts[1].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string p in players)
            {
                string[] info = p.Split(',');
                if (info.Length < 3) continue;

                ListViewItem item = new ListViewItem(info[0]);
                item.SubItems.Add(info[1]);
                item.SubItems.Add(info[2] == "1" ? "Có" : "Không");
                lvScore.Items.Add(item);
            }
        }
    }
}