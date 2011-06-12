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
	partial class DebuggerForm
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
			System.Windows.Forms.GroupBox registersGroupBox;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebuggerForm));
			System.Windows.Forms.Label ieLabel;
			System.Windows.Forms.Label ifLabel;
			System.Windows.Forms.Label pcLabel;
			System.Windows.Forms.Label spLabel;
			System.Windows.Forms.Label hlLabel;
			System.Windows.Forms.Label deLabel;
			System.Windows.Forms.Label bcLabel;
			System.Windows.Forms.Label afLabel;
			System.Windows.Forms.GroupBox flagsGroupBox;
			System.Windows.Forms.GroupBox debugGroupBox;
			this.ieValueLabel = new System.Windows.Forms.Label();
			this.ifValueLabel = new System.Windows.Forms.Label();
			this.pcValueLabel = new System.Windows.Forms.Label();
			this.spValueLabel = new System.Windows.Forms.Label();
			this.hlValueLabel = new System.Windows.Forms.Label();
			this.deValueLabel = new System.Windows.Forms.Label();
			this.bcValueLabel = new System.Windows.Forms.Label();
			this.afValueLabel = new System.Windows.Forms.Label();
			this.imeCheckBox = new System.Windows.Forms.CheckBox();
			this.carryFlagCheckBox = new System.Windows.Forms.CheckBox();
			this.halfCarryFlagCheckBox = new System.Windows.Forms.CheckBox();
			this.negationFlagCheckBox = new System.Windows.Forms.CheckBox();
			this.zeroFlagCheckBox = new System.Windows.Forms.CheckBox();
			this.stepButton = new System.Windows.Forms.Button();
			this.runButton = new System.Windows.Forms.Button();
			this.breakpointsGroupBox = new System.Windows.Forms.GroupBox();
			this.toggleBreakpointButton = new System.Windows.Forms.Button();
			this.clearBreakpointsButton = new System.Windows.Forms.Button();
			this.gotoLabel = new System.Windows.Forms.Label();
			this.gotoTextBox = new System.Windows.Forms.TextBox();
			this.gotoButton = new System.Windows.Forms.Button();
			this.disassemblyView = new CrystalBoy.Disassembly.Windows.Forms.DisassemblyView();
			registersGroupBox = new System.Windows.Forms.GroupBox();
			ieLabel = new System.Windows.Forms.Label();
			ifLabel = new System.Windows.Forms.Label();
			pcLabel = new System.Windows.Forms.Label();
			spLabel = new System.Windows.Forms.Label();
			hlLabel = new System.Windows.Forms.Label();
			deLabel = new System.Windows.Forms.Label();
			bcLabel = new System.Windows.Forms.Label();
			afLabel = new System.Windows.Forms.Label();
			flagsGroupBox = new System.Windows.Forms.GroupBox();
			debugGroupBox = new System.Windows.Forms.GroupBox();
			registersGroupBox.SuspendLayout();
			flagsGroupBox.SuspendLayout();
			debugGroupBox.SuspendLayout();
			this.breakpointsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// registersGroupBox
			// 
			resources.ApplyResources(registersGroupBox, "registersGroupBox");
			registersGroupBox.Controls.Add(this.ieValueLabel);
			registersGroupBox.Controls.Add(ieLabel);
			registersGroupBox.Controls.Add(this.ifValueLabel);
			registersGroupBox.Controls.Add(ifLabel);
			registersGroupBox.Controls.Add(this.pcValueLabel);
			registersGroupBox.Controls.Add(this.spValueLabel);
			registersGroupBox.Controls.Add(this.hlValueLabel);
			registersGroupBox.Controls.Add(this.deValueLabel);
			registersGroupBox.Controls.Add(this.bcValueLabel);
			registersGroupBox.Controls.Add(this.afValueLabel);
			registersGroupBox.Controls.Add(pcLabel);
			registersGroupBox.Controls.Add(spLabel);
			registersGroupBox.Controls.Add(hlLabel);
			registersGroupBox.Controls.Add(deLabel);
			registersGroupBox.Controls.Add(bcLabel);
			registersGroupBox.Controls.Add(afLabel);
			registersGroupBox.Name = "registersGroupBox";
			registersGroupBox.TabStop = false;
			// 
			// ieValueLabel
			// 
			resources.ApplyResources(this.ieValueLabel, "ieValueLabel");
			this.ieValueLabel.Name = "ieValueLabel";
			// 
			// ieLabel
			// 
			resources.ApplyResources(ieLabel, "ieLabel");
			ieLabel.Name = "ieLabel";
			// 
			// ifValueLabel
			// 
			resources.ApplyResources(this.ifValueLabel, "ifValueLabel");
			this.ifValueLabel.Name = "ifValueLabel";
			// 
			// ifLabel
			// 
			resources.ApplyResources(ifLabel, "ifLabel");
			ifLabel.Name = "ifLabel";
			// 
			// pcValueLabel
			// 
			resources.ApplyResources(this.pcValueLabel, "pcValueLabel");
			this.pcValueLabel.Name = "pcValueLabel";
			// 
			// spValueLabel
			// 
			resources.ApplyResources(this.spValueLabel, "spValueLabel");
			this.spValueLabel.Name = "spValueLabel";
			// 
			// hlValueLabel
			// 
			resources.ApplyResources(this.hlValueLabel, "hlValueLabel");
			this.hlValueLabel.Name = "hlValueLabel";
			// 
			// deValueLabel
			// 
			resources.ApplyResources(this.deValueLabel, "deValueLabel");
			this.deValueLabel.Name = "deValueLabel";
			// 
			// bcValueLabel
			// 
			resources.ApplyResources(this.bcValueLabel, "bcValueLabel");
			this.bcValueLabel.Name = "bcValueLabel";
			// 
			// afValueLabel
			// 
			resources.ApplyResources(this.afValueLabel, "afValueLabel");
			this.afValueLabel.Name = "afValueLabel";
			// 
			// pcLabel
			// 
			resources.ApplyResources(pcLabel, "pcLabel");
			pcLabel.Name = "pcLabel";
			// 
			// spLabel
			// 
			resources.ApplyResources(spLabel, "spLabel");
			spLabel.Name = "spLabel";
			// 
			// hlLabel
			// 
			resources.ApplyResources(hlLabel, "hlLabel");
			hlLabel.Name = "hlLabel";
			// 
			// deLabel
			// 
			resources.ApplyResources(deLabel, "deLabel");
			deLabel.Name = "deLabel";
			// 
			// bcLabel
			// 
			resources.ApplyResources(bcLabel, "bcLabel");
			bcLabel.Name = "bcLabel";
			// 
			// afLabel
			// 
			resources.ApplyResources(afLabel, "afLabel");
			afLabel.Name = "afLabel";
			// 
			// flagsGroupBox
			// 
			resources.ApplyResources(flagsGroupBox, "flagsGroupBox");
			flagsGroupBox.Controls.Add(this.imeCheckBox);
			flagsGroupBox.Controls.Add(this.carryFlagCheckBox);
			flagsGroupBox.Controls.Add(this.halfCarryFlagCheckBox);
			flagsGroupBox.Controls.Add(this.negationFlagCheckBox);
			flagsGroupBox.Controls.Add(this.zeroFlagCheckBox);
			flagsGroupBox.Name = "flagsGroupBox";
			flagsGroupBox.TabStop = false;
			// 
			// imeCheckBox
			// 
			resources.ApplyResources(this.imeCheckBox, "imeCheckBox");
			this.imeCheckBox.AutoCheck = false;
			this.imeCheckBox.Name = "imeCheckBox";
			this.imeCheckBox.UseVisualStyleBackColor = true;
			// 
			// carryFlagCheckBox
			// 
			resources.ApplyResources(this.carryFlagCheckBox, "carryFlagCheckBox");
			this.carryFlagCheckBox.AutoCheck = false;
			this.carryFlagCheckBox.Name = "carryFlagCheckBox";
			this.carryFlagCheckBox.UseVisualStyleBackColor = true;
			// 
			// halfCarryFlagCheckBox
			// 
			resources.ApplyResources(this.halfCarryFlagCheckBox, "halfCarryFlagCheckBox");
			this.halfCarryFlagCheckBox.AutoCheck = false;
			this.halfCarryFlagCheckBox.Name = "halfCarryFlagCheckBox";
			this.halfCarryFlagCheckBox.UseVisualStyleBackColor = true;
			// 
			// negationFlagCheckBox
			// 
			resources.ApplyResources(this.negationFlagCheckBox, "negationFlagCheckBox");
			this.negationFlagCheckBox.AutoCheck = false;
			this.negationFlagCheckBox.Name = "negationFlagCheckBox";
			this.negationFlagCheckBox.UseVisualStyleBackColor = true;
			// 
			// zeroFlagCheckBox
			// 
			resources.ApplyResources(this.zeroFlagCheckBox, "zeroFlagCheckBox");
			this.zeroFlagCheckBox.AutoCheck = false;
			this.zeroFlagCheckBox.Name = "zeroFlagCheckBox";
			this.zeroFlagCheckBox.UseVisualStyleBackColor = true;
			// 
			// debugGroupBox
			// 
			resources.ApplyResources(debugGroupBox, "debugGroupBox");
			debugGroupBox.Controls.Add(this.stepButton);
			debugGroupBox.Controls.Add(this.runButton);
			debugGroupBox.Name = "debugGroupBox";
			debugGroupBox.TabStop = false;
			// 
			// stepButton
			// 
			resources.ApplyResources(this.stepButton, "stepButton");
			this.stepButton.Name = "stepButton";
			this.stepButton.UseVisualStyleBackColor = true;
			this.stepButton.Click += new System.EventHandler(this.stepButton_Click);
			// 
			// runButton
			// 
			resources.ApplyResources(this.runButton, "runButton");
			this.runButton.Name = "runButton";
			this.runButton.UseVisualStyleBackColor = true;
			this.runButton.Click += new System.EventHandler(this.runButton_Click);
			// 
			// breakpointsGroupBox
			// 
			resources.ApplyResources(this.breakpointsGroupBox, "breakpointsGroupBox");
			this.breakpointsGroupBox.Controls.Add(this.toggleBreakpointButton);
			this.breakpointsGroupBox.Controls.Add(this.clearBreakpointsButton);
			this.breakpointsGroupBox.Name = "breakpointsGroupBox";
			this.breakpointsGroupBox.TabStop = false;
			// 
			// toggleBreakpointButton
			// 
			resources.ApplyResources(this.toggleBreakpointButton, "toggleBreakpointButton");
			this.toggleBreakpointButton.Name = "toggleBreakpointButton";
			this.toggleBreakpointButton.UseVisualStyleBackColor = true;
			this.toggleBreakpointButton.Click += new System.EventHandler(this.toggleBreakpointButton_Click);
			// 
			// clearBreakpointsButton
			// 
			resources.ApplyResources(this.clearBreakpointsButton, "clearBreakpointsButton");
			this.clearBreakpointsButton.Name = "clearBreakpointsButton";
			this.clearBreakpointsButton.UseVisualStyleBackColor = true;
			this.clearBreakpointsButton.Click += new System.EventHandler(this.clearBreakpointsButton_Click);
			// 
			// gotoLabel
			// 
			resources.ApplyResources(this.gotoLabel, "gotoLabel");
			this.gotoLabel.Name = "gotoLabel";
			// 
			// gotoTextBox
			// 
			resources.ApplyResources(this.gotoTextBox, "gotoTextBox");
			this.gotoTextBox.Name = "gotoTextBox";
			this.gotoTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gotoTextBox_KeyDown);
			// 
			// gotoButton
			// 
			resources.ApplyResources(this.gotoButton, "gotoButton");
			this.gotoButton.Name = "gotoButton";
			this.gotoButton.UseVisualStyleBackColor = true;
			this.gotoButton.Click += new System.EventHandler(this.gotoButton_Click);
			// 
			// disassemblyView
			// 
			resources.ApplyResources(this.disassemblyView, "disassemblyView");
			this.disassemblyView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.disassemblyView.Memory = null;
			this.disassemblyView.Name = "disassemblyView";
			this.disassemblyView.ShowCurrentInstruction = true;
			this.disassemblyView.DoubleClick += new System.EventHandler(this.disassemblyView_DoubleClick);
			// 
			// DebuggerForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.gotoButton);
			this.Controls.Add(this.gotoTextBox);
			this.Controls.Add(this.gotoLabel);
			this.Controls.Add(debugGroupBox);
			this.Controls.Add(this.breakpointsGroupBox);
			this.Controls.Add(flagsGroupBox);
			this.Controls.Add(registersGroupBox);
			this.Controls.Add(this.disassemblyView);
			this.MinimizeBox = false;
			this.Name = "DebuggerForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			registersGroupBox.ResumeLayout(false);
			registersGroupBox.PerformLayout();
			flagsGroupBox.ResumeLayout(false);
			flagsGroupBox.PerformLayout();
			debugGroupBox.ResumeLayout(false);
			this.breakpointsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CrystalBoy.Disassembly.Windows.Forms.DisassemblyView disassemblyView;
		private System.Windows.Forms.Label pcValueLabel;
		private System.Windows.Forms.Label spValueLabel;
		private System.Windows.Forms.Label hlValueLabel;
		private System.Windows.Forms.Label deValueLabel;
		private System.Windows.Forms.Label bcValueLabel;
		private System.Windows.Forms.Label afValueLabel;
		private System.Windows.Forms.CheckBox carryFlagCheckBox;
		private System.Windows.Forms.CheckBox halfCarryFlagCheckBox;
		private System.Windows.Forms.CheckBox negationFlagCheckBox;
		private System.Windows.Forms.CheckBox zeroFlagCheckBox;
		private System.Windows.Forms.Button toggleBreakpointButton;
		private System.Windows.Forms.Button clearBreakpointsButton;
		private System.Windows.Forms.Button stepButton;
		private System.Windows.Forms.Button runButton;
		private System.Windows.Forms.Label gotoLabel;
		private System.Windows.Forms.TextBox gotoTextBox;
		private System.Windows.Forms.Button gotoButton;
		private System.Windows.Forms.CheckBox imeCheckBox;
		private System.Windows.Forms.GroupBox breakpointsGroupBox;
		private System.Windows.Forms.Label ieValueLabel;
		private System.Windows.Forms.Label ifValueLabel;
	}
}