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

using System;
using System.Globalization;
using System.Media;
using System.Windows.Forms;
using CrystalBoy.Emulation;

namespace CrystalBoy.Emulator
{
	partial class DebuggerForm : EmulatorForm
	{
		bool initialized;

		public DebuggerForm(EmulatedGameBoy EmulatedGameBoy)
			: base(EmulatedGameBoy)
		{
			InitializeComponent();
			OnRomChanged(EventArgs.Empty);
#if !WITH_DEBUGGING
			cycleCounterGroupBox.Visible = false;
			breakpointsGroupBox.Visible = false;
#endif
		}

		private void UpdateInformation()
		{
			Processor processor = EmulatedGameBoy.Processor;
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;

			if (!(initialized && (EmulatedGameBoy.RomLoaded || EmulatedGameBoy.Bus.UseBootRom)))
			{
			    disassemblyView.SelectedOffset = 0;
			    disassemblyView.ScrollSelectionIntoView();
			    afValueLabel.Text = "0000";
			    bcValueLabel.Text = "0000";
			    deValueLabel.Text = "0000";
			    hlValueLabel.Text = "0000";
			    spValueLabel.Text = "0000";
			    pcValueLabel.Text = "0000";
			    ieValueLabel.Text = "00";
			    ifValueLabel.Text = "00";
			    zeroFlagCheckBox.Checked = false;
			    negationFlagCheckBox.Checked = false;
			    halfCarryFlagCheckBox.Checked = false;
			    carryFlagCheckBox.Checked = false;
			    imeCheckBox.Checked = false;
				cycleCounterValueLabel.Text = "0";
			    initialized = true;
			}
			else if (EmulatedGameBoy.EmulationStatus == EmulationStatus.Paused)
			{
				disassemblyView.CurrentInstructionOffset = processor.PC;
				disassemblyView.SelectedOffset = processor.PC;
				disassemblyView.ScrollSelectionIntoView();
				afValueLabel.Text = processor.AF.ToString("X4", invariantCulture);
				bcValueLabel.Text = processor.BC.ToString("X4", invariantCulture);
				deValueLabel.Text = processor.DE.ToString("X4", invariantCulture);
				hlValueLabel.Text = processor.HL.ToString("X4", invariantCulture);
				spValueLabel.Text = processor.SP.ToString("X4", invariantCulture);
				pcValueLabel.Text = processor.PC.ToString("X4", invariantCulture);
				ieValueLabel.Text = processor.Bus.EnabledInterrupts.ToString("X2", invariantCulture);
				ifValueLabel.Text = processor.Bus.RequestedInterrupts.ToString("X2", invariantCulture);
				zeroFlagCheckBox.Checked = processor.ZeroFlag;
				negationFlagCheckBox.Checked = processor.NegationFlag;
				halfCarryFlagCheckBox.Checked = processor.HalfCarryFlag;
				carryFlagCheckBox.Checked = processor.CarryFlag;
				imeCheckBox.Checked = processor.InterruptMasterEnable;
#if WITH_DEBUGGING
				cycleCounterValueLabel.Text = (EmulatedGameBoy.Bus.DebugCycleCount % 1000).ToString();
#endif
			}
		}

		protected override void OnShown(EventArgs e)
		{
			UpdateInformation();
			base.OnShown(e);
		}

		protected override void OnRomChanged(EventArgs e)
		{
			disassemblyView.Memory = EmulatedGameBoy.RomLoaded || EmulatedGameBoy.Bus.UseBootRom ? EmulatedGameBoy.Bus : null;
			UpdateInformation();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if (Visible) UpdateInformation();
			base.OnVisibleChanged(e);
		}

		protected override void OnPaused(EventArgs e)
		{
			if (Visible) UpdateInformation();
		}

		protected override void OnBreak(EventArgs e)
		{
			if (Visible) UpdateInformation();
			else Show(Owner);
		}

		private void stepButton_Click(object sender, EventArgs e)
		{
			EmulatedGameBoy.Step();
		}

		private void runButton_Click(object sender, EventArgs e)
		{
			EmulatedGameBoy.Run();
		}

		private void toggleBreakpointButton_Click(object sender, EventArgs e)
		{
#if WITH_DEBUGGING
			EmulatedGameBoy.Bus.ToggleBreakpoint(disassemblyView.SelectedOffset);
			disassemblyView.Invalidate();
#endif
		}

		private void clearBreakpointsButton_Click(object sender, EventArgs e)
		{
#if WITH_DEBUGGING
			EmulatedGameBoy.Bus.ClearBreakpoints();
			disassemblyView.Invalidate();
#endif
		}

		private void disassemblyView_DoubleClick(object sender, EventArgs e)
		{
#if WITH_DEBUGGING
			EmulatedGameBoy.Bus.ToggleBreakpoint(disassemblyView.SelectedOffset);
			disassemblyView.Invalidate();
#endif
		}

		private bool TryGoto()
		{
			try
			{
				disassemblyView.SelectedOffset = ushort.Parse(gotoTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
				disassemblyView.ScrollSelectionIntoView();
				return true;
			}
			catch (FormatException) { return false; }
		}

		private void gotoTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter && TryGoto())
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private void gotoButton_Click(object sender, EventArgs e)
		{
			if (!TryGoto())
			{
				SystemSounds.Beep.Play();
				gotoTextBox.Focus();
			}
		}

		private void cycleCounterResetButton_Click(object sender, EventArgs e)
		{
#if WITH_DEBUGGING
			EmulatedGameBoy.Bus.ResetDebugCycleCounter();
			cycleCounterValueLabel.Text = "0";
#endif
		}
	}
}
