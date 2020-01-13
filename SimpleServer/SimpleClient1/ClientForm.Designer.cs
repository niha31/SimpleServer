namespace SimpleClient
{
    partial class ClientForm
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
            this.SubmitButton = new System.Windows.Forms.Button();
            this.SubmitText = new System.Windows.Forms.RichTextBox();
            this.OutText = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.nickName = new System.Windows.Forms.RichTextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // SubmitButton
            // 
            this.SubmitButton.Location = new System.Drawing.Point(880, 446);
            this.SubmitButton.Margin = new System.Windows.Forms.Padding(4);
            this.SubmitButton.Name = "SubmitButton";
            this.SubmitButton.Size = new System.Drawing.Size(183, 95);
            this.SubmitButton.TabIndex = 0;
            this.SubmitButton.Text = "Submit";
            this.SubmitButton.UseVisualStyleBackColor = true;
            this.SubmitButton.Click += new System.EventHandler(this.SubmitButton_Click);
            // 
            // SubmitText
            // 
            this.SubmitText.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.SubmitText.Location = new System.Drawing.Point(16, 446);
            this.SubmitText.Margin = new System.Windows.Forms.Padding(4);
            this.SubmitText.Name = "SubmitText";
            this.SubmitText.Size = new System.Drawing.Size(856, 95);
            this.SubmitText.TabIndex = 1;
            this.SubmitText.Text = "";
            this.SubmitText.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged_1);
            // 
            // OutText
            // 
            this.OutText.BackColor = System.Drawing.Color.DarkGray;
            this.OutText.ForeColor = System.Drawing.Color.Black;
            this.OutText.Location = new System.Drawing.Point(16, 16);
            this.OutText.Margin = new System.Windows.Forms.Padding(4);
            this.OutText.Name = "OutText";
            this.OutText.ReadOnly = true;
            this.OutText.Size = new System.Drawing.Size(856, 422);
            this.OutText.TabIndex = 2;
            this.OutText.Text = "";
            this.OutText.TextChanged += new System.EventHandler(this.richTextBox2_TextChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(880, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(183, 71);
            this.button1.TabIndex = 3;
            this.button1.Text = "QUIT";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // nickName
            // 
            this.nickName.BackColor = System.Drawing.SystemColors.ControlDark;
            this.nickName.Location = new System.Drawing.Point(880, 140);
            this.nickName.Name = "nickName";
            this.nickName.Size = new System.Drawing.Size(183, 34);
            this.nickName.TabIndex = 4;
            this.nickName.Text = "";
            this.nickName.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(880, 186);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(183, 41);
            this.button2.TabIndex = 5;
            this.button2.Text = "Set Nick Name";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGray;
            this.ClientSize = new System.Drawing.Size(1067, 554);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.nickName);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.OutText);
            this.Controls.Add(this.SubmitText);
            this.Controls.Add(this.SubmitButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ClientForm";
            this.Text = "2102";
            this.Load += new System.EventHandler(this.ClientForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button SubmitButton;
        private System.Windows.Forms.RichTextBox SubmitText;
        private System.Windows.Forms.RichTextBox OutText;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox nickName;
        private System.Windows.Forms.Button button2;
    }
}

