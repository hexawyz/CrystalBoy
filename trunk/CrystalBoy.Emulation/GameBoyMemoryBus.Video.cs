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

		// LCD Status Interrupt
		int lycMinCycle,
			statInterruptCycle;
		bool statInterruptEnabled, // LCD Status interrupt can happen
			notifyCoincidence,
			notifyMode2,
			notifyMode1,
			notifyMode0;
		int lyOffset; // LY counter offset
		bool lcdEnabled, lcdDrawing;

		#endregion

		#region Events

		public event EventHandler NewFrame;

		#endregion

		#region LCD Controller Status

		public bool VideoEnabled { get { return lcdEnabled; } }

		private void DisableVideo()
		{
			// Disabling the LCD is fairly easy.
			// Enabling it should be a bit more tricky...
			lcdEnabled = false;
			lcdDrawing = false;
			statInterruptEnabled = false;
		}

		private void EnableVideo()
		{
			// Do almost the same thing as AdjustTimings, excepted that the timings are adjusted basing on the current ellapsed cycles
			lcdEnabled = true;
			lcdDrawing = true;
			// Reset LY
			lyOffset = 0;
			// Update the reference timer shift
			referenceTimerShift += cycleCount % ReferenceTimerTickCycles;
			if (referenceTimerShift > ReferenceTimerTickCycles)
				referenceTimerShift -= ReferenceTimerTickCycles;
			// Update the divider base cycle
			dividerBaseCycle = (dividerBaseCycle - cycleCount) % 65536;
			// Update the timer timings if needed
			if (timerEnabled)
			{
				timerBaseCycle -= cycleCount + VerticalBlankDuration;
				timerInterruptCycle -= cycleCount + VerticalBlankDuration;
			}
			// Update HDMA if needed
			if (hdmaActive)
				hdmaNextCycle = Mode2Duration + Mode3Duration; // Reset to line 0 HBlank
			// Set the counter just before the first horizontal line
			cycleCount = -4; // First line is cycle 0, therefore, cycle -4 is the cycle before ;)
			// Update the video status interrupt for reflecting changes
			UpdateVideoStatusInterrupt();
			// Now set the timer to the first horizontal line (this is a bit of a hack but it should work perfectly :p)
			cycleCount = 0;
			// Clear the video access lists
			videoPortAccessList.Clear();
			paletteAccessList.Clear();
			// Create a new snapshot of the video ports
			videoStatusSnapshot.Capture();
		}

		public void UpdateVideoStatusInterrupt()
		{
			int coincidence, mode0, mode1, mode2;

			if (!(lcdEnabled && (notifyCoincidence || notifyMode0 || notifyMode1 || notifyMode2)))
			{
				statInterruptEnabled = false;
				return;
			}

			coincidence = notifyCoincidence ? cycleCount < lycMinCycle ? lycMinCycle : FrameDuration + lycMinCycle : int.MaxValue;

			if (notifyMode0 || notifyMode2)
			{
				if (cycleCount >= FrameDuration - VerticalBlankDuration) // Case where we are in VBlank period
				{
					if (notifyMode2) // Mode 2 is OAM fetch
					{
						mode2 = FrameDuration; // A frame begins at 0, but we calculate for next frame here ;)
						mode0 = int.MaxValue;
					}
					else // Mode 0 is Horizontal Blank
					{
						mode0 = FrameDuration + Mode2Duration + Mode3Duration; // Same here ;)
						mode2 = int.MaxValue;
					}
				}
				else // Case when we are not in VBlank period
				{
					int timing = cycleCount % HorizontalLineDuration; // Calculate the timing inside our raster line

					if (notifyMode0 && timing < Mode2Duration + Mode3Duration) // If we requested HBlank notification and we are before HBlank...
					{
						mode0 = cycleCount - timing + Mode2Duration + Mode3Duration;
						mode2 = int.MaxValue;
					}
					else if (notifyMode2)
					{
						mode2 = cycleCount - timing + HorizontalLineDuration; // If we requested OAM fetch notification... (since mode 2 begins a raster line, we are always after mode 2 beginning ;))
						mode0 = int.MaxValue;
					}
					else // And finally if we requested HBlank notification but we are inside HBlank
					{
						mode0 = cycleCount - timing + HorizontalLineDuration + Mode2Duration + Mode3Duration;
						mode2 = int.MaxValue;
					}
				}
			}
			else
			{
				mode0 = int.MaxValue;
				mode2 = int.MaxValue;
			}

			// Mode 1 is Vertical Blank (same as VBI)
			mode1 = notifyMode1 ? (cycleCount < FrameDuration - VerticalBlankDuration ? 0 : FrameDuration) + FrameDuration - VerticalBlankDuration : int.MaxValue;

			// The stat interrupt cycle is the minimum of all potential interrupt cycles
			statInterruptCycle = Math.Min(Math.Min(coincidence, mode0), Math.Min(mode1, mode2));

			// If the cycle is not valid (which may happen), we disable the interrupt
			statInterruptEnabled = statInterruptCycle != int.MaxValue;
		}

		//private void UpdatedCoincidence()
		//{
		//    // Assuming notifyCoincidence = true

		//    if (!statInterruptEnabled)
		//    {
		//        statInterruptEnabled = true;
		//        if (cycleCount < lycMinCycle)
		//            statInterruptCycle = lycMinCycle;
		//        else
		//            statInterruptCycle = FrameDuration + lycMinCycle;
		//    }
		//    else if (cycleCount < lycMinCycle && lycMinCycle < statInterruptCycle)
		//        statInterruptCycle = lycMinCycle;
		//}

		#endregion
	}
}
