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
using System.Windows.Forms;

namespace TaskMain
{
    public partial class Form1 : Form
    {
        private StudyTimer timer = new StudyTimer();
        public Form1()
        {
            InitializeComponent();
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
                        Form2 setting = new Form2();
                        setting.GoalMinutes = Properties.Settings.Default.GoalMinutes;
                        if (setting.ShowDialog(this) == DialogResult.OK)
                        {
                            Properties.Settings.Default.GoalMinutes = setting.GoalMinutes;
                            Properties.Settings.Default.Save();
                        }
                        break;
                    }
                case "timer:Start":
                    {
                        timer.Start("수학");
                        break;
                    }
                case "timer:Pause":
                    {
                        timer.Pause();
                        break;
                    }
                case "timer:Stop":
                    {
                        timer.Stop();
                        break;
                    }
                default:
                    {
                        MessageBox.Show("JS가 보낸 메시지: " + e.TryGetWebMessageAsString());
                        webView21.CoreWebView2.PostWebMessageAsString(DateTime.Now.ToString("HH:mm:ss"));
                        break;
                    }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (webView21.CoreWebView2 == null) return;
            String text = TimeSpan.FromSeconds(timer.GetElapsedSeconds()).ToString(@"hh\:mm\:ss");
            webView21.CoreWebView2.PostWebMessageAsString("time:" + text);
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }
    }
}
