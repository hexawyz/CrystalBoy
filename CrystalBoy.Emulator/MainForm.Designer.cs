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
	partial class MainForm
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
			if (disposing)
			{
				if (components != null) components.Dispose();
				components = null;
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
			System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
			System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
			System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
			System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
			System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
			this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.frameRateToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.emulationStatusToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.romInformationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.emulationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.runFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.limitSpeedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.audioToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.audioEnabledToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.muteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripSeparator();
			this.audioRendererToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.videoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.borderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.borderAutoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripSeparator();
			this.borderOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.borderOffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripSeparator();
			this.zoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.zoom100toolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.zoom200toolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.zoom300toolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.zoom400toolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.videoRendererToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.interpolationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.hardwareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.useBootstrapRomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
			this.gameBoyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.superGameBoyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gameBoyPocketToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.superGameBoy2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gameBoyColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.superGameBoyColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gameBoyAdvanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.superGameBoyAdvanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.joypadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.debuggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.memoryViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tileViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mapViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.oamViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripContainer.BottomToolStripPanel.SuspendLayout();
			this.toolStripContainer.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.mainMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripMenuItem1
			// 
			toolStripMenuItem1.Name = "toolStripMenuItem1";
			resources.ApplyResources(toolStripMenuItem1, "toolStripMenuItem1");
			// 
			// toolStripContainer
			// 
			// 
			// toolStripContainer.BottomToolStripPanel
			// 
			this.toolStripContainer.BottomToolStripPanel.Controls.Add(this.statusStrip);
			// 
			// toolStripContainer.ContentPanel
			// 
			resources.ApplyResources(this.toolStripContainer.ContentPanel, "toolStripContainer.ContentPanel");
			this.toolStripContainer.ContentPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.toolStripContainer_ContentPanel_Paint);
			resources.ApplyResources(this.toolStripContainer, "toolStripContainer");
			this.toolStripContainer.Name = "toolStripContainer";
			// 
			// toolStripContainer.TopToolStripPanel
			// 
			this.toolStripContainer.TopToolStripPanel.Controls.Add(this.mainMenuStrip);
			// 
			// statusStrip
			// 
			resources.ApplyResources(this.statusStrip, "statusStrip");
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.frameRateToolStripStatusLabel,
            this.emulationStatusToolStripStatusLabel});
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			// 
			// toolStripStatusLabel
			// 
			this.toolStripStatusLabel.Name = "toolStripStatusLabel";
			resources.ApplyResources(this.toolStripStatusLabel, "toolStripStatusLabel");
			this.toolStripStatusLabel.Spring = true;
			// 
			// frameRateToolStripStatusLabel
			// 
			resources.ApplyResources(this.frameRateToolStripStatusLabel, "frameRateToolStripStatusLabel");
			this.frameRateToolStripStatusLabel.Name = "frameRateToolStripStatusLabel";
			// 
			// emulationStatusToolStripStatusLabel
			// 
			resources.ApplyResources(this.emulationStatusToolStripStatusLabel, "emulationStatusToolStripStatusLabel");
			this.emulationStatusToolStripStatusLabel.Name = "emulationStatusToolStripStatusLabel";
			// 
			// mainMenuStrip
			// 
			resources.ApplyResources(this.mainMenuStrip, "mainMenuStrip");
			this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.emulationToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.mainMenuStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			this.mainMenuStrip.Name = "mainMenuStrip";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            toolStripMenuItem1,
            this.romInformationToolStripMenuItem,
            toolStripMenuItem2,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// romInformationToolStripMenuItem
			// 
			this.romInformationToolStripMenuItem.Name = "romInformationToolStripMenuItem";
			resources.ApplyResources(this.romInformationToolStripMenuItem, "romInformationToolStripMenuItem");
			this.romInformationToolStripMenuItem.Click += new System.EventHandler(this.romInformationToolStripMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			toolStripMenuItem2.Name = "toolStripMenuItem2";
			resources.ApplyResources(toolStripMenuItem2, "toolStripMenuItem2");
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// emulationToolStripMenuItem
			// 
			this.emulationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pauseToolStripMenuItem,
            this.resetToolStripMenuItem,
            toolStripMenuItem3,
            this.runFrameToolStripMenuItem,
            this.limitSpeedToolStripMenuItem,
            toolStripMenuItem4,
            this.audioToolStripMenuItem,
            this.videoToolStripMenuItem,
            this.hardwareToolStripMenuItem,
            this.joypadToolStripMenuItem,
            toolStripMenuItem7,
            this.optionsToolStripMenuItem});
			this.emulationToolStripMenuItem.Name = "emulationToolStripMenuItem";
			resources.ApplyResources(this.emulationToolStripMenuItem, "emulationToolStripMenuItem");
			this.emulationToolStripMenuItem.DropDownOpening += new System.EventHandler(this.emulationToolStripMenuItem_DropDownOpening);
			// 
			// pauseToolStripMenuItem
			// 
			this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
			resources.ApplyResources(this.pauseToolStripMenuItem, "pauseToolStripMenuItem");
			this.pauseToolStripMenuItem.Click += new System.EventHandler(this.pauseToolStripMenuItem_Click);
			// 
			// resetToolStripMenuItem
			// 
			this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
			resources.ApplyResources(this.resetToolStripMenuItem, "resetToolStripMenuItem");
			this.resetToolStripMenuItem.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);
			// 
			// toolStripMenuItem3
			// 
			toolStripMenuItem3.Name = "toolStripMenuItem3";
			resources.ApplyResources(toolStripMenuItem3, "toolStripMenuItem3");
			// 
			// runFrameToolStripMenuItem
			// 
			this.runFrameToolStripMenuItem.Name = "runFrameToolStripMenuItem";
			resources.ApplyResources(this.runFrameToolStripMenuItem, "runFrameToolStripMenuItem");
			this.runFrameToolStripMenuItem.Click += new System.EventHandler(this.runFrameToolStripMenuItem_Click);
			// 
			// limitSpeedToolStripMenuItem
			// 
			this.limitSpeedToolStripMenuItem.Checked = true;
			this.limitSpeedToolStripMenuItem.CheckOnClick = true;
			this.limitSpeedToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.limitSpeedToolStripMenuItem.Name = "limitSpeedToolStripMenuItem";
			resources.ApplyResources(this.limitSpeedToolStripMenuItem, "limitSpeedToolStripMenuItem");
			this.limitSpeedToolStripMenuItem.Click += new System.EventHandler(this.limitSpeedToolStripMenuItem_Click);
			// 
			// toolStripMenuItem4
			// 
			toolStripMenuItem4.Name = "toolStripMenuItem4";
			resources.ApplyResources(toolStripMenuItem4, "toolStripMenuItem4");
			// 
			// audioToolStripMenuItem
			// 
			this.audioToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.audioEnabledToolStripMenuItem,
            this.muteToolStripMenuItem,
            this.toolStripMenuItem10,
            this.audioRendererToolStripMenuItem});
			this.audioToolStripMenuItem.Name = "audioToolStripMenuItem";
			resources.ApplyResources(this.audioToolStripMenuItem, "audioToolStripMenuItem");
			// 
			// audioEnabledToolStripMenuItem
			// 
			this.audioEnabledToolStripMenuItem.Name = "audioEnabledToolStripMenuItem";
			resources.ApplyResources(this.audioEnabledToolStripMenuItem, "audioEnabledToolStripMenuItem");
			// 
			// muteToolStripMenuItem
			// 
			this.muteToolStripMenuItem.Name = "muteToolStripMenuItem";
			resources.ApplyResources(this.muteToolStripMenuItem, "muteToolStripMenuItem");
			// 
			// toolStripMenuItem10
			// 
			this.toolStripMenuItem10.Name = "toolStripMenuItem10";
			resources.ApplyResources(this.toolStripMenuItem10, "toolStripMenuItem10");
			// 
			// audioRendererToolStripMenuItem
			// 
			this.audioRendererToolStripMenuItem.Name = "audioRendererToolStripMenuItem";
			resources.ApplyResources(this.audioRendererToolStripMenuItem, "audioRendererToolStripMenuItem");
			// 
			// videoToolStripMenuItem
			// 
			this.videoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.borderToolStripMenuItem,
            this.toolStripMenuItem9,
            this.zoomToolStripMenuItem,
            this.videoRendererToolStripMenuItem,
            toolStripMenuItem5,
            this.interpolationToolStripMenuItem});
			this.videoToolStripMenuItem.Name = "videoToolStripMenuItem";
			resources.ApplyResources(this.videoToolStripMenuItem, "videoToolStripMenuItem");
			this.videoToolStripMenuItem.DropDownOpening += new System.EventHandler(this.videoToolStripMenuItem_DropDownOpening);
			// 
			// borderToolStripMenuItem
			// 
			this.borderToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.borderAutoToolStripMenuItem,
            this.toolStripMenuItem8,
            this.borderOnToolStripMenuItem,
            this.borderOffToolStripMenuItem});
			this.borderToolStripMenuItem.Name = "borderToolStripMenuItem";
			resources.ApplyResources(this.borderToolStripMenuItem, "borderToolStripMenuItem");
			this.borderToolStripMenuItem.DropDownOpening += new System.EventHandler(this.borderToolStripMenuItem_DropDownOpening);
			// 
			// borderAutoToolStripMenuItem
			// 
			this.borderAutoToolStripMenuItem.Name = "borderAutoToolStripMenuItem";
			resources.ApplyResources(this.borderAutoToolStripMenuItem, "borderAutoToolStripMenuItem");
			this.borderAutoToolStripMenuItem.Click += new System.EventHandler(this.borderAutoToolStripMenuItem_Click);
			// 
			// toolStripMenuItem8
			// 
			this.toolStripMenuItem8.Name = "toolStripMenuItem8";
			resources.ApplyResources(this.toolStripMenuItem8, "toolStripMenuItem8");
			// 
			// borderOnToolStripMenuItem
			// 
			this.borderOnToolStripMenuItem.Name = "borderOnToolStripMenuItem";
			resources.ApplyResources(this.borderOnToolStripMenuItem, "borderOnToolStripMenuItem");
			this.borderOnToolStripMenuItem.Click += new System.EventHandler(this.borderOnToolStripMenuItem_Click);
			// 
			// borderOffToolStripMenuItem
			// 
			this.borderOffToolStripMenuItem.Name = "borderOffToolStripMenuItem";
			resources.ApplyResources(this.borderOffToolStripMenuItem, "borderOffToolStripMenuItem");
			this.borderOffToolStripMenuItem.Click += new System.EventHandler(this.borderOffToolStripMenuItem_Click);
			// 
			// toolStripMenuItem9
			// 
			this.toolStripMenuItem9.Name = "toolStripMenuItem9";
			resources.ApplyResources(this.toolStripMenuItem9, "toolStripMenuItem9");
			// 
			// zoomToolStripMenuItem
			// 
			this.zoomToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoom100toolStripMenuItem,
            this.zoom200toolStripMenuItem,
            this.zoom300toolStripMenuItem,
            this.zoom400toolStripMenuItem});
			this.zoomToolStripMenuItem.Name = "zoomToolStripMenuItem";
			resources.ApplyResources(this.zoomToolStripMenuItem, "zoomToolStripMenuItem");
			this.zoomToolStripMenuItem.DropDownOpening += new System.EventHandler(this.zoomToolStripMenuItem_DropDownOpening);
			// 
			// zoom100toolStripMenuItem
			// 
			this.zoom100toolStripMenuItem.Name = "zoom100toolStripMenuItem";
			resources.ApplyResources(this.zoom100toolStripMenuItem, "zoom100toolStripMenuItem");
			this.zoom100toolStripMenuItem.Click += new System.EventHandler(this.zoom100toolStripMenuItem_Click);
			// 
			// zoom200toolStripMenuItem
			// 
			this.zoom200toolStripMenuItem.Name = "zoom200toolStripMenuItem";
			resources.ApplyResources(this.zoom200toolStripMenuItem, "zoom200toolStripMenuItem");
			this.zoom200toolStripMenuItem.Click += new System.EventHandler(this.zoom200toolStripMenuItem_Click);
			// 
			// zoom300toolStripMenuItem
			// 
			this.zoom300toolStripMenuItem.Name = "zoom300toolStripMenuItem";
			resources.ApplyResources(this.zoom300toolStripMenuItem, "zoom300toolStripMenuItem");
			this.zoom300toolStripMenuItem.Click += new System.EventHandler(this.zoom300toolStripMenuItem_Click);
			// 
			// zoom400toolStripMenuItem
			// 
			this.zoom400toolStripMenuItem.Name = "zoom400toolStripMenuItem";
			resources.ApplyResources(this.zoom400toolStripMenuItem, "zoom400toolStripMenuItem");
			this.zoom400toolStripMenuItem.Click += new System.EventHandler(this.zoom400toolStripMenuItem_Click);
			// 
			// videoRendererToolStripMenuItem
			// 
			this.videoRendererToolStripMenuItem.Name = "videoRendererToolStripMenuItem";
			resources.ApplyResources(this.videoRendererToolStripMenuItem, "videoRendererToolStripMenuItem");
			// 
			// toolStripMenuItem5
			// 
			toolStripMenuItem5.Name = "toolStripMenuItem5";
			resources.ApplyResources(toolStripMenuItem5, "toolStripMenuItem5");
			// 
			// interpolationToolStripMenuItem
			// 
			this.interpolationToolStripMenuItem.Name = "interpolationToolStripMenuItem";
			resources.ApplyResources(this.interpolationToolStripMenuItem, "interpolationToolStripMenuItem");
			this.interpolationToolStripMenuItem.Click += new System.EventHandler(this.interpolationToolStripMenuItem_Click);
			// 
			// hardwareToolStripMenuItem
			// 
			this.hardwareToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.useBootstrapRomToolStripMenuItem,
            this.toolStripMenuItem6,
            this.gameBoyToolStripMenuItem,
            this.superGameBoyToolStripMenuItem,
            this.gameBoyPocketToolStripMenuItem,
            this.superGameBoy2ToolStripMenuItem,
            this.gameBoyColorToolStripMenuItem,
            this.superGameBoyColorToolStripMenuItem,
            this.gameBoyAdvanceToolStripMenuItem,
            this.superGameBoyAdvanceToolStripMenuItem});
			this.hardwareToolStripMenuItem.Name = "hardwareToolStripMenuItem";
			resources.ApplyResources(this.hardwareToolStripMenuItem, "hardwareToolStripMenuItem");
			this.hardwareToolStripMenuItem.DropDownOpening += new System.EventHandler(this.hardwareToolStripMenuItem_DropDownOpening);
			// 
			// useBootstrapRomToolStripMenuItem
			// 
			this.useBootstrapRomToolStripMenuItem.Name = "useBootstrapRomToolStripMenuItem";
			resources.ApplyResources(this.useBootstrapRomToolStripMenuItem, "useBootstrapRomToolStripMenuItem");
			this.useBootstrapRomToolStripMenuItem.Click += new System.EventHandler(this.useBootstrapRomToolStripMenuItem_Click);
			// 
			// toolStripMenuItem6
			// 
			this.toolStripMenuItem6.Name = "toolStripMenuItem6";
			resources.ApplyResources(this.toolStripMenuItem6, "toolStripMenuItem6");
			// 
			// gameBoyToolStripMenuItem
			// 
			this.gameBoyToolStripMenuItem.Name = "gameBoyToolStripMenuItem";
			resources.ApplyResources(this.gameBoyToolStripMenuItem, "gameBoyToolStripMenuItem");
			this.gameBoyToolStripMenuItem.Tag = CrystalBoy.Emulation.HardwareType.GameBoy;
			this.gameBoyToolStripMenuItem.Click += new System.EventHandler(this.randomHardwareToolStripMenuItem_Click);
			// 
			// superGameBoyToolStripMenuItem
			// 
			this.superGameBoyToolStripMenuItem.Name = "superGameBoyToolStripMenuItem";
			resources.ApplyResources(this.superGameBoyToolStripMenuItem, "superGameBoyToolStripMenuItem");
			this.superGameBoyToolStripMenuItem.Tag = CrystalBoy.Emulation.HardwareType.SuperGameBoy;
			this.superGameBoyToolStripMenuItem.Click += new System.EventHandler(this.randomHardwareToolStripMenuItem_Click);
			// 
			// gameBoyPocketToolStripMenuItem
			// 
			this.gameBoyPocketToolStripMenuItem.Name = "gameBoyPocketToolStripMenuItem";
			resources.ApplyResources(this.gameBoyPocketToolStripMenuItem, "gameBoyPocketToolStripMenuItem");
			this.gameBoyPocketToolStripMenuItem.Tag = CrystalBoy.Emulation.HardwareType.GameBoyPocket;
			this.gameBoyPocketToolStripMenuItem.Click += new System.EventHandler(this.randomHardwareToolStripMenuItem_Click);
			// 
			// superGameBoy2ToolStripMenuItem
			// 
			this.superGameBoy2ToolStripMenuItem.Name = "superGameBoy2ToolStripMenuItem";
			resources.ApplyResources(this.superGameBoy2ToolStripMenuItem, "superGameBoy2ToolStripMenuItem");
			this.superGameBoy2ToolStripMenuItem.Tag = CrystalBoy.Emulation.HardwareType.SuperGameBoy2;
			this.superGameBoy2ToolStripMenuItem.Click += new System.EventHandler(this.randomHardwareToolStripMenuItem_Click);
			// 
			// gameBoyColorToolStripMenuItem
			// 
			this.gameBoyColorToolStripMenuItem.Name = "gameBoyColorToolStripMenuItem";
			resources.ApplyResources(this.gameBoyColorToolStripMenuItem, "gameBoyColorToolStripMenuItem");
			this.gameBoyColorToolStripMenuItem.Tag = CrystalBoy.Emulation.HardwareType.GameBoyColor;
			this.gameBoyColorToolStripMenuItem.Click += new System.EventHandler(this.randomHardwareToolStripMenuItem_Click);
			// 
			// superGameBoyColorToolStripMenuItem
			// 
			this.superGameBoyColorToolStripMenuItem.Name = "superGameBoyColorToolStripMenuItem";
			resources.ApplyResources(this.superGameBoyColorToolStripMenuItem, "superGameBoyColorToolStripMenuItem");
			this.superGameBoyColorToolStripMenuItem.Tag = CrystalBoy.Emulation.HardwareType.SuperGameBoyColor;
			this.superGameBoyColorToolStripMenuItem.Click += new System.EventHandler(this.randomHardwareToolStripMenuItem_Click);
			// 
			// gameBoyAdvanceToolStripMenuItem
			// 
			this.gameBoyAdvanceToolStripMenuItem.Name = "gameBoyAdvanceToolStripMenuItem";
			resources.ApplyResources(this.gameBoyAdvanceToolStripMenuItem, "gameBoyAdvanceToolStripMenuItem");
			this.gameBoyAdvanceToolStripMenuItem.Tag = CrystalBoy.Emulation.HardwareType.GameBoyAdvance;
			this.gameBoyAdvanceToolStripMenuItem.Click += new System.EventHandler(this.randomHardwareToolStripMenuItem_Click);
			// 
			// superGameBoyAdvanceToolStripMenuItem
			// 
			this.superGameBoyAdvanceToolStripMenuItem.Name = "superGameBoyAdvanceToolStripMenuItem";
			resources.ApplyResources(this.superGameBoyAdvanceToolStripMenuItem, "superGameBoyAdvanceToolStripMenuItem");
			this.superGameBoyAdvanceToolStripMenuItem.Tag = CrystalBoy.Emulation.HardwareType.SuperGameBoyAdvance;
			this.superGameBoyAdvanceToolStripMenuItem.Click += new System.EventHandler(this.randomHardwareToolStripMenuItem_Click);
			// 
			// joypadToolStripMenuItem
			// 
			this.joypadToolStripMenuItem.Name = "joypadToolStripMenuItem";
			resources.ApplyResources(this.joypadToolStripMenuItem, "joypadToolStripMenuItem");
			// 
			// toolStripMenuItem7
			// 
			toolStripMenuItem7.Name = "toolStripMenuItem7";
			resources.ApplyResources(toolStripMenuItem7, "toolStripMenuItem7");
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			resources.ApplyResources(this.optionsToolStripMenuItem, "optionsToolStripMenuItem");
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debuggerToolStripMenuItem,
            this.memoryViewerToolStripMenuItem,
            this.tileViewerToolStripMenuItem,
            this.mapViewerToolStripMenuItem,
            this.oamViewerToolStripMenuItem});
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			resources.ApplyResources(this.toolsToolStripMenuItem, "toolsToolStripMenuItem");
			// 
			// debuggerToolStripMenuItem
			// 
			this.debuggerToolStripMenuItem.Name = "debuggerToolStripMenuItem";
			resources.ApplyResources(this.debuggerToolStripMenuItem, "debuggerToolStripMenuItem");
			this.debuggerToolStripMenuItem.Click += new System.EventHandler(this.debuggerToolStripMenuItem_Click);
			// 
			// memoryViewerToolStripMenuItem
			// 
			this.memoryViewerToolStripMenuItem.Name = "memoryViewerToolStripMenuItem";
			resources.ApplyResources(this.memoryViewerToolStripMenuItem, "memoryViewerToolStripMenuItem");
			// 
			// tileViewerToolStripMenuItem
			// 
			this.tileViewerToolStripMenuItem.Name = "tileViewerToolStripMenuItem";
			resources.ApplyResources(this.tileViewerToolStripMenuItem, "tileViewerToolStripMenuItem");
			this.tileViewerToolStripMenuItem.Click += new System.EventHandler(this.tileViewerToolStripMenuItem_Click);
			// 
			// mapViewerToolStripMenuItem
			// 
			this.mapViewerToolStripMenuItem.Name = "mapViewerToolStripMenuItem";
			resources.ApplyResources(this.mapViewerToolStripMenuItem, "mapViewerToolStripMenuItem");
			this.mapViewerToolStripMenuItem.Click += new System.EventHandler(this.mapViewerToolStripMenuItem_Click);
			// 
			// oamViewerToolStripMenuItem
			// 
			this.oamViewerToolStripMenuItem.Name = "oamViewerToolStripMenuItem";
			resources.ApplyResources(this.oamViewerToolStripMenuItem, "oamViewerToolStripMenuItem");
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "gb";
			resources.ApplyResources(this.openFileDialog, "openFileDialog");
			// 
			// MainForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.toolStripContainer);
			this.MainMenuStrip = this.mainMenuStrip;
			this.Name = "MainForm";
			this.toolStripContainer.BottomToolStripPanel.ResumeLayout(false);
			this.toolStripContainer.BottomToolStripPanel.PerformLayout();
			this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer.TopToolStripPanel.PerformLayout();
			this.toolStripContainer.ResumeLayout(false);
			this.toolStripContainer.PerformLayout();
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.mainMenuStrip.ResumeLayout(false);
			this.mainMenuStrip.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.MenuStrip mainMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem debuggerToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem memoryViewerToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem tileViewerToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mapViewerToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem oamViewerToolStripMenuItem;
		private System.Windows.Forms.ToolStripContainer toolStripContainer;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel frameRateToolStripStatusLabel;
		private System.Windows.Forms.ToolStripMenuItem emulationToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pauseToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel emulationStatusToolStripStatusLabel;
		private System.Windows.Forms.ToolStripMenuItem videoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem hardwareToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gameBoyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gameBoyPocketToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gameBoyColorToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem superGameBoyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem superGameBoy2ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem romInformationToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem videoRendererToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoomToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoom100toolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoom200toolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoom300toolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoom400toolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gameBoyAdvanceToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem interpolationToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem joypadToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem audioToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem audioEnabledToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem muteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem limitSpeedToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem runFrameToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem useBootstrapRomToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
		private System.Windows.Forms.ToolStripMenuItem superGameBoyColorToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem superGameBoyAdvanceToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem borderToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem borderAutoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem borderOffToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem9;
		private System.Windows.Forms.ToolStripMenuItem borderOnToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem8;
		private System.Windows.Forms.ToolStripMenuItem audioRendererToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem10;
	}
}

