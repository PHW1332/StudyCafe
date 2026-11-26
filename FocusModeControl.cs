using System;
using System.Drawing;
using System.Windows.Forms;

namespace TaskMain
{
    public partial class FocusModeControl : UserControl
    {
        // 집중 모드 활성화 여부
        private bool isFocusMode = false;

        // 메인 Form1에 신호
        public event Action OnWindowLeftDuringFocus;
        public event Action<bool> OnFocusModeChanged;

        public FocusModeControl()
        {
            InitializeComponent();
            UpdateUI();
        }

        // 컨트롤이 Form 위에 올라갈 때 부모 Form의 창 이탈(Deactivate) 이벤트 연결
        private void FocusModeControl_Load(object sender, EventArgs e)
        {
            if (this.ParentForm != null)
            {
                this.ParentForm.Deactivate += ParentForm_Deactivate;
            }
        }

        //집중 모드 버튼 클릭
        private void btnFocusToggle_Click(object sender, EventArgs e)
        {
            ToggleFocusMode();
        }

        //메인 폼에서 다른 창으로 넘어갔을 때 자동 실행
        private void ParentForm_Deactivate(object sender, EventArgs e)
        {
            if (isFocusMode)
            {
                OnWindowLeftDuringFocus?.Invoke();

                lblFocusStatus.Text = "⚠️ 다른 창 이동 감지! 타이머 일시정지됨";
                lblFocusStatus.ForeColor = Color.Crimson;
            }
        }

        //외부(메인 Form1)에서도 호출 가능하도록 분리한 public 메서드

        public void ToggleFocusMode()
        {
            isFocusMode = !isFocusMode;
            OnFocusModeChanged?.Invoke(isFocusMode);
            UpdateUI();
        }

        public bool IsFocusModeActive()
        {
            return isFocusMode;
        }

        //화면 상태 갱신

        private void UpdateUI()
        {
            if (isFocusMode)
            {
                lblFocusStatus.Text = "현재 상태: ON (집중 감시 중)";
                lblFocusStatus.ForeColor = Color.DarkGreen;
                btnFocusToggle.Text = "집중 모드 OFF";
            }
            else
            {
                lblFocusStatus.Text = "현재 상태: OFF (자유 모드)";
                lblFocusStatus.ForeColor = Color.Black;
                btnFocusToggle.Text = "집중 모드 ON";
            }
        }
    }
}
