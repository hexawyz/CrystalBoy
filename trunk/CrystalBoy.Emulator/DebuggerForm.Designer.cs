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
			registersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
			registersGroupBox.Location = new System.Drawing.Point(316, 13);
			registersGroupBox.Name = "registersGroupBox";
			registersGroupBox.Size = new System.Drawing.Size(164, 75);
			registersGroupBox.TabIndex = 4;
			registersGroupBox.TabStop = false;
			registersGroupBox.Text = "Registers";
			// 
			// ieValueLabel
			// 
			this.ieValueLabel.AutoSize = true;
			this.ieValueLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ieValueLabel.Location = new System.Drawing.Point(123, 58);
			this.ieValueLabel.Name = "ieValueLabel";
			this.ieValueLabel.Size = new System.Drawing.Size(0, 14);
			this.ieValueLabel.TabIndex = 15;
			// 
			// ieLabel
			// 
			ieLabel.AutoSize = true;
			ieLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			ieLabel.Location = new System.Drawing.Point(89, 58);
			ieLabel.Name = "ieLabel";
			ieLabel.Size = new System.Drawing.Size(28, 14);
			ieLabel.TabIndex = 14;
			ieLabel.Text = "IE:";
			// 
			// ifValueLabel
			// 
			this.ifValueLabel.AutoSize = true;
			this.ifValueLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ifValueLabel.Location = new System.Drawing.Point(40, 58);
			this.ifValueLabel.Name = "ifValueLabel";
			this.ifValueLabel.Size = new System.Drawing.Size(0, 14);
			this.ifValueLabel.TabIndex = 13;
			// 
			// ifLabel
			// 
			ifLabel.AutoSize = true;
			ifLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			ifLabel.Location = new System.Drawing.Point(6, 58);
			ifLabel.Name = "ifLabel";
			ifLabel.Size = new System.Drawing.Size(28, 14);
			ifLabel.TabIndex = 12;
			ifLabel.Text = "IF:";
			// 
			// pcValueLabel
			// 
			this.pcValueLabel.AutoSize = true;
			this.pcValueLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pcValueLabel.Location = new System.Drawing.Point(123, 44);
			this.pcValueLabel.Name = "pcValueLabel";
			this.pcValueLabel.Size = new System.Drawing.Size(0, 14);
			this.pcValueLabel.TabIndex = 11;
			// 
			// spValueLabel
			// 
			this.spValueLabel.AutoSize = true;
			this.spValueLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.spValueLabel.Location = new System.Drawing.Point(123, 30);
			this.spValueLabel.Name = "spValueLabel";
			this.spValueLabel.Size = new System.Drawing.Size(0, 14);
			this.spValueLabel.TabIndex = 9;
			// 
			// hlValueLabel
			// 
			this.hlValueLabel.AutoSize = true;
			this.hlValueLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.hlValueLabel.Location = new System.Drawing.Point(123, 16);
			this.hlValueLabel.Name = "hlValueLabel";
			this.hlValueLabel.Size = new System.Drawing.Size(0, 14);
			this.hlValueLabel.TabIndex = 7;
			// 
			// deValueLabel
			// 
			this.deValueLabel.AutoSize = true;
			this.deValueLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.deValueLabel.Location = new System.Drawing.Point(40, 44);
			this.deValueLabel.Name = "deValueLabel";
			this.deValueLabel.Size = new System.Drawing.Size(0, 14);
			this.deValueLabel.TabIndex = 5;
			// 
			// bcValueLabel
			// 
			this.bcValueLabel.AutoSize = true;
			this.bcValueLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.bcValueLabel.Location = new System.Drawing.Point(40, 30);
			this.bcValueLabel.Name = "bcValueLabel";
			this.bcValueLabel.Size = new System.Drawing.Size(0, 14);
			this.bcValueLabel.TabIndex = 3;
			// 
			// afValueLabel
			// 
			this.afValueLabel.AutoSize = true;
			this.afValueLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.afValueLabel.Location = new System.Drawing.Point(40, 16);
			this.afValueLabel.Name = "afValueLabel";
			this.afValueLabel.Size = new System.Drawing.Size(0, 14);
			this.afValueLabel.TabIndex = 1;
			// 
			// pcLabel
			// 
			pcLabel.AutoSize = true;
			pcLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			pcLabel.Location = new System.Drawing.Point(89, 44);
			pcLabel.Name = "pcLabel";
			pcLabel.Size = new System.Drawing.Size(28, 14);
			pcLabel.TabIndex = 10;
			pcLabel.Text = "PC:";
			// 
			// spLabel
			// 
			spLabel.AutoSize = true;
			spLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			spLabel.Location = new System.Drawing.Point(89, 30);
			spLabel.Name = "spLabel";
			spLabel.Size = new System.Drawing.Size(28, 14);
			spLabel.TabIndex = 8;
			spLabel.Text = "SP:";
			// 
			// hlLabel
			// 
			hlLabel.AutoSize = true;
			hlLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			hlLabel.Location = new System.Drawing.Point(89, 16);
			hlLabel.Name = "hlLabel";
			hlLabel.Size = new System.Drawing.Size(28, 14);
			hlLabel.TabIndex = 6;
			hlLabel.Text = "HL:";
			// 
			// deLabel
			// 
			deLabel.AutoSize = true;
			deLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			deLabel.Location = new System.Drawing.Point(6, 44);
			deLabel.Name = "deLabel";
			deLabel.Size = new System.Drawing.Size(28, 14);
			deLabel.TabIndex = 4;
			deLabel.Text = "DE:";
			// 
			// bcLabel
			// 
			bcLabel.AutoSize = true;
			bcLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			bcLabel.Location = new System.Drawing.Point(6, 30);
			bcLabel.Name = "bcLabel";
			bcLabel.Size = new System.Drawing.Size(28, 14);
			bcLabel.TabIndex = 2;
			bcLabel.Text = "BC:";
			// 
			// afLabel
			// 
			afLabel.AutoSize = true;
			afLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			afLabel.Location = new System.Drawing.Point(6, 16);
			afLabel.Name = "afLabel";
			afLabel.Size = new System.Drawing.Size(28, 14);
			afLabel.TabIndex = 0;
			afLabel.Text = "AF:";
			// 
			// flagsGroupBox
			// 
			flagsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			flagsGroupBox.Controls.Add(this.imeCheckBox);
			flagsGroupBox.Controls.Add(this.carryFlagCheckBox);
			flagsGroupBox.Controls.Add(this.halfCarryFlagCheckBox);
			flagsGroupBox.Controls.Add(this.negationFlagCheckBox);
			flagsGroupBox.Controls.Add(this.zeroFlagCheckBox);
			flagsGroupBox.Location = new System.Drawing.Point(316, 94);
			flagsGroupBox.Name = "flagsGroupBox";
			flagsGroupBox.Size = new System.Drawing.Size(164, 65);
			flagsGroupBox.TabIndex = 5;
			flagsGroupBox.TabStop = false;
			flagsGroupBox.Text = "Flags";
			// 
			// imeCheckBox
			// 
			this.imeCheckBox.AutoCheck = false;
			this.imeCheckBox.AutoSize = true;
			this.imeCheckBox.Location = new System.Drawing.Point(6, 42);
			this.imeCheckBox.Name = "imeCheckBox";
			this.imeCheckBox.Size = new System.Drawing.Size(136, 17);
			this.imeCheckBox.TabIndex = 4;
			this.imeCheckBox.Text = "Interrupt Master Enable";
			this.imeCheckBox.UseVisualStyleBackColor = true;
			// 
			// carryFlagCheckBox
			// 
			this.carryFlagCheckBox.AutoCheck = false;
			this.carryFlagCheckBox.AutoSize = true;
			this.carryFlagCheckBox.Location = new System.Drawing.Point(125, 19);
			this.carryFlagCheckBox.Name = "carryFlagCheckBox";
			this.carryFlagCheckBox.Size = new System.Drawing.Size(33, 17);
			this.carryFlagCheckBox.TabIndex = 3;
			this.carryFlagCheckBox.Text = "C";
			this.carryFlagCheckBox.UseVisualStyleBackColor = true;
			// 
			// halfCarryFlagCheckBox
			// 
			this.halfCarryFlagCheckBox.AutoCheck = false;
			this.halfCarryFlagCheckBox.AutoSize = true;
			this.halfCarryFlagCheckBox.Location = new System.Drawing.Point(85, 19);
			this.halfCarryFlagCheckBox.Name = "halfCarryFlagCheckBox";
			this.halfCarryFlagCheckBox.Size = new System.Drawing.Size(34, 17);
			this.halfCarryFlagCheckBox.TabIndex = 2;
			this.halfCarryFlagCheckBox.Text = "H";
			this.halfCarryFlagCheckBox.UseVisualStyleBackColor = true;
			// 
			// negationFlagCheckBox
			// 
			this.negationFlagCheckBox.AutoCheck = false;
			this.negationFlagCheckBox.AutoSize = true;
			this.negationFlagCheckBox.Location = new System.Drawing.Point(45, 19);
			this.negationFlagCheckBox.Name = "negationFlagCheckBox";
			this.negationFlagCheckBox.Size = new System.Drawing.Size(34, 17);
			this.negationFlagCheckBox.TabIndex = 1;
			this.negationFlagCheckBox.Text = "N";
			this.negationFlagCheckBox.UseVisualStyleBackColor = true;
			// 
			// zeroFlagCheckBox
			// 
			this.zeroFlagCheckBox.AutoCheck = false;
			this.zeroFlagCheckBox.AutoSize = true;
			this.zeroFlagCheckBox.Location = new System.Drawing.Point(6, 19);
			this.zeroFlagCheckBox.Name = "zeroFlagCheckBox";
			this.zeroFlagCheckBox.Size = new System.Drawing.Size(33, 17);
			this.zeroFlagCheckBox.TabIndex = 0;
			this.zeroFlagCheckBox.Text = "Z";
			this.zeroFlagCheckBox.UseVisualStyleBackColor = true;
			// 
			// debugGroupBox
			// 
			debugGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			debugGroupBox.Controls.Add(this.stepButton);
			debugGroupBox.Controls.Add(this.runButton);
			debugGroupBox.Location = new System.Drawing.Point(316, 248);
			debugGroupBox.Name = "debugGroupBox";
			debugGroupBox.Size = new System.Drawing.Size(164, 48);
			debugGroupBox.TabIndex = 7;
			debugGroupBox.TabStop = false;
			debugGroupBox.Text = "Debug";
			// 
			// stepButton
			// 
			this.stepButton.Location = new System.Drawing.Point(83, 19);
			this.stepButton.Name = "stepButton";
			this.stepButton.Size = new System.Drawing.Size(75, 23);
			this.stepButton.TabIndex = 1;
			this.stepButton.Text = "Step";
			this.stepButton.UseVisualStyleBackColor = true;
			this.stepButton.Click += new System.EventHandler(this.stepButton_Click);
			// 
			// runButton
			// 
			this.runButton.Location = new System.Drawing.Point(6, 19);
			this.runButton.Name = "runButton";
			this.runButton.Size = new System.Drawing.Size(75, 23);
			this.runButton.TabIndex = 0;
			this.runButton.Text = "Run";
			this.runButton.UseVisualStyleBackColor = true;
			this.runButton.Click += new System.EventHandler(this.runButton_Click);
			// 
			// breakpointsGroupBox
			// 
			this.breakpointsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.breakpointsGroupBox.Controls.Add(this.toggleBreakpointButton);
			this.breakpointsGroupBox.Controls.Add(this.clearBreakpointsButton);
			this.breakpointsGroupBox.Location = new System.Drawing.Point(316, 165);
			this.breakpointsGroupBox.Name = "breakpointsGroupBox";
			this.breakpointsGroupBox.Size = new System.Drawing.Size(164, 77);
			this.breakpointsGroupBox.TabIndex = 6;
			this.breakpointsGroupBox.TabStop = false;
			this.breakpointsGroupBox.Text = "Breakpoints";
			// 
			// toggleBreakpointButton
			// 
			this.toggleBreakpointButton.Location = new System.Drawing.Point(6, 19);
			this.toggleBreakpointButton.Name = "toggleBreakpointButton";
			this.toggleBreakpointButton.Size = new System.Drawing.Size(152, 23);
			this.toggleBreakpointButton.TabIndex = 0;
			this.toggleBreakpointButton.Text = "Toggle";
			this.toggleBreakpointButton.UseVisualStyleBackColor = true;
			this.toggleBreakpointButton.Click += new System.EventHandler(this.toggleBreakpointButton_Click);
			// 
			// clearBreakpointsButton
			// 
			this.clearBreakpointsButton.Location = new System.Drawing.Point(6, 48);
			this.clearBreakpointsButton.Name = "clearBreakpointsButton";
			this.clearBreakpointsButton.Size = new System.Drawing.Size(152, 23);
			this.clearBreakpointsButton.TabIndex = 1;
			this.clearBreakpointsButton.Text = "Clear All";
			this.clearBreakpointsButton.UseVisualStyleBackColor = true;
			this.clearBreakpointsButton.Click += new System.EventHandler(this.clearBreakpointsButton_Click);
			// 
			// gotoLabel
			// 
			this.gotoLabel.AutoSize = true;
			this.gotoLabel.Location = new System.Drawing.Point(12, 15);
			this.gotoLabel.Name = "gotoLabel";
			this.gotoLabel.Size = new System.Drawing.Size(51, 13);
			this.gotoLabel.TabIndex = 0;
			this.gotoLabel.Text = "Address: ";
			// 
			// gotoTextBox
			// 
			this.gotoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.gotoTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.gotoTextBox.Location = new System.Drawing.Point(164, 12);
			this.gotoTextBox.MaxLength = 4;
			this.gotoTextBox.Name = "gotoTextBox";
			this.gotoTextBox.Size = new System.Drawing.Size(100, 20);
			this.gotoTextBox.TabIndex = 1;
			this.gotoTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.gotoTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gotoTextBox_KeyDown);
			// 
			// gotoButton
			// 
			this.gotoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.gotoButton.Location = new System.Drawing.Point(270, 10);
			this.gotoButton.Name = "gotoButton";
			this.gotoButton.Size = new System.Drawing.Size(39, 23);
			this.gotoButton.TabIndex = 2;
			this.gotoButton.Text = "Go";
			this.gotoButton.UseVisualStyleBackColor = true;
			this.gotoButton.Click += new System.EventHandler(this.gotoButton_Click);
			// 
			// disassemblyView
			// 
			this.disassemblyView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.disassemblyView.AutoScrollMinSize = new System.Drawing.Size(0, 1179648);
			this.disassemblyView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.disassemblyView.Location = new System.Drawing.Point(14, 39);
			this.disassemblyView.Margin = new System.Windows.Forms.Padding(4);
			this.disassemblyView.Memory = null;
			this.disassemblyView.Name = "disassemblyView";
			this.disassemblyView.ShowCurrentInstruction = true;
			this.disassemblyView.Size = new System.Drawing.Size(295, 256);
			this.disassemblyView.TabIndex = 3;
			this.disassemblyView.DoubleClick += new System.EventHandler(this.disassemblyView_DoubleClick);
			// 
			// DebuggerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(492, 308);
			this.Controls.Add(this.gotoButton);
			this.Controls.Add(this.gotoTextBox);
			this.Controls.Add(this.gotoLabel);
			this.Controls.Add(debugGroupBox);
			this.Controls.Add(this.breakpointsGroupBox);
			this.Controls.Add(flagsGroupBox);
			this.Controls.Add(registersGroupBox);
			this.Controls.Add(this.disassemblyView);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(500, 342);
			this.Name = "DebuggerForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Debugger";
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