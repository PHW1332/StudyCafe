namespace TaskMain
{
    partial class FocusModeControl
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblFocusStatus = new System.Windows.Forms.Label();
            this.lblGuide = new System.Windows.Forms.Label();
            this.btnFocusToggle = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblFocusStatus
            // 
            this.lblFocusStatus.Font = new System.Drawing.Font("굴림", 10F);
            this.lblFocusStatus.Location = new System.Drawing.Point(234, 38);
            this.lblFocusStatus.Name = "lblFocusStatus";
            this.lblFocusStatus.Size = new System.Drawing.Size(418, 40);
            this.lblFocusStatus.TabIndex = 1;
            this.lblFocusStatus.Text = "현재 상태: OFF (자유 모드)";
            this.lblFocusStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblGuide
            // 
            this.lblGuide.Location = new System.Drawing.Point(106, 101);
            this.lblGuide.Name = "lblGuide";
            this.lblGuide.Size = new System.Drawing.Size(420, 60);
            this.lblGuide.TabIndex = 2;
            this.lblGuide.Text = "집중 모드가 켜져 있을 때 다른 창으로 이동하면\r\n타이머가 자동으로 일시정지됩니다.\r\n\r\n\r\n";
            this.lblGuide.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnFocusToggle
            // 
            this.btnFocusToggle.Location = new System.Drawing.Point(28, 34);
            this.btnFocusToggle.Name = "btnFocusToggle";
            this.btnFocusToggle.Size = new System.Drawing.Size(200, 50);
            this.btnFocusToggle.TabIndex = 3;
            this.btnFocusToggle.Text = "집중 모드 ON";
            this.btnFocusToggle.UseVisualStyleBackColor = true;
            this.btnFocusToggle.Click += new System.EventHandler(this.btnFocusToggle_Click);
            // 
            // FocusModeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnFocusToggle);
            this.Controls.Add(this.lblGuide);
            this.Controls.Add(this.lblFocusStatus);
            this.Name = "FocusModeControl";
            this.Size = new System.Drawing.Size(660, 175);
            this.Load += new System.EventHandler(this.FocusModeControl_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblFocusStatus;
        private System.Windows.Forms.Label lblGuide;
        private System.Windows.Forms.Button btnFocusToggle;
    }
}
