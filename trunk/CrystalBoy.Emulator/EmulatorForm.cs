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
using System.ComponentModel;
using System.Windows.Forms;
using CrystalBoy.Core;
using CrystalBoy.Emulation;

namespace CrystalBoy.Emulator
{
	class EmulatorForm : Form
	{
		EmulatedGameBoy emulatedGameBoy;

		private EmulatorForm()
		{
		}

		public EmulatorForm(EmulatedGameBoy emulatedGameBoy)
		{
			if (emulatedGameBoy == null)
				throw new ArgumentNullException("emulatedGameBoy");
			this.emulatedGameBoy = emulatedGameBoy;
			this.emulatedGameBoy.AfterReset += OnAfterResetInternal;
			this.emulatedGameBoy.RomChanged += OnRomChangedInternal;
			this.emulatedGameBoy.Paused += OnPausedInternal;
			this.emulatedGameBoy.Break += OnBreakInternal;
			this.emulatedGameBoy.EmulationStatusChanged += OnEmulationStatusChangedInternal;
			this.emulatedGameBoy.NewFrame += OnNewFrameInternal;
		}

		protected EmulatedGameBoy EmulatedGameBoy { get { return emulatedGameBoy; } }

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				Hide();
				e.Cancel = true;
			}
			base.OnFormClosing(e);
		}

		private void OnAfterResetInternal(object sender, EventArgs e)
		{
			OnReset(e);
		}

		private void OnRomChangedInternal(object sender, EventArgs e)
		{
			OnRomChanged(e);
		}

		private void OnPausedInternal(object sender, EventArgs e)
		{
			OnPaused(e);
		}

		private void OnBreakInternal(object sender, EventArgs e)
		{
			OnBreak(e);
		}

		private void OnEmulationStatusChangedInternal(object sender, EventArgs e)
		{
			OnEmulationStatusChanged(e);
		}

		private void OnNewFrameInternal(object sender, EventArgs e)
		{
			OnNewFrame(e);
		}

		protected virtual void OnReset(EventArgs e)
		{
		}

		protected virtual void OnRomChanged(EventArgs e)
		{
		}

		protected virtual void OnPaused(EventArgs e)
		{
		}

		protected virtual void OnBreak(EventArgs e)
		{
		}

		protected virtual void OnNewFrame(EventArgs e)
		{
		}

		protected virtual void OnEmulationStatusChanged(EventArgs e)
		{
		}
	}
}
