using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace TaskMain
{
    public partial class Form1 : Form
    {
        /* 과목을 고르지 않고 시작하면 이 이름으로 저장한다 */
        private const string NoSubject = "미분류";

        private readonly StudyDatabase db;
        private readonly StudyTimer timer;
        private string currentSubject = NoSubject;

        /* DB에서 읽어 둔 값. 저장할 때만 다시 읽고, 매초 표시는 이 값에 진행 중인 시간을 더한다 */
        private DateTime statsDate;
        private int[] weekSeconds = new int[7];

        public Form1()
        {
            InitializeComponent();
            ClientSize = new System.Drawing.Size(1280, 720);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            db = new StudyDatabase();
            timer = new StudyTimer(db);
            ReloadStats();

            Load += async (s, e) =>
            {
                await webView21.EnsureCoreWebView2Async();
                webView21.CoreWebView2.WebMessageReceived += OnMessage;
                webView21.CoreWebView2.NavigationCompleted += (sender, args) =>
                {
                    SendGoalMinutes();
                    SendStats();
                };
                string path = Path.Combine(Application.StartupPath, "ui", "index.html");
                webView21.CoreWebView2.Navigate(new Uri(path).AbsoluteUri);
            };

            /* 공부 중에 창을 닫아도 지금까지 공부한 시간은 저장 */
            FormClosing += (s, e) => RunSafely(timer.Stop);
        }

        private void OnMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string message = e.TryGetWebMessageAsString();

            if (message.StartsWith("subject:"))
            {
                string subject = message.Substring("subject:".Length).Trim();
                currentSubject = subject == "" ? NoSubject : subject;
                RunSafely(() => timer.ChangeSubject(currentSubject));
                ReloadStats();
                return;
            }

            switch (message)
            {
                case "openSettings":
                    {
                        Form2 setting = new Form2();
                        setting.GoalMinutes = Properties.Settings.Default.GoalMinutes;
                        if (setting.ShowDialog(this) == DialogResult.OK)
                        {
                            Properties.Settings.Default.GoalMinutes = setting.GoalMinutes;
                            Properties.Settings.Default.Save();
                            SendGoalMinutes();
                            SendStats();
                        }
                        break;
                    }
                case "timer:Start":
                    {
                        timer.Start(currentSubject);
                        break;
                    }
                case "timer:Pause":
                    {
                        RunSafely(timer.Pause);
                        ReloadStats();
                        SendStats();
                        break;
                    }
                case "timer:Stop":
                    {
                        RunSafely(timer.Stop);
                        ReloadStats();
                        SendStats();
                        break;
                    }
                default:
                    {
                        MessageBox.Show("JS가 보낸 메시지: " + message);
                        break;
                    }
            }
        }

        /* DB 저장/조회에 실패해도 앱이 꺼지지 않게 오류만 알려준다 */
        private void RunSafely(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "공부 기록을 저장하지 못했습니다.\n" + ex.Message,
                    "데이터베이스 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ReloadStats()
        {
            statsDate = DateTime.Today;
            RunSafely(() => weekSeconds = db.GetWeekSeconds(statsDate));
        }

        /* 오늘 공부한 시간(초) = DB에 저장된 시간 + 아직 저장 안 한 진행 중인 시간 */
        private int TodaySeconds()
        {
            int todayIndex = (DateTime.Today - StudyDatabase.WeekStart(DateTime.Today)).Days;
            return weekSeconds[todayIndex] + timer.GetUnsavedSecondsSince(DateTime.Today);
        }

        private void SendGoalMinutes()
        {
            PostMessage("goalMinutes:" + Properties.Settings.Default.GoalMinutes);
        }

        /* 이번 주 그래프(시간 단위)와 오늘 목표 달성률(%)을 화면에 보낸다 */
        private void SendStats()
        {
            if (statsDate != DateTime.Today)
            {
                ReloadStats();
            }

            int todayIndex = (DateTime.Today - StudyDatabase.WeekStart(DateTime.Today)).Days;
            int todaySeconds = TodaySeconds();
            string week = string.Join(",", weekSeconds.Select((seconds, index) =>
            {
                int value = index == todayIndex ? todaySeconds : seconds;
                return (value / 3600.0).ToString("0.###", CultureInfo.InvariantCulture);
            }));
            PostMessage("week:" + week);

            int goalSeconds = Properties.Settings.Default.GoalMinutes * 60;
            double percent = goalSeconds > 0 ? todaySeconds * 100.0 / goalSeconds : 100;
            PostMessage("goal:" + percent.ToString("0.#", CultureInfo.InvariantCulture));
        }

        private void PostMessage(string message)
        {
            if (webView21.CoreWebView2 == null) return;
            webView21.CoreWebView2.PostWebMessageAsString(message);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (webView21.CoreWebView2 == null) return;
            string text = TimeSpan.FromSeconds(timer.GetElapsedSeconds()).ToString(@"hh\:mm\:ss");
            PostMessage("time:" + text);
            SendStats();
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }
    }
}
