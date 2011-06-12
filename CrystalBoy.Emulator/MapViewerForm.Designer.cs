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
	partial class MapViewerForm
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
			System.Windows.Forms.GroupBox backgroundMapGroupBox;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapViewerForm));
			System.Windows.Forms.GroupBox windowMapGroupBox;
			System.Windows.Forms.GroupBox customMapGroupBox;
			System.Windows.Forms.TabPage autoMapsTabPage;
			System.Windows.Forms.Label label13;
			System.Windows.Forms.Label label11;
			System.Windows.Forms.Label label9;
			System.Windows.Forms.Label label4;
			System.Windows.Forms.Label label3;
			System.Windows.Forms.Label label2;
			System.Windows.Forms.Label label1;
			this.backgroundMapPanel = new CrystalBoy.Emulator.BitmapPanel();
			this.windowMapPanel = new CrystalBoy.Emulator.BitmapPanel();
			this.customMapPanel = new CrystalBoy.Emulator.BitmapPanel();
			this.refreshButton = new System.Windows.Forms.Button();
			this.closeButton = new System.Windows.Forms.Button();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.customMapsTabPage = new System.Windows.Forms.TabPage();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.pauseUpdateCheckBox = new System.Windows.Forms.CheckBox();
			this.frameUpdateCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.informationGroupBox = new System.Windows.Forms.GroupBox();
			this.label14 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.mapDataGroupBox = new System.Windows.Forms.GroupBox();
			this.mapData1RadioButton = new System.Windows.Forms.RadioButton();
			this.mapData0RadioButton = new System.Windows.Forms.RadioButton();
			this.tileDataGroupBox = new System.Windows.Forms.GroupBox();
			this.tileData1RadioButton = new System.Windows.Forms.RadioButton();
			this.tileData0RadioButton = new System.Windows.Forms.RadioButton();
			backgroundMapGroupBox = new System.Windows.Forms.GroupBox();
			windowMapGroupBox = new System.Windows.Forms.GroupBox();
			customMapGroupBox = new System.Windows.Forms.GroupBox();
			autoMapsTabPage = new System.Windows.Forms.TabPage();
			label13 = new System.Windows.Forms.Label();
			label11 = new System.Windows.Forms.Label();
			label9 = new System.Windows.Forms.Label();
			label4 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			backgroundMapGroupBox.SuspendLayout();
			windowMapGroupBox.SuspendLayout();
			customMapGroupBox.SuspendLayout();
			autoMapsTabPage.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.customMapsTabPage.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.informationGroupBox.SuspendLayout();
			this.mapDataGroupBox.SuspendLayout();
			this.tileDataGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// backgroundMapGroupBox
			// 
			backgroundMapGroupBox.Controls.Add(this.backgroundMapPanel);
			resources.ApplyResources(backgroundMapGroupBox, "backgroundMapGroupBox");
			backgroundMapGroupBox.Name = "backgroundMapGroupBox";
			backgroundMapGroupBox.TabStop = false;
			// 
			// backgroundMapPanel
			// 
			this.backgroundMapPanel.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.backgroundMapPanel, "backgroundMapPanel");
			this.backgroundMapPanel.Name = "backgroundMapPanel";
			// 
			// windowMapGroupBox
			// 
			windowMapGroupBox.Controls.Add(this.windowMapPanel);
			resources.ApplyResources(windowMapGroupBox, "windowMapGroupBox");
			windowMapGroupBox.Name = "windowMapGroupBox";
			windowMapGroupBox.TabStop = false;
			// 
			// windowMapPanel
			// 
			this.windowMapPanel.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.windowMapPanel, "windowMapPanel");
			this.windowMapPanel.Name = "windowMapPanel";
			// 
			// customMapGroupBox
			// 
			customMapGroupBox.Controls.Add(this.customMapPanel);
			resources.ApplyResources(customMapGroupBox, "customMapGroupBox");
			customMapGroupBox.Name = "customMapGroupBox";
			customMapGroupBox.TabStop = false;
			// 
			// customMapPanel
			// 
			this.customMapPanel.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.customMapPanel, "customMapPanel");
			this.customMapPanel.Name = "customMapPanel";
			// 
			// autoMapsTabPage
			// 
			autoMapsTabPage.Controls.Add(backgroundMapGroupBox);
			autoMapsTabPage.Controls.Add(windowMapGroupBox);
			resources.ApplyResources(autoMapsTabPage, "autoMapsTabPage");
			autoMapsTabPage.Name = "autoMapsTabPage";
			autoMapsTabPage.UseVisualStyleBackColor = true;
			// 
			// label13
			// 
			resources.ApplyResources(label13, "label13");
			label13.Name = "label13";
			// 
			// label11
			// 
			resources.ApplyResources(label11, "label11");
			label11.Name = "label11";
			// 
			// label9
			// 
			resources.ApplyResources(label9, "label9");
			label9.Name = "label9";
			// 
			// label4
			// 
			resources.ApplyResources(label4, "label4");
			label4.Name = "label4";
			// 
			// label3
			// 
			resources.ApplyResources(label3, "label3");
			label3.Name = "label3";
			// 
			// label2
			// 
			resources.ApplyResources(label2, "label2");
			label2.Name = "label2";
			// 
			// label1
			// 
			resources.ApplyResources(label1, "label1");
			label1.Name = "label1";
			// 
			// refreshButton
			// 
			resources.ApplyResources(this.refreshButton, "refreshButton");
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.UseVisualStyleBackColor = true;
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.closeButton, "closeButton");
			this.closeButton.Name = "closeButton";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(autoMapsTabPage);
			this.tabControl.Controls.Add(this.customMapsTabPage);
			resources.ApplyResources(this.tabControl, "tabControl");
			this.tabControl.Multiline = true;
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			// 
			// customMapsTabPage
			// 
			this.customMapsTabPage.Controls.Add(this.groupBox2);
			this.customMapsTabPage.Controls.Add(this.groupBox1);
			this.customMapsTabPage.Controls.Add(this.informationGroupBox);
			this.customMapsTabPage.Controls.Add(this.mapDataGroupBox);
			this.customMapsTabPage.Controls.Add(this.tileDataGroupBox);
			this.customMapsTabPage.Controls.Add(customMapGroupBox);
			resources.ApplyResources(this.customMapsTabPage, "customMapsTabPage");
			this.customMapsTabPage.Name = "customMapsTabPage";
			this.customMapsTabPage.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.pauseUpdateCheckBox);
			this.groupBox2.Controls.Add(this.frameUpdateCheckBox);
			resources.ApplyResources(this.groupBox2, "groupBox2");
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.TabStop = false;
			// 
			// pauseUpdateCheckBox
			// 
			resources.ApplyResources(this.pauseUpdateCheckBox, "pauseUpdateCheckBox");
			this.pauseUpdateCheckBox.Name = "pauseUpdateCheckBox";
			this.pauseUpdateCheckBox.UseVisualStyleBackColor = true;
			// 
			// frameUpdateCheckBox
			// 
			resources.ApplyResources(this.frameUpdateCheckBox, "frameUpdateCheckBox");
			this.frameUpdateCheckBox.Name = "frameUpdateCheckBox";
			this.frameUpdateCheckBox.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.panel1);
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.panel1, "panel1");
			this.panel1.Name = "panel1";
			// 
			// informationGroupBox
			// 
			this.informationGroupBox.Controls.Add(this.label14);
			this.informationGroupBox.Controls.Add(label13);
			this.informationGroupBox.Controls.Add(this.label12);
			this.informationGroupBox.Controls.Add(label11);
			this.informationGroupBox.Controls.Add(this.label10);
			this.informationGroupBox.Controls.Add(label9);
			this.informationGroupBox.Controls.Add(this.label8);
			this.informationGroupBox.Controls.Add(this.label7);
			this.informationGroupBox.Controls.Add(this.label6);
			this.informationGroupBox.Controls.Add(this.label5);
			this.informationGroupBox.Controls.Add(label4);
			this.informationGroupBox.Controls.Add(label3);
			this.informationGroupBox.Controls.Add(label2);
			this.informationGroupBox.Controls.Add(label1);
			resources.ApplyResources(this.informationGroupBox, "informationGroupBox");
			this.informationGroupBox.Name = "informationGroupBox";
			this.informationGroupBox.TabStop = false;
			// 
			// label14
			// 
			resources.ApplyResources(this.label14, "label14");
			this.label14.Name = "label14";
			// 
			// label12
			// 
			resources.ApplyResources(this.label12, "label12");
			this.label12.Name = "label12";
			// 
			// label10
			// 
			resources.ApplyResources(this.label10, "label10");
			this.label10.Name = "label10";
			// 
			// label8
			// 
			resources.ApplyResources(this.label8, "label8");
			this.label8.Name = "label8";
			// 
			// label7
			// 
			resources.ApplyResources(this.label7, "label7");
			this.label7.Name = "label7";
			// 
			// label6
			// 
			resources.ApplyResources(this.label6, "label6");
			this.label6.Name = "label6";
			// 
			// label5
			// 
			resources.ApplyResources(this.label5, "label5");
			this.label5.Name = "label5";
			// 
			// mapDataGroupBox
			// 
			this.mapDataGroupBox.Controls.Add(this.mapData1RadioButton);
			this.mapDataGroupBox.Controls.Add(this.mapData0RadioButton);
			resources.ApplyResources(this.mapDataGroupBox, "mapDataGroupBox");
			this.mapDataGroupBox.Name = "mapDataGroupBox";
			this.mapDataGroupBox.TabStop = false;
			// 
			// mapData1RadioButton
			// 
			resources.ApplyResources(this.mapData1RadioButton, "mapData1RadioButton");
			this.mapData1RadioButton.Name = "mapData1RadioButton";
			this.mapData1RadioButton.TabStop = true;
			this.mapData1RadioButton.UseVisualStyleBackColor = true;
			// 
			// mapData0RadioButton
			// 
			resources.ApplyResources(this.mapData0RadioButton, "mapData0RadioButton");
			this.mapData0RadioButton.Checked = true;
			this.mapData0RadioButton.Name = "mapData0RadioButton";
			this.mapData0RadioButton.TabStop = true;
			this.mapData0RadioButton.UseVisualStyleBackColor = true;
			this.mapData0RadioButton.CheckedChanged += new System.EventHandler(this.dataRadioButton_CheckedChanged);
			// 
			// tileDataGroupBox
			// 
			this.tileDataGroupBox.Controls.Add(this.tileData1RadioButton);
			this.tileDataGroupBox.Controls.Add(this.tileData0RadioButton);
			resources.ApplyResources(this.tileDataGroupBox, "tileDataGroupBox");
			this.tileDataGroupBox.Name = "tileDataGroupBox";
			this.tileDataGroupBox.TabStop = false;
			// 
			// tileData1RadioButton
			// 
			resources.ApplyResources(this.tileData1RadioButton, "tileData1RadioButton");
			this.tileData1RadioButton.Name = "tileData1RadioButton";
			this.tileData1RadioButton.TabStop = true;
			this.tileData1RadioButton.UseVisualStyleBackColor = true;
			// 
			// tileData0RadioButton
			// 
			resources.ApplyResources(this.tileData0RadioButton, "tileData0RadioButton");
			this.tileData0RadioButton.Checked = true;
			this.tileData0RadioButton.Name = "tileData0RadioButton";
			this.tileData0RadioButton.TabStop = true;
			this.tileData0RadioButton.UseVisualStyleBackColor = true;
			this.tileData0RadioButton.CheckedChanged += new System.EventHandler(this.dataRadioButton_CheckedChanged);
			// 
			// MapViewerForm
			// 
			this.AcceptButton = this.refreshButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.closeButton;
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.refreshButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MapViewerForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			backgroundMapGroupBox.ResumeLayout(false);
			windowMapGroupBox.ResumeLayout(false);
			customMapGroupBox.ResumeLayout(false);
			autoMapsTabPage.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.customMapsTabPage.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.informationGroupBox.ResumeLayout(false);
			this.informationGroupBox.PerformLayout();
			this.mapDataGroupBox.ResumeLayout(false);
			this.mapDataGroupBox.PerformLayout();
			this.tileDataGroupBox.ResumeLayout(false);
			this.tileDataGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private BitmapPanel backgroundMapPanel;
		private BitmapPanel windowMapPanel;
		private System.Windows.Forms.Button refreshButton;
		private System.Windows.Forms.Button closeButton;
		private BitmapPanel customMapPanel;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage customMapsTabPage;
		private System.Windows.Forms.GroupBox tileDataGroupBox;
		private System.Windows.Forms.GroupBox mapDataGroupBox;
		private System.Windows.Forms.RadioButton mapData1RadioButton;
		private System.Windows.Forms.RadioButton mapData0RadioButton;
		private System.Windows.Forms.RadioButton tileData1RadioButton;
		private System.Windows.Forms.RadioButton tileData0RadioButton;
		private System.Windows.Forms.GroupBox informationGroupBox;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox pauseUpdateCheckBox;
		private System.Windows.Forms.CheckBox frameUpdateCheckBox;
	}
}