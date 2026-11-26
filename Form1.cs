using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMain
{
    public partial class Form1 : Form
    {
        private StudyTimer timer = new StudyTimer();
        private PomodoroForm pomo = new PomodoroForm();
        private bool pausedByPomodoro = false;
        private int todaySavedSeconds = 0;   // 오늘 끝난 세션 합계 (DB 오기 전 임시)
        private FocusModeControl focus = new FocusModeControl();
        private bool dialogOpen = false;   // 설정창 열 때도 '창 이탈'로 잡히는 것 방지
        public Form1()
        {
            InitializeComponent();
            pomo.PhaseChanged += (isRest) =>
            {
                if (isRest)
                {
                    if (timer.State == TimerState.Running)   // 공부 중이었을 때만
                    {
                        timer.Pause();                        // 휴식이니 공부 타이머 멈춤
                        pausedByPomodoro = true;              // "뽀모도로가 멈췄다"고 기억
                    }
                }
                else
                {
                    if (pausedByPomodoro)                     // 뽀모도로가 멈춘 경우에만
                    {
                        timer.Start();                        // 다시 공부 시작
                        pausedByPomodoro = false;
                    }
                }
            };
            timer.SessionEnded += (subject, start, seconds) =>
            {
                todaySavedSeconds += seconds;
            };
            Controls.Add(focus);    // 폼 위에 올려야 조원 코드가 창 이탈을 감지할 수 있음
            focus.SendToBack();     // 웹뷰 뒤에 숨김

            focus.OnFocusModeChanged += (isOn) =>
            {
                PostToWeb("focus:" + (isOn ? "on" : "off"));
            };
            focus.OnWindowLeftDuringFocus += () =>
            {
                if (dialogOpen) return;
                if (timer.State == TimerState.Running)
                {
                    timer.Pause();
                    PostToWeb("focus:left");
                }
            };
            ClientSize = new System.Drawing.Size(1280, 720);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Load += async (s, e) =>
            {
                await webView21.EnsureCoreWebView2Async();
                webView21.CoreWebView2.WebMessageReceived += OnMessage;
                string path = Path.Combine(Application.StartupPath, "ui", "index.html");
                webView21.CoreWebView2.Navigate(new Uri(path).AbsoluteUri);
            };
        }

        private void OnMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            String message = e.TryGetWebMessageAsString();
            switch (message)
            {
                case "openSettings":
                    {
                        BeginInvoke(new Action(OpenSettings));
                        break;
                    }
                case "timer:start":
                    {
                        timer.Start("수학");
                        break;
                    }
                case "timer:pause":
                    {
                        timer.Pause();
                        break;
                    }
                case "timer:stop":
                    {
                        timer.Stop();
                        break;
                    }
                case "pomo:toggle":
                    {
                        pomo.Toggle();
                        SendPomoState();
                        break;
                    }
                case "pomo:minus":
                    {
                        pomo.AdjustMinutes(-5);
                        SendPomoState();
                        break;
                    }

                case "pomo:plus":
                    {
                        pomo.AdjustMinutes(5);
                        SendPomoState();
                        break;
                    }
                case "pomo:skip":
                    {
                        pomo.SkipRest();
                        SendPomoState();
                        break;
                    }
                case "focus:toggle":
                    {
                        focus.ToggleFocusMode();
                        break; 
                    }
                default:
                    {
                        MessageBox.Show("JS가 보낸 메시지: " + e.TryGetWebMessageAsString());
                        break;
                    }
            }
        }

        private void OpenSettings()
        {
            dialogOpen = true;
            Form2 setting = new Form2();
            setting.GoalMinutes = Properties.Settings.Default.GoalMinutes;
            if (setting.ShowDialog(this) == DialogResult.OK)
            {
                Properties.Settings.Default.GoalMinutes = setting.GoalMinutes;
                Properties.Settings.Default.Save();
            }
            dialogOpen = false;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (webView21.CoreWebView2 == null) return;
            String text = TimeSpan.FromSeconds(timer.GetElapsedSeconds()).ToString(@"hh\:mm\:ss");
            webView21.CoreWebView2.PostWebMessageAsString("time:" + text);
            if(timer.State == TimerState.Stopped)
            {
                webView21.CoreWebView2.PostWebMessageAsString("time:00:00:00");
            }
            SendPomoState();
            SendGoal();
            SendTimerState();
        }

        private void SendPomoState()
        {
            if (webView21.CoreWebView2 == null) return;

            string phase = pomo.IsRestMode ? "rest" : "focus";
            string running = pomo.IsRunning ? "on" : "off";

            string msg = "pomo:" + pomo.RemainingSeconds + ":" + phase + ":"
                       + pomo.CurrentCycle + ":" + running;

            webView21.CoreWebView2.PostWebMessageAsString(msg);
        }
        private void SendGoal()
        {
            if (webView21.CoreWebView2 == null) return;

            int today = todaySavedSeconds;
            if (timer.State != TimerState.Stopped)
                today += timer.GetElapsedSeconds();   // 지금 돌고 있는 세션도 포함

            int goal = Properties.Settings.Default.GoalMinutes;
            int percent = StudyStats.GoalPercent(today, goal);

            webView21.CoreWebView2.PostWebMessageAsString("goal:" + percent + ":" + goal);
        }
        private void PostToWeb(string msg)
        {
            if (webView21.CoreWebView2 == null) return;
            webView21.CoreWebView2.PostWebMessageAsString(msg);
        }

        private void SendTimerState()
        {
            string state = timer.State == TimerState.Running ? "running"
                         : timer.State == TimerState.Paused ? "paused"
                         : "idle";
            PostToWeb("state:" + state);
        }
        private void webView21_Click(object sender, EventArgs e)
        {

        }
    }
}
