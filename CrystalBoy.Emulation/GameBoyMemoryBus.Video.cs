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
		private bool hdmaDone;

		#endregion

		#region Reset

		partial void ResetVideo()
		{
			lcdRealLine = 0;
			lyRegister = 0;
			frameCycles = rasterCycles = 60;
			videoNotifications = 0;
			lcdEnabled = true;
			hdmaActive = false;
			hdmaDone = false;

			// The boostrap ROM uses the video RAM before executing the game, so we need to emulate this behavior when not using the bootstrap ROM
			if (!useBootRom)
				unsafe
				{
					// The video RAM will first be cleared by the bootstrap ROM
					MemoryBlock.Set((void*)videoMemory, 0, colorHardware ? (uint)videoMemoryBlock.Length : (uint)videoMemoryBlock.Length / 2);
					// Then before leaving control to the actual game, the bootstrap ROM will leave the video RAM with the Nintendo logo ad the ® symbol.
					// (Actual time depends on the specific boostrap ROM, but we don't care here)
					WriteLogoTiles();
					// Only write the logo map if not running a color game
					if (!colorMode) WriteLogoMap();
				}
		}

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

		#region Various Methods

		private unsafe void WriteLogoTiles()
		{
			byte* destination = videoMemory + 16;
			byte sourceData;
			byte finalData = 0; // Initialize the variable once to stop the compiler from complaining. In fact we _really_ do not care about the initial value.
			int bitCount = NintendoLogo.Length << 3;

			// The three loops iterate on the same variable…
			// I'll name this the dododo pattern %)
			do
			{
				sourceData = this[(ushort)(0x104 + NintendoLogo.Length - (bitCount >> 3))];
				do
				{
					do
					{
						finalData <<= 2;
						if ((sourceData & 0x80) != 0) finalData |= 3;
						sourceData <<= 1;
					}
					while ((--bitCount & 0x3) != 0);
					*destination++ = finalData;
					*destination++ = 0; // Set this to 0 even though this shouldn't be needed
					*destination++ = finalData;
					*destination++ = 0; // Set this to 0 even though this shouldn't be needed
				}
				while ((bitCount & 0x4) != 0);
			}
			while (bitCount != 0);

			// Write data for the ® symbol
			*destination++ = 0x3C;
			*destination++ = 0x00;
			*destination++ = 0x42;
			*destination++ = 0x00;
			*destination++ = 0xB9;
			*destination++ = 0x00;
			*destination++ = 0xA5;
			*destination++ = 0x00;
			*destination++ = 0xB9;
			*destination++ = 0x00;
			*destination++ = 0xA5;
			*destination++ = 0x00;
			*destination++ = 0x42;
			*destination++ = 0x00;
			*destination++ = 0x3C;
			*destination++ = 0x00;
		}

		private unsafe void WriteLogoMap()
		{
			byte tileIndex = 0x19; // Tile 0 is (should be) blank, logo starts at 1

			videoMemory[0x1910] = tileIndex--;

			// Put the nintendo logo tiles in place
			byte* destination = videoMemory + 0x192f;

			do
			{
				for (int i = 0; i < 12; i++) *destination-- = tileIndex--;
				destination -= 0x14;
			}
			while (tileIndex != 0);
		}

		#endregion
	}
}
