using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace TaskMain
{
    public static class StudyStats
    {
        public static readonly string[] DayNames = { "월", "화", "수", "목", "금", "토", "일" };

        public static int DayIndex(DateTime date)
        {
            return ((int)date.DayOfWeek + 6) % 7;
        }

        public static int[] AddRunning(int[] weekSeconds, int runningSeconds, DateTime now)
        {
            var result = new int[7];
            if (weekSeconds != null)
                Array.Copy(weekSeconds, result, Math.Min(7, weekSeconds.Length));
            if (runningSeconds > 0)
                result[DayIndex(now)] += runningSeconds;
            return result;
        }

        public static int GoalPercent(int todaySeconds, int goalMinutes)
        {
            if (goalMinutes <= 0 || todaySeconds <= 0) return 0;
            long percent = (long)todaySeconds * 100 / (goalMinutes * 60L);
            return (int)Math.Min(100, percent);
        }

        public static string ToKorean(int seconds)
        {
            int h = seconds / 3600, m = seconds / 60 % 60;
            if (h > 0 && m > 0) return h + "시간 " + m + "분";
            if (h > 0) return h + "시간";
            return m + "분";
        }

        public static string RemainText(int todaySeconds, int goalMinutes)
        {
            int remain = goalMinutes * 60 - todaySeconds;
            if (remain <= 0) return "목표 달성!";
            return "약 " + ToKorean(remain) + " 남음";
        }
    }

    [ToolboxItem(true)]
    public class StudyGraphPanel : Control
    {
        private int[] _week = new int[7];
        private int _goalMinutes = 360;

        private Color _accent = Color.FromArgb(47, 93, 80);
        private Color _todayColor = Color.FromArgb(184, 122, 61);
        private Color _pastColor = Color.FromArgb(138, 160, 150);
        private Color _trackColor = Color.FromArgb(214, 220, 215);
        private Color _inkColor = Color.FromArgb(28, 32, 30);
        private Color _mutedColor = Color.FromArgb(95, 102, 98);

        [Category("모양"), Description("하루 목표(분)"), DefaultValue(360)]
        public int GoalMinutes
        {
            get { return _goalMinutes; }
            set { _goalMinutes = Math.Max(1, value); Invalidate(); }
        }

        [Category("모양"), Description("목표 막대, 퍼센트 색")]
        public Color AccentColor
        {
            get { return _accent; }
            set { _accent = value; Invalidate(); }
        }

        [Category("모양"), Description("오늘 막대 색")]
        public Color TodayColor
        {
            get { return _todayColor; }
            set { _todayColor = value; Invalidate(); }
        }

        [Category("모양"), Description("지난 요일 막대 색")]
        public Color PastColor
        {
            get { return _pastColor; }
            set { _pastColor = value; Invalidate(); }
        }

        public StudyGraphPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(250, 248, 245);
            Font = new Font("맑은 고딕", 9f);
            Size = new Size(288, 300);
        }

        public int TodaySeconds
        {
            get { return _week[StudyStats.DayIndex(DateTime.Today)]; }
        }

        public int WeekTotalSeconds
        {
            get { return _week.Sum(); }
        }

        public int GoalPercent
        {
            get { return StudyStats.GoalPercent(TodaySeconds, _goalMinutes); }
        }

        public void SetData(int[] weekSeconds)
        {
            _week = StudyStats.AddRunning(weekSeconds, 0, DateTime.Now)
                              .Select(s => Math.Max(0, s)).ToArray();
            Invalidate();
        }

        public void SetData(int[] weekSeconds, int goalMinutes)
        {
            _goalMinutes = Math.Max(1, goalMinutes);
            SetData(weekSeconds);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);

            int todayIdx = StudyStats.DayIndex(DateTime.Today);
            int[] week = _week;
            int today = week[todayIdx];
            if (DesignMode)
            {
                todayIdx = 1;
                today = 15120;
                week = new[] { 8640, 15120, 0, 0, 0, 0, 0 };
            }

            float pad = 18;
            float w = Width - pad * 2;
            int percent = StudyStats.GoalPercent(today, _goalMinutes);

            using (var titleFont = new Font("맑은 고딕", 11f, FontStyle.Bold))
            using (var bigFont = new Font("Segoe UI", 16f, FontStyle.Bold))
            using (var smallBold = new Font("맑은 고딕", 8.5f, FontStyle.Bold))
            using (var ink = new SolidBrush(_inkColor))
            using (var muted = new SolidBrush(_mutedColor))
            using (var accent = new SolidBrush(_accent))
            using (var track = new SolidBrush(_trackColor))
            using (var todayBrush = new SolidBrush(_todayColor))
            using (var pastBrush = new SolidBrush(_pastColor))
            {
                float y = 16;
                g.DrawString("오늘 목표", titleFont, ink, pad - 2, y + 4);
                DrawRight(g, percent + "%", bigFont, accent, pad + w, y - 2);

                y = 50;
                using (var p = Round(new RectangleF(pad, y, w, 8), 4)) g.FillPath(track, p);
                if (percent > 0)
                    using (var p = Round(new RectangleF(pad, y, Math.Max(8, w * percent / 100f), 8), 4))
                        g.FillPath(accent, p);

                y = 66;
                g.DrawString("목표 " + StudyStats.ToKorean(_goalMinutes * 60), Font, muted, pad - 2, y);
                DrawRight(g, StudyStats.RemainText(today, _goalMinutes), Font, muted, pad + w, y);

                y = 98;
                using (var line = new Pen(Color.FromArgb(225, 222, 216), 1)) g.DrawLine(line, pad, y, pad + w, y);

                y = 112;
                g.DrawString("이번 주", titleFont, ink, pad - 2, y);
                DrawRight(g, "합계 " + StudyStats.ToKorean(week.Sum()), Font, muted, pad + w, y + 3);

                float labelH = 24;
                float chartTop = 156;
                float baseY = Height - labelH - 6;
                float chartH = baseY - chartTop;
                int goalSec = _goalMinutes * 60;
                int max = Math.Max(goalSec, Math.Max(1, week.Max()));

                float goalY = baseY - chartH * goalSec / max;
                using (var dash = new Pen(Color.FromArgb(160, 165, 160), 1) { DashStyle = DashStyle.Dash })
                    g.DrawLine(dash, pad, goalY, pad + w, goalY);

                float gap = 10;
                float barW = (w - gap * 6) / 7;
                string todayLabel = null;
                RectangleF todayLabelRect = RectangleF.Empty;

                for (int i = 0; i < 7; i++)
                {
                    float x = pad + i * (barW + gap);
                    float barH = chartH * week[i] / max;
                    bool isToday = i == todayIdx;

                    if (week[i] > 0 && barH >= 3)
                    {
                        Brush b = isToday ? todayBrush : pastBrush;
                        using (var p = TopRound(new RectangleF(x, baseY - barH, barW, barH), 4)) g.FillPath(b, p);
                    }
                    else
                    {
                        using (var p = Round(new RectangleF(x, baseY - 3, barW, 3), 1.5f)) g.FillPath(track, p);
                    }

                    if (isToday && week[i] >= 60)
                    {
                        todayLabel = StudyStats.ToKorean(week[i]);
                        SizeF ts = g.MeasureString(todayLabel, smallBold);
                        float tx = Math.Max(pad - 4, Math.Min(pad + w - ts.Width + 4, x + (barW - ts.Width) / 2));
                        float ty = Math.Max(chartTop - ts.Height, baseY - barH - ts.Height - 2);
                        todayLabelRect = new RectangleF(tx, ty, ts.Width, ts.Height);
                    }

                    Font df = isToday ? smallBold : Font;
                    SizeF ds = g.MeasureString(StudyStats.DayNames[i], df);
                    g.DrawString(StudyStats.DayNames[i], df, isToday ? ink : muted,
                                 x + (barW - ds.Width) / 2, baseY + 6);
                }

                if (todayLabel != null)
                {
                    var bg = RectangleF.Inflate(todayLabelRect, 2, 0);
                    using (var p = Round(bg, 4))
                    using (var bgBrush = new SolidBrush(BackColor))
                        g.FillPath(bgBrush, p);
                    g.DrawString(todayLabel, smallBold, todayBrush, todayLabelRect.X, todayLabelRect.Y);
                }
            }
        }

        private static void DrawRight(Graphics g, string text, Font font, Brush brush, float right, float y)
        {
            SizeF s = g.MeasureString(text, font);
            g.DrawString(text, font, brush, right - s.Width + 2, y);
        }

        private static GraphicsPath Round(RectangleF r, float radius)
        {
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            var p = new GraphicsPath();
            if (d < 1) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        private static GraphicsPath TopRound(RectangleF r, float radius)
        {
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            var p = new GraphicsPath();
            if (d < 1) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddLine(r.Right, r.Y + d / 2, r.Right, r.Bottom);
            p.AddLine(r.Right, r.Bottom, r.X, r.Bottom);
            p.CloseFigure();
            return p;
        }
    }
}
