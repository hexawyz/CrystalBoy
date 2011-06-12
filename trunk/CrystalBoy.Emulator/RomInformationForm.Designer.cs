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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RomInformationForm));
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
			resources.ApplyResources(gameNameLabel, "gameNameLabel");
			gameNameLabel.Name = "gameNameLabel";
			// 
			// makerCodeLabel
			// 
			resources.ApplyResources(makerCodeLabel, "makerCodeLabel");
			makerCodeLabel.Name = "makerCodeLabel";
			// 
			// makerNameLabel
			// 
			resources.ApplyResources(makerNameLabel, "makerNameLabel");
			makerNameLabel.Name = "makerNameLabel";
			// 
			// gameGroupBox
			// 
			resources.ApplyResources(gameGroupBox, "gameGroupBox");
			gameGroupBox.Controls.Add(this.gameTableLayoutPanel);
			gameGroupBox.Name = "gameGroupBox";
			gameGroupBox.TabStop = false;
			// 
			// gameTableLayoutPanel
			// 
			resources.ApplyResources(this.gameTableLayoutPanel, "gameTableLayoutPanel");
			this.gameTableLayoutPanel.Controls.Add(this.makerNameValueLabel, 1, 2);
			this.gameTableLayoutPanel.Controls.Add(makerNameLabel, 0, 2);
			this.gameTableLayoutPanel.Controls.Add(this.makerCodeValueLabel, 1, 1);
			this.gameTableLayoutPanel.Controls.Add(this.nameValueLabel, 1, 0);
			this.gameTableLayoutPanel.Controls.Add(gameNameLabel, 0, 0);
			this.gameTableLayoutPanel.Controls.Add(makerCodeLabel, 0, 1);
			this.gameTableLayoutPanel.Name = "gameTableLayoutPanel";
			// 
			// makerNameValueLabel
			// 
			resources.ApplyResources(this.makerNameValueLabel, "makerNameValueLabel");
			this.makerNameValueLabel.Name = "makerNameValueLabel";
			// 
			// makerCodeValueLabel
			// 
			resources.ApplyResources(this.makerCodeValueLabel, "makerCodeValueLabel");
			this.makerCodeValueLabel.Name = "makerCodeValueLabel";
			// 
			// nameValueLabel
			// 
			resources.ApplyResources(this.nameValueLabel, "nameValueLabel");
			this.nameValueLabel.Name = "nameValueLabel";
			// 
			// hardwareGroupBox
			// 
			resources.ApplyResources(hardwareGroupBox, "hardwareGroupBox");
			hardwareGroupBox.Controls.Add(this.tableLayoutPanel2);
			hardwareGroupBox.Name = "hardwareGroupBox";
			hardwareGroupBox.TabStop = false;
			// 
			// tableLayoutPanel2
			// 
			resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
			this.tableLayoutPanel2.Controls.Add(this.ramSizeValueLabel, 1, 2);
			this.tableLayoutPanel2.Controls.Add(ramSizeLabel, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.romSizeValueLabel, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.romTypeValueLabel, 1, 0);
			this.tableLayoutPanel2.Controls.Add(romTypeLabel, 0, 0);
			this.tableLayoutPanel2.Controls.Add(romSizeLabel, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.sgbCheckBox, 0, 3);
			this.tableLayoutPanel2.Controls.Add(this.cgbCheckBox, 0, 4);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			// 
			// ramSizeValueLabel
			// 
			resources.ApplyResources(this.ramSizeValueLabel, "ramSizeValueLabel");
			this.ramSizeValueLabel.Name = "ramSizeValueLabel";
			// 
			// ramSizeLabel
			// 
			resources.ApplyResources(ramSizeLabel, "ramSizeLabel");
			ramSizeLabel.Name = "ramSizeLabel";
			// 
			// romSizeValueLabel
			// 
			resources.ApplyResources(this.romSizeValueLabel, "romSizeValueLabel");
			this.romSizeValueLabel.Name = "romSizeValueLabel";
			// 
			// romTypeValueLabel
			// 
			resources.ApplyResources(this.romTypeValueLabel, "romTypeValueLabel");
			this.romTypeValueLabel.Name = "romTypeValueLabel";
			// 
			// romTypeLabel
			// 
			resources.ApplyResources(romTypeLabel, "romTypeLabel");
			romTypeLabel.Name = "romTypeLabel";
			// 
			// romSizeLabel
			// 
			resources.ApplyResources(romSizeLabel, "romSizeLabel");
			romSizeLabel.Name = "romSizeLabel";
			// 
			// sgbCheckBox
			// 
			resources.ApplyResources(this.sgbCheckBox, "sgbCheckBox");
			this.sgbCheckBox.AutoCheck = false;
			this.tableLayoutPanel2.SetColumnSpan(this.sgbCheckBox, 2);
			this.sgbCheckBox.Name = "sgbCheckBox";
			this.sgbCheckBox.UseVisualStyleBackColor = true;
			// 
			// cgbCheckBox
			// 
			resources.ApplyResources(this.cgbCheckBox, "cgbCheckBox");
			this.cgbCheckBox.AutoCheck = false;
			this.tableLayoutPanel2.SetColumnSpan(this.cgbCheckBox, 2);
			this.cgbCheckBox.Name = "cgbCheckBox";
			this.cgbCheckBox.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			resources.ApplyResources(this.okButton, "okButton");
			this.okButton.Name = "okButton";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// RomInformationForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.okButton);
			this.Controls.Add(hardwareGroupBox);
			this.Controls.Add(gameGroupBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RomInformationForm";
			this.ShowInTaskbar = false;
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