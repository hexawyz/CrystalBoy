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

using System;
using System.Collections.Generic;
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	partial class GameBoyMemoryBus
	{
		#region Variables

		// Cycle counter
		// We count cycles on a frame basis
		// A frame always lasts 70224 cycles, excepted when the frame starts with lcd disabled, and lcd is enabled before frame end
		// In that case only, we reset the cycle counter before the frame ends, which makes a longer frame, but is needed for correct emulation...
		// When LCD is enabled, a frame begins at raster line 0, and ends at the last raster line (153, in VBlank)
		int cycleCount; // Current clock cycle count

		// Double Speed Mode (Color Game Boy Only)
		bool doubleSpeed, prepareSpeedSwitch;

		#endregion

		#region Properties

		public bool DoubleSpeed { get { return doubleSpeed; } }

		public bool PrepareSpeedSwitch { get { return prepareSpeedSwitch; } }

		#endregion

		#region Reset

		partial void ResetTiming()
		{
			cycleCount = 0;
			doubleSpeed = false;
			prepareSpeedSwitch = false;
		}

		#endregion

		#region Timing

		public int CycleCount { get { return cycleCount; } }

		public bool AddCycles(int count)
		{
			cycleCount += doubleSpeed ? count >> 1 : count;

			if (hdmaActive && lcdEnabled && cycleCount >= hdmaNextCycle)
				HandleHdma(count < 20);

			if (cycleCount > FrameDuration)
			{
				AdjustTimings();
				return false;
			}
			return true;
		}

		private void AdjustTimings()
		{
			// Reset LY
			lyOffset = 0;
			// Resume LCD drawing (after VBlank)
			lcdDrawing = lcdEnabled;
			// Update the reference timer shift
			referenceTimerShift += ReferenceTimerFrameShift;
			if (referenceTimerShift > ReferenceTimerTickCycles)
				referenceTimerShift -= ReferenceTimerTickCycles;
			// Update the divider base cycle
			dividerBaseCycle = (dividerBaseCycle - FrameDuration) % 65536;
			// Update the timer timings if needed
			if (timerEnabled)
			{
				timerBaseCycle -= FrameDuration;
				timerInterruptCycle -= FrameDuration;
			}
			// Update LCD status timings if needed
			if (statInterruptEnabled)
				statInterruptCycle -= FrameDuration;
			// Update HDMA if needed
			// TODO: Add HDMA support in HALT handler !
			if (hdmaActive)
				hdmaNextCycle = Mode2Duration + Mode3Duration; // Reset to line 0 HBlank
			// Remove a frame from the cycle count
			cycleCount -= FrameDuration;
			// Raise the FrameReady event
			if (NewFrame != null)
			    NewFrame(this, EventArgs.Empty);
			Render();
			// Prepare for the new frame...
			// Clear the video access lists
			videoPortAccessList.Clear();
			paletteAccessList.Clear();
			// Create a new snapshot of the video ports
			videoStatusSnapshot.Capture();
		}

		internal int HandleProcessorStop()
		{
			if (colorMode)
			{
				doubleSpeed = !doubleSpeed;
				return 0;
			}
			else
				return WaitForInterrupts();
		}

		#endregion
	}
}
