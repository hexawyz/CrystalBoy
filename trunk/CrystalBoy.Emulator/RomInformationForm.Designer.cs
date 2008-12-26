#region Copyright Notice
// This file is part of CrystalBoy.
// Copyright (C) 2008 Fabien Barbier
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
	partial class RomInformationForm
	{
		/// <summary>
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur Windows Form

		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.Label gameNameLabel;
			System.Windows.Forms.Label makerCodeLabel;
			System.Windows.Forms.Label makerNameLabel;
			System.Windows.Forms.GroupBox gameGroupBox;
			System.Windows.Forms.GroupBox hardwareGroupBox;
			System.Windows.Forms.Label ramSizeLabel;
			System.Windows.Forms.Label romTypeLabel;
			System.Windows.Forms.Label romSizeLabel;
			this.gameTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.makerNameValueLabel = new System.Windows.Forms.Label();
			this.makerCodeValueLabel = new System.Windows.Forms.Label();
			this.nameValueLabel = new System.Windows.Forms.Label();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.ramSizeValueLabel = new System.Windows.Forms.Label();
			this.romSizeValueLabel = new System.Windows.Forms.Label();
			this.romTypeValueLabel = new System.Windows.Forms.Label();
			this.sgbCheckBox = new System.Windows.Forms.CheckBox();
			this.cgbCheckBox = new System.Windows.Forms.CheckBox();
			this.okButton = new System.Windows.Forms.Button();
			gameNameLabel = new System.Windows.Forms.Label();
			makerCodeLabel = new System.Windows.Forms.Label();
			makerNameLabel = new System.Windows.Forms.Label();
			gameGroupBox = new System.Windows.Forms.GroupBox();
			hardwareGroupBox = new System.Windows.Forms.GroupBox();
			ramSizeLabel = new System.Windows.Forms.Label();
			romTypeLabel = new System.Windows.Forms.Label();
			romSizeLabel = new System.Windows.Forms.Label();
			gameGroupBox.SuspendLayout();
			this.gameTableLayoutPanel.SuspendLayout();
			hardwareGroupBox.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// gameNameLabel
			// 
			gameNameLabel.AutoSize = true;
			gameNameLabel.Location = new System.Drawing.Point(3, 2);
			gameNameLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			gameNameLabel.Name = "gameNameLabel";
			gameNameLabel.Size = new System.Drawing.Size(38, 13);
			gameNameLabel.TabIndex = 0;
			gameNameLabel.Text = "Name:";
			// 
			// makerCodeLabel
			// 
			makerCodeLabel.AutoSize = true;
			makerCodeLabel.Location = new System.Drawing.Point(3, 19);
			makerCodeLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			makerCodeLabel.Name = "makerCodeLabel";
			makerCodeLabel.Size = new System.Drawing.Size(67, 13);
			makerCodeLabel.TabIndex = 2;
			makerCodeLabel.Text = "Maker code:";
			// 
			// makerNameLabel
			// 
			makerNameLabel.AutoSize = true;
			makerNameLabel.Location = new System.Drawing.Point(3, 36);
			makerNameLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			makerNameLabel.Name = "makerNameLabel";
			makerNameLabel.Size = new System.Drawing.Size(69, 13);
			makerNameLabel.TabIndex = 4;
			makerNameLabel.Text = "Maker name:";
			// 
			// gameGroupBox
			// 
			gameGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			gameGroupBox.Controls.Add(this.gameTableLayoutPanel);
			gameGroupBox.Location = new System.Drawing.Point(12, 12);
			gameGroupBox.Name = "gameGroupBox";
			gameGroupBox.Size = new System.Drawing.Size(320, 77);
			gameGroupBox.TabIndex = 0;
			gameGroupBox.TabStop = false;
			gameGroupBox.Text = "Game";
			// 
			// gameTableLayoutPanel
			// 
			this.gameTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.gameTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.gameTableLayoutPanel.ColumnCount = 2;
			this.gameTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
			this.gameTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68F));
			this.gameTableLayoutPanel.Controls.Add(this.makerNameValueLabel, 1, 2);
			this.gameTableLayoutPanel.Controls.Add(makerNameLabel, 0, 2);
			this.gameTableLayoutPanel.Controls.Add(this.makerCodeValueLabel, 1, 1);
			this.gameTableLayoutPanel.Controls.Add(this.nameValueLabel, 1, 0);
			this.gameTableLayoutPanel.Controls.Add(gameNameLabel, 0, 0);
			this.gameTableLayoutPanel.Controls.Add(makerCodeLabel, 0, 1);
			this.gameTableLayoutPanel.Location = new System.Drawing.Point(6, 19);
			this.gameTableLayoutPanel.Name = "gameTableLayoutPanel";
			this.gameTableLayoutPanel.RowCount = 3;
			this.gameTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.gameTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.gameTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.gameTableLayoutPanel.Size = new System.Drawing.Size(308, 52);
			this.gameTableLayoutPanel.TabIndex = 0;
			// 
			// makerNameValueLabel
			// 
			this.makerNameValueLabel.AutoSize = true;
			this.makerNameValueLabel.Location = new System.Drawing.Point(101, 36);
			this.makerNameValueLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.makerNameValueLabel.Name = "makerNameValueLabel";
			this.makerNameValueLabel.Size = new System.Drawing.Size(0, 13);
			this.makerNameValueLabel.TabIndex = 5;
			// 
			// makerCodeValueLabel
			// 
			this.makerCodeValueLabel.AutoSize = true;
			this.makerCodeValueLabel.Location = new System.Drawing.Point(101, 19);
			this.makerCodeValueLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.makerCodeValueLabel.Name = "makerCodeValueLabel";
			this.makerCodeValueLabel.Size = new System.Drawing.Size(0, 13);
			this.makerCodeValueLabel.TabIndex = 3;
			// 
			// nameValueLabel
			// 
			this.nameValueLabel.AutoSize = true;
			this.nameValueLabel.Location = new System.Drawing.Point(101, 2);
			this.nameValueLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.nameValueLabel.Name = "nameValueLabel";
			this.nameValueLabel.Size = new System.Drawing.Size(0, 13);
			this.nameValueLabel.TabIndex = 1;
			// 
			// hardwareGroupBox
			// 
			hardwareGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			hardwareGroupBox.Controls.Add(this.tableLayoutPanel2);
			hardwareGroupBox.Location = new System.Drawing.Point(12, 95);
			hardwareGroupBox.Name = "hardwareGroupBox";
			hardwareGroupBox.Size = new System.Drawing.Size(320, 123);
			hardwareGroupBox.TabIndex = 1;
			hardwareGroupBox.TabStop = false;
			hardwareGroupBox.Text = "Hardware";
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68F));
			this.tableLayoutPanel2.Controls.Add(this.ramSizeValueLabel, 1, 2);
			this.tableLayoutPanel2.Controls.Add(ramSizeLabel, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.romSizeValueLabel, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.romTypeValueLabel, 1, 0);
			this.tableLayoutPanel2.Controls.Add(romTypeLabel, 0, 0);
			this.tableLayoutPanel2.Controls.Add(romSizeLabel, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.sgbCheckBox, 0, 3);
			this.tableLayoutPanel2.Controls.Add(this.cgbCheckBox, 0, 4);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 19);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 5;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(308, 98);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// ramSizeValueLabel
			// 
			this.ramSizeValueLabel.AutoSize = true;
			this.ramSizeValueLabel.Location = new System.Drawing.Point(101, 36);
			this.ramSizeValueLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.ramSizeValueLabel.Name = "ramSizeValueLabel";
			this.ramSizeValueLabel.Size = new System.Drawing.Size(0, 13);
			this.ramSizeValueLabel.TabIndex = 5;
			// 
			// ramSizeLabel
			// 
			ramSizeLabel.AutoSize = true;
			ramSizeLabel.Location = new System.Drawing.Point(3, 36);
			ramSizeLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			ramSizeLabel.Name = "ramSizeLabel";
			ramSizeLabel.Size = new System.Drawing.Size(57, 13);
			ramSizeLabel.TabIndex = 4;
			ramSizeLabel.Text = "RAM Size:";
			// 
			// romSizeValueLabel
			// 
			this.romSizeValueLabel.AutoSize = true;
			this.romSizeValueLabel.Location = new System.Drawing.Point(101, 19);
			this.romSizeValueLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.romSizeValueLabel.Name = "romSizeValueLabel";
			this.romSizeValueLabel.Size = new System.Drawing.Size(0, 13);
			this.romSizeValueLabel.TabIndex = 3;
			// 
			// romTypeValueLabel
			// 
			this.romTypeValueLabel.AutoSize = true;
			this.romTypeValueLabel.Location = new System.Drawing.Point(101, 2);
			this.romTypeValueLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.romTypeValueLabel.Name = "romTypeValueLabel";
			this.romTypeValueLabel.Size = new System.Drawing.Size(0, 13);
			this.romTypeValueLabel.TabIndex = 1;
			// 
			// romTypeLabel
			// 
			romTypeLabel.AutoSize = true;
			romTypeLabel.Location = new System.Drawing.Point(3, 2);
			romTypeLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			romTypeLabel.Name = "romTypeLabel";
			romTypeLabel.Size = new System.Drawing.Size(62, 13);
			romTypeLabel.TabIndex = 0;
			romTypeLabel.Text = "ROM Type:";
			// 
			// romSizeLabel
			// 
			romSizeLabel.AutoSize = true;
			romSizeLabel.Location = new System.Drawing.Point(3, 19);
			romSizeLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			romSizeLabel.Name = "romSizeLabel";
			romSizeLabel.Size = new System.Drawing.Size(58, 13);
			romSizeLabel.TabIndex = 2;
			romSizeLabel.Text = "ROM Size:";
			// 
			// sgbCheckBox
			// 
			this.sgbCheckBox.AutoCheck = false;
			this.sgbCheckBox.AutoSize = true;
			this.tableLayoutPanel2.SetColumnSpan(this.sgbCheckBox, 2);
			this.sgbCheckBox.Location = new System.Drawing.Point(3, 54);
			this.sgbCheckBox.Name = "sgbCheckBox";
			this.sgbCheckBox.Size = new System.Drawing.Size(149, 17);
			this.sgbCheckBox.TabIndex = 6;
			this.sgbCheckBox.Text = "Super Game Boy Funtions";
			this.sgbCheckBox.UseVisualStyleBackColor = true;
			// 
			// cgbCheckBox
			// 
			this.cgbCheckBox.AutoCheck = false;
			this.cgbCheckBox.AutoSize = true;
			this.tableLayoutPanel2.SetColumnSpan(this.cgbCheckBox, 2);
			this.cgbCheckBox.Location = new System.Drawing.Point(3, 77);
			this.cgbCheckBox.Name = "cgbCheckBox";
			this.cgbCheckBox.Size = new System.Drawing.Size(149, 17);
			this.cgbCheckBox.TabIndex = 7;
			this.cgbCheckBox.Text = "Color game Boy Functions";
			this.cgbCheckBox.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Location = new System.Drawing.Point(257, 224);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 2;
			this.okButton.Text = "&OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// RomInformationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(344, 259);
			this.Controls.Add(this.okButton);
			this.Controls.Add(hardwareGroupBox);
			this.Controls.Add(gameGroupBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RomInformationForm";
			this.ShowInTaskbar = false;
			this.Text = "ROM Information";
			gameGroupBox.ResumeLayout(false);
			this.gameTableLayoutPanel.ResumeLayout(false);
			this.gameTableLayoutPanel.PerformLayout();
			hardwareGroupBox.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel gameTableLayoutPanel;
		private System.Windows.Forms.Label makerNameValueLabel;
		private System.Windows.Forms.Label makerCodeValueLabel;
		private System.Windows.Forms.Label nameValueLabel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label ramSizeValueLabel;
		private System.Windows.Forms.Label romSizeValueLabel;
		private System.Windows.Forms.Label romTypeValueLabel;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.CheckBox sgbCheckBox;
		private System.Windows.Forms.CheckBox cgbCheckBox;
	}
}