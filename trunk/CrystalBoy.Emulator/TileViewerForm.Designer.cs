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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TileViewerForm));
			this.refreshButton = new System.Windows.Forms.Button();
			this.tileSet0GroupBox = new System.Windows.Forms.GroupBox();
			this.tileSet0Panel = new CrystalBoy.Emulator.BitmapPanel();
			this.tileSet1GroupBox = new System.Windows.Forms.GroupBox();
			this.tileSet1Panel = new CrystalBoy.Emulator.BitmapPanel();
			this.closeButton = new System.Windows.Forms.Button();
			this.tileSet0GroupBox.SuspendLayout();
			this.tileSet1GroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// refreshButton
			// 
			resources.ApplyResources(this.refreshButton, "refreshButton");
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.UseVisualStyleBackColor = true;
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// tileSet0GroupBox
			// 
			resources.ApplyResources(this.tileSet0GroupBox, "tileSet0GroupBox");
			this.tileSet0GroupBox.Controls.Add(this.tileSet0Panel);
			this.tileSet0GroupBox.Name = "tileSet0GroupBox";
			this.tileSet0GroupBox.TabStop = false;
			// 
			// tileSet0Panel
			// 
			resources.ApplyResources(this.tileSet0Panel, "tileSet0Panel");
			this.tileSet0Panel.Name = "tileSet0Panel";
			// 
			// tileSet1GroupBox
			// 
			resources.ApplyResources(this.tileSet1GroupBox, "tileSet1GroupBox");
			this.tileSet1GroupBox.Controls.Add(this.tileSet1Panel);
			this.tileSet1GroupBox.Name = "tileSet1GroupBox";
			this.tileSet1GroupBox.TabStop = false;
			// 
			// tileSet1Panel
			// 
			resources.ApplyResources(this.tileSet1Panel, "tileSet1Panel");
			this.tileSet1Panel.Name = "tileSet1Panel";
			// 
			// closeButton
			// 
			resources.ApplyResources(this.closeButton, "closeButton");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Name = "closeButton";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// TileViewerForm
			// 
			this.AcceptButton = this.refreshButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.closeButton;
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