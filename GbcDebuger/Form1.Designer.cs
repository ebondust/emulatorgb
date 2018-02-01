namespace GbcDebuger
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
            this.Registers = new System.Windows.Forms.ListBox();
            this.Canvas = new System.Windows.Forms.PictureBox();
            this.Dipsach = new System.Windows.Forms.Timer(this.components);
            this.BButton = new System.Windows.Forms.Button();
            this.AButton = new System.Windows.Forms.Button();
            this.StartButton = new System.Windows.Forms.Button();
            this.LeftButton = new System.Windows.Forms.Button();
            this.RightButton = new System.Windows.Forms.Button();
            this.DownButton = new System.Windows.Forms.Button();
            this.UpButton = new System.Windows.Forms.Button();
            this.SelectButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.Canvas)).BeginInit();
            this.SuspendLayout();
            // 
            // Registers
            // 
            this.Registers.FormattingEnabled = true;
            this.Registers.Location = new System.Drawing.Point(352, 10);
            this.Registers.Name = "Registers";
            this.Registers.Size = new System.Drawing.Size(153, 290);
            this.Registers.TabIndex = 2;
            // 
            // Canvas
            // 
            this.Canvas.Location = new System.Drawing.Point(26, 12);
            this.Canvas.Name = "Canvas";
            this.Canvas.Size = new System.Drawing.Size(320, 288);
            this.Canvas.TabIndex = 3;
            this.Canvas.TabStop = false;
            // 
            // Dipsach
            // 
            this.Dipsach.Interval = 10;
            // 
            // BButton
            // 
            this.BButton.Location = new System.Drawing.Point(308, 306);
            this.BButton.Name = "BButton";
            this.BButton.Size = new System.Drawing.Size(38, 34);
            this.BButton.TabIndex = 11;
            this.BButton.Text = "B";
            this.BButton.UseVisualStyleBackColor = true;
            this.BButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BButton_MouseDown);
            this.BButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BButton_MouseUp);
            // 
            // AButton
            // 
            this.AButton.Location = new System.Drawing.Point(264, 306);
            this.AButton.Name = "AButton";
            this.AButton.Size = new System.Drawing.Size(38, 34);
            this.AButton.TabIndex = 12;
            this.AButton.Text = "A";
            this.AButton.UseVisualStyleBackColor = true;
            this.AButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AButton_MouseDown);
            this.AButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.AButton_MouseUp);
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(252, 346);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(50, 34);
            this.StartButton.TabIndex = 13;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StartButton_MouseDown);
            this.StartButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.StartButton_MouseUp);
            // 
            // LeftButton
            // 
            this.LeftButton.Location = new System.Drawing.Point(26, 325);
            this.LeftButton.Name = "LeftButton";
            this.LeftButton.Size = new System.Drawing.Size(44, 34);
            this.LeftButton.TabIndex = 14;
            this.LeftButton.Text = "Left";
            this.LeftButton.UseVisualStyleBackColor = true;
            this.LeftButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LeftButton_MouseDown);
            this.LeftButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LeftButton_MouseUp);
            // 
            // RightButton
            // 
            this.RightButton.Location = new System.Drawing.Point(126, 325);
            this.RightButton.Name = "RightButton";
            this.RightButton.Size = new System.Drawing.Size(42, 34);
            this.RightButton.TabIndex = 15;
            this.RightButton.Text = "Right";
            this.RightButton.UseVisualStyleBackColor = true;
            this.RightButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RightButton_MouseDown);
            this.RightButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RightButton_MouseUp);
            // 
            // DownButton
            // 
            this.DownButton.Location = new System.Drawing.Point(76, 346);
            this.DownButton.Name = "DownButton";
            this.DownButton.Size = new System.Drawing.Size(44, 34);
            this.DownButton.TabIndex = 16;
            this.DownButton.Text = "Down";
            this.DownButton.UseVisualStyleBackColor = true;
            // 
            // UpButton
            // 
            this.UpButton.Location = new System.Drawing.Point(76, 306);
            this.UpButton.Name = "UpButton";
            this.UpButton.Size = new System.Drawing.Size(44, 34);
            this.UpButton.TabIndex = 17;
            this.UpButton.Text = "Up";
            this.UpButton.UseVisualStyleBackColor = true;
            // 
            // SelectButton
            // 
            this.SelectButton.Location = new System.Drawing.Point(308, 346);
            this.SelectButton.Name = "SelectButton";
            this.SelectButton.Size = new System.Drawing.Size(50, 34);
            this.SelectButton.TabIndex = 18;
            this.SelectButton.Text = "Select";
            this.SelectButton.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 386);
            this.Controls.Add(this.SelectButton);
            this.Controls.Add(this.UpButton);
            this.Controls.Add(this.DownButton);
            this.Controls.Add(this.RightButton);
            this.Controls.Add(this.LeftButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.AButton);
            this.Controls.Add(this.BButton);
            this.Controls.Add(this.Canvas);
            this.Controls.Add(this.Registers);
            this.Name = "Form1";
            this.Text = "Emulator GB";
            ((System.ComponentModel.ISupportInitialize)(this.Canvas)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ListBox Registers;
        private System.Windows.Forms.PictureBox Canvas;
        private System.Windows.Forms.Timer Dipsach;
        private System.Windows.Forms.Button BButton;
        private System.Windows.Forms.Button AButton;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button LeftButton;
        private System.Windows.Forms.Button RightButton;
        private System.Windows.Forms.Button DownButton;
        private System.Windows.Forms.Button UpButton;
        private System.Windows.Forms.Button SelectButton;
    }
}

