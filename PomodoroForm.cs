using System;
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
    public partial class PomodoroForm : Form
    {
        private int focusSeconds = 25 * 60;     // 기본 집중 25분
        private int restSeconds = 5 * 60;       // 기본 휴식 5분
        private int remainingSeconds = 25 * 60; // 현재 남은 시간
        public int RemainingSeconds
        {
            get { return remainingSeconds; }
        }
        private bool isRunning = false;   // 타이머 동작 여부
        public bool IsRunning
        {
            get { return isRunning; }
        }
            
        private bool isRestMode = false; // false: 집중 단계, true: 휴식 단계
        public bool IsRestMode
        {
            get { return isRestMode; } 
        }
        private int currentCycle = 1;           // 현재 회차 (1 ~ 4회차)
        
        public int CurrentCycle
        {
            get { return currentCycle; }
        }
        public event Action<bool> PhaseChanged;
        public PomodoroForm()
        {
            InitializeComponent();
            UpdateUI();
        }

        private void PomodoroForm_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (remainingSeconds > 0)
            {
                remainingSeconds--;
                UpdateTimerLabel();
            }
            else
            {
                HandleTimerCompletion();
            }
        }

        private void btnMinus5_Click(object sender, EventArgs e)
        {
            AdjustMinutes(-5);
        }

        private void btnPlus5_Click(object sender, EventArgs e)
        {
            AdjustMinutes(5);
        }

        private void btnToggle_Click(object sender, EventArgs e)
        {
            Toggle();
        }

        private void btnSkipRest_Click(object sender, EventArgs e)
        {
            SkipRest();
        }


        public void Toggle()
        {
            isRunning = !isRunning;

            if (isRunning)
            {
                timer1.Start();
                btnToggle.Text = "OFF";
            }
            else
            {
                timer1.Stop();
                btnToggle.Text = "ON";
            }
        }

        public void AdjustMinutes(int deltaMinutes)
        {
            if (!isRestMode)
            {
                int updated = remainingSeconds + (deltaMinutes * 60);
                if (updated >= 300) // 최소 5분(300초) 유지
                {
                    remainingSeconds = updated;
                    UpdateTimerLabel();
                }
            }
        }

        public void SkipRest()
        {
            if (isRestMode)
            {
                MoveToNextCycle();
            }
        }

        public void SetDurations(int focusMinutes, int restMinutes)
        {
            focusSeconds = focusMinutes * 60;
            restSeconds = restMinutes * 60;

            if (!isRunning && !isRestMode)
            {
                remainingSeconds = focusSeconds;
                UpdateTimerLabel();
            }
        }

        private void HandleTimerCompletion()
        {
            timer1.Stop();
            isRunning = false;
            btnToggle.Text = "ON";

            if (!isRestMode)
            {
                // 집중 종료 -> 휴식 시작
                isRestMode = true;
                PhaseChanged?.Invoke(isRestMode);
                remainingSeconds = restSeconds;
                System.Media.SystemSounds.Beep.Play();
            }
            else
            {
                // 휴식 종료 -> 다음 회차 집중 시작
                MoveToNextCycle();
            }

            UpdateUI();
        }

        private void MoveToNextCycle()
        {
            isRestMode = false;
            PhaseChanged?.Invoke(isRestMode);
            currentCycle = (currentCycle < 4) ? currentCycle + 1 : 1;
            remainingSeconds = focusSeconds;
            System.Media.SystemSounds.Asterisk.Play();
            UpdateUI();
        }

        private void UpdateUI()
        {
            UpdateTimerLabel();
            string modeText = isRestMode ? "휴식 5분" : "집중 중";
            lblStatus.Text = $"{modeText} · {currentCycle} / 4회차";
            btnSkipRest.Enabled = isRestMode;
        }

        private void UpdateTimerLabel()
        {
            TimeSpan time = TimeSpan.FromSeconds(remainingSeconds);
            lblTimerDisplay.Text = time.ToString(@"mm\:ss");
        }       
    }
}
