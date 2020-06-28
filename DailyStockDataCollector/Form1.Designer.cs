namespace DailyStockDataCollector
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.axKHOpenAPI1 = new AxKHOpenAPILib.AxKHOpenAPI();
            this.listBox = new System.Windows.Forms.ListBox();
            this.투자자별매매동향Button = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.dateTextBox = new System.Windows.Forms.TextBox();
            this.프로그램매매동향Button = new System.Windows.Forms.Button();
            this.여기서부터다시시작textBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.주식기본정보Button = new System.Windows.Forms.Button();
            this.일별가격정보Button = new System.Windows.Forms.Button();
            this.종목명저장Button = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.progressLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.axKHOpenAPI1)).BeginInit();
            this.SuspendLayout();
            // 
            // axKHOpenAPI1
            // 
            this.axKHOpenAPI1.Enabled = true;
            this.axKHOpenAPI1.Location = new System.Drawing.Point(39, 428);
            this.axKHOpenAPI1.Name = "axKHOpenAPI1";
            this.axKHOpenAPI1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axKHOpenAPI1.OcxState")));
            this.axKHOpenAPI1.Size = new System.Drawing.Size(55, 17);
            this.axKHOpenAPI1.TabIndex = 0;
            // 
            // listBox
            // 
            this.listBox.FormattingEnabled = true;
            this.listBox.Location = new System.Drawing.Point(55, 157);
            this.listBox.Name = "listBox";
            this.listBox.Size = new System.Drawing.Size(570, 173);
            this.listBox.TabIndex = 1;
            // 
            // 투자자별매매동향Button
            // 
            this.투자자별매매동향Button.Location = new System.Drawing.Point(636, 27);
            this.투자자별매매동향Button.Name = "투자자별매매동향Button";
            this.투자자별매매동향Button.Size = new System.Drawing.Size(152, 23);
            this.투자자별매매동향Button.TabIndex = 2;
            this.투자자별매매동향Button.Text = "투자자별매매동향 저장";
            this.투자자별매매동향Button.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // dateTextBox
            // 
            this.dateTextBox.Location = new System.Drawing.Point(39, 30);
            this.dateTextBox.Name = "dateTextBox";
            this.dateTextBox.Size = new System.Drawing.Size(100, 20);
            this.dateTextBox.TabIndex = 4;
            // 
            // 프로그램매매동향Button
            // 
            this.프로그램매매동향Button.Location = new System.Drawing.Point(636, 65);
            this.프로그램매매동향Button.Name = "프로그램매매동향Button";
            this.프로그램매매동향Button.Size = new System.Drawing.Size(152, 23);
            this.프로그램매매동향Button.TabIndex = 5;
            this.프로그램매매동향Button.Text = "프로그램매매동향 저장";
            this.프로그램매매동향Button.UseVisualStyleBackColor = true;
            // 
            // 여기서부터다시시작textBox
            // 
            this.여기서부터다시시작textBox.Location = new System.Drawing.Point(39, 81);
            this.여기서부터다시시작textBox.Name = "여기서부터다시시작textBox";
            this.여기서부터다시시작textBox.Size = new System.Drawing.Size(100, 20);
            this.여기서부터다시시작textBox.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "이 종목부터 다시 시작";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(39, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "yyyyMMdd";
            // 
            // 주식기본정보Button
            // 
            this.주식기본정보Button.Location = new System.Drawing.Point(636, 104);
            this.주식기본정보Button.Name = "주식기본정보Button";
            this.주식기본정보Button.Size = new System.Drawing.Size(152, 23);
            this.주식기본정보Button.TabIndex = 9;
            this.주식기본정보Button.Text = "주식기본정보 저장";
            this.주식기본정보Button.UseVisualStyleBackColor = true;
            // 
            // 일별가격정보Button
            // 
            this.일별가격정보Button.Location = new System.Drawing.Point(636, 143);
            this.일별가격정보Button.Name = "일별가격정보Button";
            this.일별가격정보Button.Size = new System.Drawing.Size(152, 23);
            this.일별가격정보Button.TabIndex = 10;
            this.일별가격정보Button.Text = "일별가격정보 저장";
            this.일별가격정보Button.UseVisualStyleBackColor = true;
            // 
            // 종목명저장Button
            // 
            this.종목명저장Button.Location = new System.Drawing.Point(636, 181);
            this.종목명저장Button.Name = "종목명저장Button";
            this.종목명저장Button.Size = new System.Drawing.Size(152, 23);
            this.종목명저장Button.TabIndex = 11;
            this.종목명저장Button.Text = "종목명 저장";
            this.종목명저장Button.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(55, 359);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(570, 23);
            this.progressBar.TabIndex = 12;
            // 
            // progressLabel
            // 
            this.progressLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.progressLabel.Location = new System.Drawing.Point(55, 385);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(570, 40);
            this.progressLabel.TabIndex = 13;
            this.progressLabel.Text = "로그인중...";
            this.progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.progressLabel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.종목명저장Button);
            this.Controls.Add(this.일별가격정보Button);
            this.Controls.Add(this.주식기본정보Button);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.여기서부터다시시작textBox);
            this.Controls.Add(this.프로그램매매동향Button);
            this.Controls.Add(this.dateTextBox);
            this.Controls.Add(this.투자자별매매동향Button);
            this.Controls.Add(this.listBox);
            this.Controls.Add(this.axKHOpenAPI1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.axKHOpenAPI1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AxKHOpenAPILib.AxKHOpenAPI axKHOpenAPI1;
        private System.Windows.Forms.ListBox listBox;
        private System.Windows.Forms.Button 투자자별매매동향Button;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.TextBox dateTextBox;
        private System.Windows.Forms.Button 프로그램매매동향Button;
        private System.Windows.Forms.TextBox 여기서부터다시시작textBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button 주식기본정보Button;
        private System.Windows.Forms.Button 일별가격정보Button;
        private System.Windows.Forms.Button 종목명저장Button;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label progressLabel;
    }
}

