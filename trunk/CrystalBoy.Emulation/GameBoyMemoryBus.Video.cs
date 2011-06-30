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
using System.Collections.Generic;
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	partial class GameBoyMemoryBus
	{
		#region Constants

		public const int FrameDuration = 70224;
		public const int VerticalBlankDuration = 4560;
		public const int HorizontalBlankDuration = 204;
		public const int Mode0Duration = HorizontalBlankDuration;
		public const int Mode1Duration = VerticalBlankDuration;
		public const int Mode2Duration = 80;
		public const int Mode3Duration = 172;
		public const int HorizontalLineDuration = Mode2Duration + Mode3Duration + Mode0Duration;

		#endregion

		#region Variables

		// LCD “live” status
		private int frameCycles;
		private int rasterCycles; // Current clock cycle count, relative to the current LCD line
		private int lcdRealLine; // LY should be equal to this *most of the time*.
		private int lyRegister; // Maintain LY status in parallel 
		private bool lcdEnabled; // Self-explanatory…
		// LCD Status Interrupt
		private byte videoNotifications; // Bitmask indicating the requested video interrupts (1 = LY=LYC, 2 = OAM Fetch, 4 = HBLANK, 8 = VBLANK)
		private bool notifyCoincidence;
		private bool notifyMode2;
		private bool notifyMode1;
		private bool notifyMode0;

		#endregion

		#region LCD Controller Status

		public bool VideoEnabled { get { return lcdEnabled; } }

		private void DisableVideo()
		{
			// Disabling the LCD is fairly easy.
			// Enabling it should be a bit more tricky...
			lcdEnabled = false;
			videoNotifications = 0;
			lcdRealLine = 0;
			lyRegister = 0;
			rasterCycles = 0;
		}

		private void EnableVideo()
		{
			// Restart the LCD at the first line
			lcdEnabled = true;
			frameCycles = 0;
			rasterCycles = 0;
			lcdRealLine = 0;
			lyRegister = 0;
			videoNotifications = 0;
			// Clear the video access lists
			videoPortAccessList.Clear();
			paletteAccessList.Clear();
			// Create a new snapshot of the video ports
			videoStatusSnapshot.Capture();
		}

		#endregion
	}
}
