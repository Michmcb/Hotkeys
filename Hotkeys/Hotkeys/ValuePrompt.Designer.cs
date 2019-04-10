namespace Hotkeys
{
    partial class ValuePrompt
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ValuePrompt));
			this.uxMessage = new System.Windows.Forms.Label();
			this.uxInput = new System.Windows.Forms.TextBox();
			this.uxOk = new System.Windows.Forms.Button();
			this.uxCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// uxMessage
			// 
			this.uxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.uxMessage.Location = new System.Drawing.Point(12, 9);
			this.uxMessage.Name = "uxMessage";
			this.uxMessage.Size = new System.Drawing.Size(488, 13);
			this.uxMessage.TabIndex = 0;
			// 
			// uxInput
			// 
			this.uxInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.uxInput.Location = new System.Drawing.Point(12, 28);
			this.uxInput.Name = "uxInput";
			this.uxInput.Size = new System.Drawing.Size(488, 20);
			this.uxInput.TabIndex = 1;
			// 
			// uxOk
			// 
			this.uxOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.uxOk.Location = new System.Drawing.Point(425, 54);
			this.uxOk.Name = "uxOk";
			this.uxOk.Size = new System.Drawing.Size(75, 23);
			this.uxOk.TabIndex = 2;
			this.uxOk.Text = "OK";
			this.uxOk.UseVisualStyleBackColor = true;
			// 
			// uxCancel
			// 
			this.uxCancel.Location = new System.Drawing.Point(344, 54);
			this.uxCancel.Name = "uxCancel";
			this.uxCancel.Size = new System.Drawing.Size(75, 23);
			this.uxCancel.TabIndex = 3;
			this.uxCancel.Text = "Cancel";
			this.uxCancel.UseVisualStyleBackColor = true;
			// 
			// ValuePrompt
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(512, 87);
			this.Controls.Add(this.uxCancel);
			this.Controls.Add(this.uxOk);
			this.Controls.Add(this.uxInput);
			this.Controls.Add(this.uxMessage);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ValuePrompt";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label uxMessage;
        private System.Windows.Forms.TextBox uxInput;
        private System.Windows.Forms.Button uxOk;
        private System.Windows.Forms.Button uxCancel;
    }
}