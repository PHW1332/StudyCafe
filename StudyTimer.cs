using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace TaskMain
{
    public enum TimerState
    {
        Stopped,
        Running,
        Paused
    }

    public class StudyTimer
    {
        public const int MaxSubjectLength = 15;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly List<string> _subjects = new List<string> { "국어", "수학", "영어" };

        public event Action<string, DateTime, int> SessionEnded;

        public TimerState State { get; private set; } = TimerState.Stopped;

        public string Subject { get; private set; }

        public DateTime StartTime { get; private set; }

        public string CurrentSubject { get; private set; } = "국어";

        public IReadOnlyList<string> Subjects
        {
            get { return _subjects; }
        }

        public void Start(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("과목을 선택하세요.", nameof(subject));
            subject = subject.Trim();

            if (!_subjects.Contains(subject)) _subjects.Add(subject);
            CurrentSubject = subject;

            switch (State)
            {
                case TimerState.Running:
                    if (subject == Subject) return;
                    Stop();
                    break;

                case TimerState.Paused:
                    if (subject == Subject)
                    {
                        _stopwatch.Start();
                        State = TimerState.Running;
                        return;
                    }
                    Stop();
                    break;
            }

            Subject = subject;
            StartTime = DateTime.Now;
            _stopwatch.Reset();
            _stopwatch.Start();
            State = TimerState.Running;
        }

        public void Start()
        {
            Start(CurrentSubject);
        }

        public void Pause()
        {
            if (State != TimerState.Running) return;
            _stopwatch.Stop();
            State = TimerState.Paused;
        }

        public void Stop()
        {
            if (State == TimerState.Stopped) return;
            _stopwatch.Stop();
            State = TimerState.Stopped;

            int seconds = GetElapsedSeconds();
            if (seconds > 0 && SessionEnded != null)
                SessionEnded(Subject, StartTime, seconds);
        }

        public int GetElapsedSeconds()
        {
            return (int)_stopwatch.Elapsed.TotalSeconds;
        }

        public void ChangeSubject(string newSubject)
        {
            if (string.IsNullOrWhiteSpace(newSubject)) return;
            newSubject = newSubject.Trim();
            if (newSubject == CurrentSubject) return;

            if (!_subjects.Contains(newSubject)) _subjects.Add(newSubject);

            TimerState before = State;
            if (before != TimerState.Stopped) Stop();

            CurrentSubject = newSubject;
            if (before == TimerState.Running) Start(newSubject);
        }

        public bool TryAddSubject(string name, out string error)
        {
            name = (name ?? "").Trim();

            if (name.Length == 0) { error = "과목 이름을 입력하세요."; return false; }
            if (name.Length > MaxSubjectLength) { error = MaxSubjectLength + "글자까지 쓸 수 있어요."; return false; }
            if (_subjects.Contains(name)) { error = "이미 있는 과목이에요."; return false; }

            _subjects.Add(name);
            error = null;
            return true;
        }

        public static string FormatTime(int seconds)
        {
            if (seconds < 0) seconds = 0;
            return string.Format("{0:00}:{1:00}:{2:00}",
                seconds / 3600, seconds / 60 % 60, seconds % 60);
        }
    }

    [ToolboxItem(true)]
    public class PillButton : Control
    {
        private bool _primary;
        private bool _hover;

        [Category("모양"), Description("true면 초록으로 채움, false면 테두리만"), DefaultValue(false)]
        public bool Primary
        {
            get { return _primary; }
            set { _primary = value; Invalidate(); }
        }

        public PillButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Card;
            Font = new Font("맑은 고딕", 9.5f, FontStyle.Bold);
            Cursor = Cursors.Hand;
            Size = new Size(84, 34);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnEnabledChanged(EventArgs e) { Cursor = Enabled ? Cursors.Hand : Cursors.Default; Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            var r = new RectangleF(1, 1, Width - 3, Height - 3);
            int alpha = Enabled ? 255 : 90;
            Color green = Color.FromArgb(alpha, _hover && Enabled ? Theme.AccentHover : Theme.Accent);

            using (var path = Theme.Round(r, r.Height / 2))
            {
                if (_primary)
                {
                    using (var b = new SolidBrush(green)) g.FillPath(b, path);
                }
                else
                {
                    if (_hover && Enabled)
                        using (var hb = new SolidBrush(Color.FromArgb(25, Theme.Accent))) g.FillPath(hb, path);
                    using (var pen = new Pen(green, 1.5f)) g.DrawPath(pen, path);
                }
            }

            TextRenderer.DrawText(g, Text, Font, ClientRectangle,
                _primary ? Color.FromArgb(alpha, Color.White) : green,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    public static class Theme
    {
        public static readonly Color Card = Color.FromArgb(250, 248, 245);
        public static readonly Color Ink = Color.FromArgb(28, 32, 30);
        public static readonly Color Muted = Color.FromArgb(105, 112, 108);
        public static readonly Color Accent = Color.FromArgb(47, 93, 80);
        public static readonly Color AccentHover = Color.FromArgb(38, 77, 66);

        public static GraphicsPath Round(RectangleF r, float radius)
        {
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            var p = new GraphicsPath();
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    public class SubjectInputDialog : Form
    {
        private readonly StudyTimer _timer;
        private readonly TextBox txtName = new TextBox();
        private readonly Label lblError = new Label();

        public string SubjectName { get; private set; }

        public SubjectInputDialog(StudyTimer timer)
        {
            _timer = timer;

            Text = "새 과목 추가";
            ClientSize = new Size(280, 150);
            BackColor = Theme.Card;
            Font = new Font("맑은 고딕", 10f);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;

            var lbl = new Label { Text = "과목 이름", AutoSize = true, Location = new Point(20, 18), ForeColor = Theme.Ink };

            txtName.SetBounds(20, 42, 240, 28);
            txtName.MaxLength = StudyTimer.MaxSubjectLength;
            txtName.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; TryOk(); }
                if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; }
            };
            txtName.TextChanged += (s, e) => lblError.Text = "";

            lblError.SetBounds(20, 72, 240, 20);
            lblError.ForeColor = Color.FromArgb(190, 60, 50);
            lblError.Font = new Font("맑은 고딕", 8.5f);

            var btnOk = new PillButton { Primary = true, Text = "추가" };
            btnOk.SetBounds(96, 100, 78, 32);
            btnOk.Click += (s, e) => TryOk();

            var btnCancel = new PillButton { Text = "취소" };
            btnCancel.SetBounds(182, 100, 78, 32);
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { lbl, txtName, lblError, btnOk, btnCancel });
            Shown += (s, e) => txtName.Focus();
        }

        private void TryOk()
        {
            string error;
            if (!_timer.TryAddSubject(txtName.Text, out error))
            {
                lblError.Text = error;
                return;
            }
            SubjectName = txtName.Text.Trim();
            DialogResult = DialogResult.OK;
        }
    }
}

