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
            MessageBox.Show("JS가 보낸 메시지: " + e.TryGetWebMessageAsString());
            webView21.CoreWebView2.PostWebMessageAsString(DateTime.Now.ToString("HH:mm:ss"));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
