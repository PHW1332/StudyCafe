namespace TaskMain
{
    partial class PomodoroForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnMinus5 = new System.Windows.Forms.Button();
            this.btnPlus5 = new System.Windows.Forms.Button();
            this.lblTimerDisplay = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnToggle = new System.Windows.Forms.Button();
            this.btnSkipRest = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // btnMinus5
            // 
            this.btnMinus5.Font = new System.Drawing.Font("굴림", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnMinus5.Location = new System.Drawing.Point(45, 115);
            this.btnMinus5.Name = "btnMinus5";
            this.btnMinus5.Size = new System.Drawing.Size(65, 65);
            this.btnMinus5.TabIndex = 0;
            this.btnMinus5.Text = "-5";
            this.btnMinus5.UseVisualStyleBackColor = true;
            this.btnMinus5.Click += new System.EventHandler(this.btnMinus5_Click);
            // 
            // btnPlus5
            // 
            this.btnPlus5.Font = new System.Drawing.Font("굴림", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnPlus5.Location = new System.Drawing.Point(395, 115);
            this.btnPlus5.Name = "btnPlus5";
            this.btnPlus5.Size = new System.Drawing.Size(65, 65);
            this.btnPlus5.TabIndex = 1;
            this.btnPlus5.Text = "+5";
            this.btnPlus5.UseVisualStyleBackColor = true;
            this.btnPlus5.Click += new System.EventHandler(this.btnPlus5_Click);
            // 
            // lblTimerDisplay
            // 
            this.lblTimerDisplay.Font = new System.Drawing.Font("굴림", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblTimerDisplay.Location = new System.Drawing.Point(135, 100);
            this.lblTimerDisplay.Name = "lblTimerDisplay";
            this.lblTimerDisplay.Size = new System.Drawing.Size(234, 90);
            this.lblTimerDisplay.TabIndex = 2;
            this.lblTimerDisplay.Text = "25:00";
            this.lblTimerDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblStatus.Location = new System.Drawing.Point(100, 210);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(304, 30);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "집중 중 · 1 / 4회차";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnToggle
            // 
            this.btnToggle.Font = new System.Drawing.Font("굴림", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnToggle.Location = new System.Drawing.Point(75, 275);
            this.btnToggle.Name = "btnToggle";
            this.btnToggle.Size = new System.Drawing.Size(155, 65);
            this.btnToggle.TabIndex = 4;
            this.btnToggle.Text = "ON";
            this.btnToggle.UseVisualStyleBackColor = true;
            this.btnToggle.Click += new System.EventHandler(this.btnToggle_Click);
            // 
            // btnSkipRest
            // 
            this.btnSkipRest.Font = new System.Drawing.Font("굴림", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSkipRest.Location = new System.Drawing.Point(275, 275);
            this.btnSkipRest.Name = "btnSkipRest";
            this.btnSkipRest.Size = new System.Drawing.Size(155, 65);
            this.btnSkipRest.TabIndex = 5;
            this.btnSkipRest.Text = "휴식 건너뛰기";
            this.btnSkipRest.UseVisualStyleBackColor = true;
            this.btnSkipRest.Click += new System.EventHandler(this.btnSkipRest_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // PomodoroForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 364);
            this.Controls.Add(this.btnSkipRest);
            this.Controls.Add(this.btnToggle);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblTimerDisplay);
            this.Controls.Add(this.btnPlus5);
            this.Controls.Add(this.btnMinus5);
            this.Name = "PomodoroForm";
            this.Text = "뽀모도로";
            this.Load += new System.EventHandler(this.PomodoroForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnMinus5;
        private System.Windows.Forms.Button btnPlus5;
        private System.Windows.Forms.Label lblTimerDisplay;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnToggle;
        private System.Windows.Forms.Button btnSkipRest;
        private System.Windows.Forms.Timer timer1;
    }
}