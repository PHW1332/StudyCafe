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
                    RefreshScreen();
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

            /* 뽀모도로 집중 남은 시간(초). "none"이면 제한 없음 */
            if (message.StartsWith("timer:Limit:"))
            {
                string value = message.Substring("timer:Limit:".Length);
                int seconds;
                timer.SetLimit(int.TryParse(value, out seconds) ? seconds : (int?)null);
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
                            RefreshScreen();
                        }
                        break;
                    }
                case "timer:Start":
                    {
                        timer.Start(currentSubject);
                        RefreshScreen();
                        break;
                    }
                case "timer:Pause":
                    {
                        RunSafely(timer.Pause);
                        ReloadStats();
                        RefreshScreen();
                        break;
                    }
                case "timer:LimitReached":
                    {
                        RunSafely(timer.PauseAtLimit);
                        ReloadStats();
                        RefreshScreen();
                        break;
                    }
                case "timer:Stop":
                    {
                        RunSafely(timer.Stop);
                        ReloadStats();
                        RefreshScreen();
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

        /* 오늘 누적 공부 시간(초) = DB에 저장된 시간 + 타이머의 아직 저장 안 한 시간.
           타이머가 멈춰 있으면 두 번째 값이 0이라 누적 시간도 멈춘다 */
        private int TodaySeconds(DateTime now)
        {
            int todayIndex = (now.Date - StudyDatabase.WeekStart(now.Date)).Days;
            return weekSeconds[todayIndex] + timer.GetUnsavedSecondsToday(now);
        }

        private void SendGoalMinutes()
        {
            PostMessage("goalMinutes:" + Properties.Settings.Default.GoalMinutes);
        }

        /* 이번 주 그래프(시간 단위)와 오늘 목표 달성률(%)을 화면에 보낸다 */
        private void SendStats(DateTime now)
        {
            if (statsDate != now.Date)
            {
                ReloadStats();
            }

            int todayIndex = (now.Date - StudyDatabase.WeekStart(now.Date)).Days;
            int todaySeconds = TodaySeconds(now);
            string week = string.Join(",", weekSeconds.Select((seconds, index) =>
            {
                int value = index == todayIndex ? todaySeconds : seconds;
                return (value / 3600.0).ToString("0.######", CultureInfo.InvariantCulture);
            }));
            PostMessage("week:" + week);

            int goalSeconds = Properties.Settings.Default.GoalMinutes * 60;
            double percent = goalSeconds > 0 ? todaySeconds * 100.0 / goalSeconds : 100;
            PostMessage("goal:" + percent.ToString("0.###", CultureInfo.InvariantCulture));
        }

        /* 타이머 시간과 누적 시간을 같은 시각(now)으로 계산해서 함께 보낸다 */
        private void RefreshScreen()
        {
            if (webView21.CoreWebView2 == null) return;
            DateTime now = DateTime.Now;
            string text = TimeSpan.FromSeconds(timer.GetElapsedSeconds(now)).ToString(@"hh\:mm\:ss");
            PostMessage("time:" + text);
            SendStats(now);
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
            RefreshScreen();
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }
    }
}
