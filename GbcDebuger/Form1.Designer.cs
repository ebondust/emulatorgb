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
            this.ByteCode = new System.Windows.Forms.ListBox();
            this.Instructions = new System.Windows.Forms.ListBox();
            this.Registers = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // ByteCode
            // 
            this.ByteCode.FormattingEnabled = true;
            this.ByteCode.Location = new System.Drawing.Point(12, 360);
            this.ByteCode.Name = "ByteCode";
            this.ByteCode.Size = new System.Drawing.Size(566, 121);
            this.ByteCode.TabIndex = 0;
            // 
            // Instructions
            // 
            this.Instructions.FormattingEnabled = true;
            this.Instructions.Location = new System.Drawing.Point(12, 12);
            this.Instructions.Name = "Instructions";
            this.Instructions.Size = new System.Drawing.Size(346, 329);
            this.Instructions.TabIndex = 1;
            // 
            // Registers
            // 
            this.Registers.FormattingEnabled = true;
            this.Registers.Location = new System.Drawing.Point(364, 12);
            this.Registers.Name = "Registers";
            this.Registers.Size = new System.Drawing.Size(214, 277);
            this.Registers.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 493);
            this.Controls.Add(this.Registers);
            this.Controls.Add(this.Instructions);
            this.Controls.Add(this.ByteCode);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox ByteCode;
        private System.Windows.Forms.ListBox Instructions;
        private System.Windows.Forms.ListBox Registers;
    }
}

