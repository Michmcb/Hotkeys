namespace Hotkeys
{
	partial class ChordProcessor
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
			this.uxChords = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// uxChords
			// 
			this.uxChords.AutoSize = true;
			this.uxChords.Dock = System.Windows.Forms.DockStyle.Fill;
			this.uxChords.Location = new System.Drawing.Point(0, 0);
			this.uxChords.Name = "uxChords";
			this.uxChords.Size = new System.Drawing.Size(28, 13);
			this.uxChords.TabIndex = 0;
			this.uxChords.Text = "Test";
			// 
			// ChordProcessor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(240, 120);
			this.ControlBox = false;
			this.Controls.Add(this.uxChords);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ChordProcessor";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ChordProcessor";
			this.TopMost = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label uxChords;
	}
}