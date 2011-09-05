#region Copyright Notice
// This file is part of CrystalBoy.
// Copyright © 2008-2011 Fabien Barbier
// 
// CrystalBoy is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// CrystalBoy is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

namespace CrystalBoy.Emulator
{
	partial class AboutForm
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
			System.Windows.Forms.Label titleLabel;
			System.Windows.Forms.Label versionTextLabel;
			this.urlLabel = new System.Windows.Forms.LinkLabel();
			this.versionValueLabel = new System.Windows.Forms.Label();
			this.authorLabel = new System.Windows.Forms.Label();
			titleLabel = new System.Windows.Forms.Label();
			versionTextLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// titleLabel
			// 
			titleLabel.AutoSize = true;
			titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			titleLabel.Location = new System.Drawing.Point(12, 9);
			titleLabel.Name = "titleLabel";
			titleLabel.Size = new System.Drawing.Size(180, 37);
			titleLabel.TabIndex = 1;
			titleLabel.Text = "CrystalBoy";
			// 
			// versionTextLabel
			// 
			versionTextLabel.AutoSize = true;
			versionTextLabel.Location = new System.Drawing.Point(16, 46);
			versionTextLabel.Name = "versionTextLabel";
			versionTextLabel.Size = new System.Drawing.Size(42, 13);
			versionTextLabel.TabIndex = 2;
			versionTextLabel.Text = "Version";
			// 
			// urlLabel
			// 
			this.urlLabel.AutoSize = true;
			this.urlLabel.Location = new System.Drawing.Point(12, 85);
			this.urlLabel.Name = "urlLabel";
			this.urlLabel.Size = new System.Drawing.Size(188, 13);
			this.urlLabel.TabIndex = 0;
			this.urlLabel.TabStop = true;
			this.urlLabel.Text = "http://code.google.com/p/crystalboy/";
			this.urlLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.urlLabel_LinkClicked);
			// 
			// versionValueLabel
			// 
			this.versionValueLabel.AutoSize = true;
			this.versionValueLabel.Location = new System.Drawing.Point(64, 46);
			this.versionValueLabel.Name = "versionValueLabel";
			this.versionValueLabel.Size = new System.Drawing.Size(40, 13);
			this.versionValueLabel.TabIndex = 3;
			this.versionValueLabel.Text = "0.0.0.0";
			// 
			// authorLabel
			// 
			this.authorLabel.AutoSize = true;
			this.authorLabel.Location = new System.Drawing.Point(16, 59);
			this.authorLabel.Name = "authorLabel";
			this.authorLabel.Size = new System.Drawing.Size(86, 13);
			this.authorLabel.TabIndex = 4;
			this.authorLabel.Text = "by GoldenCrystal";
			// 
			// AboutForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(212, 107);
			this.Controls.Add(this.authorLabel);
			this.Controls.Add(this.versionValueLabel);
			this.Controls.Add(versionTextLabel);
			this.Controls.Add(titleLabel);
			this.Controls.Add(this.urlLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutForm";
			this.Text = "About CrystalBoy…";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.LinkLabel urlLabel;
		private System.Windows.Forms.Label versionValueLabel;
		private System.Windows.Forms.Label authorLabel;
	}
}