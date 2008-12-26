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
	partial class TileViewerForm
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
			this.refreshButton = new System.Windows.Forms.Button();
			this.tileSet0GroupBox = new System.Windows.Forms.GroupBox();
			this.tileSet1GroupBox = new System.Windows.Forms.GroupBox();
			this.closeButton = new System.Windows.Forms.Button();
			this.tileSet0Panel = new CrystalBoy.Emulator.BitmapPanel();
			this.tileSet1Panel = new CrystalBoy.Emulator.BitmapPanel();
			this.tileSet0GroupBox.SuspendLayout();
			this.tileSet1GroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// refreshButton
			// 
			this.refreshButton.Location = new System.Drawing.Point(142, 235);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = new System.Drawing.Size(75, 23);
			this.refreshButton.TabIndex = 2;
			this.refreshButton.Text = "&Refresh";
			this.refreshButton.UseVisualStyleBackColor = true;
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// tileSet0GroupBox
			// 
			this.tileSet0GroupBox.Controls.Add(this.tileSet0Panel);
			this.tileSet0GroupBox.Location = new System.Drawing.Point(12, 12);
			this.tileSet0GroupBox.Name = "tileSet0GroupBox";
			this.tileSet0GroupBox.Size = new System.Drawing.Size(140, 217);
			this.tileSet0GroupBox.TabIndex = 0;
			this.tileSet0GroupBox.TabStop = false;
			this.tileSet0GroupBox.Text = "Bank 0";
			// 
			// tileSet1GroupBox
			// 
			this.tileSet1GroupBox.Controls.Add(this.tileSet1Panel);
			this.tileSet1GroupBox.Location = new System.Drawing.Point(158, 12);
			this.tileSet1GroupBox.Name = "tileSet1GroupBox";
			this.tileSet1GroupBox.Size = new System.Drawing.Size(140, 217);
			this.tileSet1GroupBox.TabIndex = 1;
			this.tileSet1GroupBox.TabStop = false;
			this.tileSet1GroupBox.Text = "Bank 1";
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(223, 235);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(75, 23);
			this.closeButton.TabIndex = 3;
			this.closeButton.Text = "&Close";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// tileSet0Panel
			// 
			this.tileSet0Panel.Bitmap = null;
			this.tileSet0Panel.Location = new System.Drawing.Point(6, 19);
			this.tileSet0Panel.Name = "tileSet0Panel";
			this.tileSet0Panel.Size = new System.Drawing.Size(128, 192);
			this.tileSet0Panel.TabIndex = 0;
			this.tileSet0Panel.Text = "bitmapPanel1";
			// 
			// tileSet1Panel
			// 
			this.tileSet1Panel.Bitmap = null;
			this.tileSet1Panel.Location = new System.Drawing.Point(6, 19);
			this.tileSet1Panel.Name = "tileSet1Panel";
			this.tileSet1Panel.Size = new System.Drawing.Size(128, 192);
			this.tileSet1Panel.TabIndex = 0;
			this.tileSet1Panel.Text = "bitmapPanel1";
			// 
			// TileViewerForm
			// 
			this.AcceptButton = this.refreshButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(310, 270);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.tileSet1GroupBox);
			this.Controls.Add(this.tileSet0GroupBox);
			this.Controls.Add(this.refreshButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TileViewerForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Tile Viewer";
			this.tileSet0GroupBox.ResumeLayout(false);
			this.tileSet1GroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button refreshButton;
		private System.Windows.Forms.GroupBox tileSet0GroupBox;
		private System.Windows.Forms.GroupBox tileSet1GroupBox;
		private System.Windows.Forms.Button closeButton;
		private BitmapPanel tileSet0Panel;
		private BitmapPanel tileSet1Panel;
	}
}