namespace TaskBasedStateMachineTest
{
    partial class Test
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonDisplay = new System.Windows.Forms.Button();
            this.buttonRun = new System.Windows.Forms.Button();
            this.mRichTextBox = new System.Windows.Forms.RichTextBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonDisplay
            // 
            this.buttonDisplay.Location = new System.Drawing.Point(30, 29);
            this.buttonDisplay.Name = "buttonDisplay";
            this.buttonDisplay.Size = new System.Drawing.Size(161, 41);
            this.buttonDisplay.TabIndex = 0;
            this.buttonDisplay.Text = "Display";
            this.buttonDisplay.UseVisualStyleBackColor = true;
            this.buttonDisplay.Click += new System.EventHandler(this.buttonDisplay_Click);
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(215, 29);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(149, 41);
            this.buttonRun.TabIndex = 1;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // mRichTextBox
            // 
            this.mRichTextBox.Location = new System.Drawing.Point(30, 88);
            this.mRichTextBox.Name = "mRichTextBox";
            this.mRichTextBox.Size = new System.Drawing.Size(746, 436);
            this.mRichTextBox.TabIndex = 2;
            this.mRichTextBox.Text = "";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(727, 29);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(41, 41);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "X";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // Test
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(806, 545);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.mRichTextBox);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.buttonDisplay);
            this.Name = "Test";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonDisplay;
        private System.Windows.Forms.Button buttonRun;
        public System.Windows.Forms.RichTextBox mRichTextBox;
        private System.Windows.Forms.Button buttonCancel;
    }
}

