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
			backgroundMapGroupBox.Location = new System.Drawing.Point(3, 3);
			backgroundMapGroupBox.Name = "backgroundMapGroupBox";
			backgroundMapGroupBox.Size = new System.Drawing.Size(268, 281);
			backgroundMapGroupBox.TabIndex = 0;
			backgroundMapGroupBox.TabStop = false;
			backgroundMapGroupBox.Text = "Background Map";
			// 
			// backgroundMapPanel
			// 
			this.backgroundMapPanel.BackColor = System.Drawing.Color.White;
			this.backgroundMapPanel.Location = new System.Drawing.Point(6, 19);
			this.backgroundMapPanel.Name = "backgroundMapPanel";
			this.backgroundMapPanel.Size = new System.Drawing.Size(256, 256);
			this.backgroundMapPanel.TabIndex = 2;
			// 
			// windowMapGroupBox
			// 
			windowMapGroupBox.Controls.Add(this.windowMapPanel);
			windowMapGroupBox.Location = new System.Drawing.Point(277, 3);
			windowMapGroupBox.Name = "windowMapGroupBox";
			windowMapGroupBox.Size = new System.Drawing.Size(268, 281);
			windowMapGroupBox.TabIndex = 1;
			windowMapGroupBox.TabStop = false;
			windowMapGroupBox.Text = "Window Map";
			// 
			// windowMapPanel
			// 
			this.windowMapPanel.BackColor = System.Drawing.Color.White;
			this.windowMapPanel.Location = new System.Drawing.Point(6, 19);
			this.windowMapPanel.Name = "windowMapPanel";
			this.windowMapPanel.Size = new System.Drawing.Size(256, 256);
			this.windowMapPanel.TabIndex = 3;
			// 
			// customMapGroupBox
			// 
			customMapGroupBox.Controls.Add(this.customMapPanel);
			customMapGroupBox.Location = new System.Drawing.Point(277, 3);
			customMapGroupBox.Name = "customMapGroupBox";
			customMapGroupBox.Size = new System.Drawing.Size(268, 281);
			customMapGroupBox.TabIndex = 4;
			customMapGroupBox.TabStop = false;
			customMapGroupBox.Text = "Map";
			// 
			// customMapPanel
			// 
			this.customMapPanel.BackColor = System.Drawing.Color.White;
			this.customMapPanel.Location = new System.Drawing.Point(6, 19);
			this.customMapPanel.Name = "customMapPanel";
			this.customMapPanel.Size = new System.Drawing.Size(256, 256);
			this.customMapPanel.TabIndex = 2;
			// 
			// autoMapsTabPage
			// 
			autoMapsTabPage.Controls.Add(backgroundMapGroupBox);
			autoMapsTabPage.Controls.Add(windowMapGroupBox);
			autoMapsTabPage.Location = new System.Drawing.Point(4, 22);
			autoMapsTabPage.Name = "autoMapsTabPage";
			autoMapsTabPage.Padding = new System.Windows.Forms.Padding(3);
			autoMapsTabPage.Size = new System.Drawing.Size(548, 287);
			autoMapsTabPage.TabIndex = 0;
			autoMapsTabPage.Text = "Automatic";
			autoMapsTabPage.UseVisualStyleBackColor = true;
			// 
			// label13
			// 
			label13.AutoSize = true;
			label13.Location = new System.Drawing.Point(6, 118);
			label13.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			label13.Name = "label13";
			label13.Size = new System.Drawing.Size(44, 13);
			label13.TabIndex = 12;
			label13.Text = "Priority: ";
			// 
			// label11
			// 
			label11.AutoSize = true;
			label11.Location = new System.Drawing.Point(6, 101);
			label11.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			label11.Name = "label11";
			label11.Size = new System.Drawing.Size(46, 13);
			label11.TabIndex = 10;
			label11.Text = "Palette: ";
			// 
			// label9
			// 
			label9.AutoSize = true;
			label9.Location = new System.Drawing.Point(6, 33);
			label9.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			label9.Name = "label9";
			label9.Size = new System.Drawing.Size(50, 13);
			label9.TabIndex = 8;
			label9.Text = "Position: ";
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(6, 84);
			label4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(29, 13);
			label4.TabIndex = 3;
			label4.Text = "Flip: ";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(6, 67);
			label3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(30, 13);
			label3.TabIndex = 2;
			label3.Text = "Tile: ";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(6, 50);
			label2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(51, 13);
			label2.TabIndex = 1;
			label2.Text = "Address: ";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(6, 16);
			label1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(35, 13);
			label1.TabIndex = 0;
			label1.Text = "Pixel: ";
			// 
			// refreshButton
			// 
			this.refreshButton.Location = new System.Drawing.Point(412, 331);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = new System.Drawing.Size(75, 23);
			this.refreshButton.TabIndex = 2;
			this.refreshButton.Text = "&Refresh";
			this.refreshButton.UseVisualStyleBackColor = true;
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(493, 331);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(75, 23);
			this.closeButton.TabIndex = 3;
			this.closeButton.Text = "&Close";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(autoMapsTabPage);
			this.tabControl.Controls.Add(this.customMapsTabPage);
			this.tabControl.Location = new System.Drawing.Point(12, 12);
			this.tabControl.Multiline = true;
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(556, 313);
			this.tabControl.TabIndex = 5;
			// 
			// customMapsTabPage
			// 
			this.customMapsTabPage.Controls.Add(this.groupBox2);
			this.customMapsTabPage.Controls.Add(this.groupBox1);
			this.customMapsTabPage.Controls.Add(this.informationGroupBox);
			this.customMapsTabPage.Controls.Add(this.mapDataGroupBox);
			this.customMapsTabPage.Controls.Add(this.tileDataGroupBox);
			this.customMapsTabPage.Controls.Add(customMapGroupBox);
			this.customMapsTabPage.Location = new System.Drawing.Point(4, 22);
			this.customMapsTabPage.Name = "customMapsTabPage";
			this.customMapsTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.customMapsTabPage.Size = new System.Drawing.Size(548, 287);
			this.customMapsTabPage.TabIndex = 1;
			this.customMapsTabPage.Text = "Custom";
			this.customMapsTabPage.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.pauseUpdateCheckBox);
			this.groupBox2.Controls.Add(this.frameUpdateCheckBox);
			this.groupBox2.Location = new System.Drawing.Point(3, 145);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(100, 65);
			this.groupBox2.TabIndex = 9;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Update on";
			// 
			// pauseUpdateCheckBox
			// 
			this.pauseUpdateCheckBox.AutoSize = true;
			this.pauseUpdateCheckBox.Location = new System.Drawing.Point(6, 42);
			this.pauseUpdateCheckBox.Name = "pauseUpdateCheckBox";
			this.pauseUpdateCheckBox.Size = new System.Drawing.Size(56, 17);
			this.pauseUpdateCheckBox.TabIndex = 1;
			this.pauseUpdateCheckBox.Text = "Pause";
			this.pauseUpdateCheckBox.UseVisualStyleBackColor = true;
			// 
			// frameUpdateCheckBox
			// 
			this.frameUpdateCheckBox.AutoSize = true;
			this.frameUpdateCheckBox.Location = new System.Drawing.Point(6, 19);
			this.frameUpdateCheckBox.Name = "frameUpdateCheckBox";
			this.frameUpdateCheckBox.Size = new System.Drawing.Size(77, 17);
			this.frameUpdateCheckBox.TabIndex = 0;
			this.frameUpdateCheckBox.Text = "New frame";
			this.frameUpdateCheckBox.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.panel1);
			this.groupBox1.Location = new System.Drawing.Point(195, 195);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(76, 89);
			this.groupBox1.TabIndex = 8;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Tile View";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.panel1.Location = new System.Drawing.Point(6, 19);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(64, 64);
			this.panel1.TabIndex = 0;
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
			this.informationGroupBox.Location = new System.Drawing.Point(109, 3);
			this.informationGroupBox.Name = "informationGroupBox";
			this.informationGroupBox.Size = new System.Drawing.Size(162, 136);
			this.informationGroupBox.TabIndex = 7;
			this.informationGroupBox.TabStop = false;
			this.informationGroupBox.Text = "Information";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(80, 118);
			this.label14.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(10, 13);
			this.label14.TabIndex = 13;
			this.label14.Text = "-";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(80, 101);
			this.label12.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(10, 13);
			this.label12.TabIndex = 11;
			this.label12.Text = "-";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(80, 84);
			this.label10.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(10, 13);
			this.label10.TabIndex = 9;
			this.label10.Text = "-";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(80, 67);
			this.label8.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(10, 13);
			this.label8.TabIndex = 7;
			this.label8.Text = "-";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(80, 50);
			this.label7.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(10, 13);
			this.label7.TabIndex = 6;
			this.label7.Text = "-";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(80, 33);
			this.label6.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(10, 13);
			this.label6.TabIndex = 5;
			this.label6.Text = "-";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(80, 16);
			this.label5.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(10, 13);
			this.label5.TabIndex = 4;
			this.label5.Text = "-";
			// 
			// mapDataGroupBox
			// 
			this.mapDataGroupBox.Controls.Add(this.mapData1RadioButton);
			this.mapDataGroupBox.Controls.Add(this.mapData0RadioButton);
			this.mapDataGroupBox.Location = new System.Drawing.Point(3, 74);
			this.mapDataGroupBox.Name = "mapDataGroupBox";
			this.mapDataGroupBox.Size = new System.Drawing.Size(100, 65);
			this.mapDataGroupBox.TabIndex = 6;
			this.mapDataGroupBox.TabStop = false;
			this.mapDataGroupBox.Text = "Map Data";
			// 
			// mapData1RadioButton
			// 
			this.mapData1RadioButton.AutoSize = true;
			this.mapData1RadioButton.Location = new System.Drawing.Point(6, 42);
			this.mapData1RadioButton.Name = "mapData1RadioButton";
			this.mapData1RadioButton.Size = new System.Drawing.Size(77, 17);
			this.mapData1RadioButton.TabIndex = 1;
			this.mapData1RadioButton.TabStop = true;
			this.mapData1RadioButton.Text = "9C00-9FFF";
			this.mapData1RadioButton.UseVisualStyleBackColor = true;
			// 
			// mapData0RadioButton
			// 
			this.mapData0RadioButton.AutoSize = true;
			this.mapData0RadioButton.Checked = true;
			this.mapData0RadioButton.Location = new System.Drawing.Point(6, 19);
			this.mapData0RadioButton.Name = "mapData0RadioButton";
			this.mapData0RadioButton.Size = new System.Drawing.Size(77, 17);
			this.mapData0RadioButton.TabIndex = 0;
			this.mapData0RadioButton.TabStop = true;
			this.mapData0RadioButton.Text = "9800-9BFF";
			this.mapData0RadioButton.UseVisualStyleBackColor = true;
			this.mapData0RadioButton.CheckedChanged += new System.EventHandler(this.dataRadioButton_CheckedChanged);
			// 
			// tileDataGroupBox
			// 
			this.tileDataGroupBox.Controls.Add(this.tileData1RadioButton);
			this.tileDataGroupBox.Controls.Add(this.tileData0RadioButton);
			this.tileDataGroupBox.Location = new System.Drawing.Point(3, 3);
			this.tileDataGroupBox.Name = "tileDataGroupBox";
			this.tileDataGroupBox.Size = new System.Drawing.Size(100, 65);
			this.tileDataGroupBox.TabIndex = 5;
			this.tileDataGroupBox.TabStop = false;
			this.tileDataGroupBox.Text = "Character Data";
			// 
			// tileData1RadioButton
			// 
			this.tileData1RadioButton.AutoSize = true;
			this.tileData1RadioButton.Location = new System.Drawing.Point(6, 42);
			this.tileData1RadioButton.Name = "tileData1RadioButton";
			this.tileData1RadioButton.Size = new System.Drawing.Size(76, 17);
			this.tileData1RadioButton.TabIndex = 1;
			this.tileData1RadioButton.TabStop = true;
			this.tileData1RadioButton.Text = "8800-97FF";
			this.tileData1RadioButton.UseVisualStyleBackColor = true;
			// 
			// tileData0RadioButton
			// 
			this.tileData0RadioButton.AutoSize = true;
			this.tileData0RadioButton.Checked = true;
			this.tileData0RadioButton.Location = new System.Drawing.Point(6, 19);
			this.tileData0RadioButton.Name = "tileData0RadioButton";
			this.tileData0RadioButton.Size = new System.Drawing.Size(76, 17);
			this.tileData0RadioButton.TabIndex = 0;
			this.tileData0RadioButton.TabStop = true;
			this.tileData0RadioButton.Text = "8000-8FFF";
			this.tileData0RadioButton.UseVisualStyleBackColor = true;
			this.tileData0RadioButton.CheckedChanged += new System.EventHandler(this.dataRadioButton_CheckedChanged);
			// 
			// MapViewerForm
			// 
			this.AcceptButton = this.refreshButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(580, 366);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.refreshButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MapViewerForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Map Viewer";
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