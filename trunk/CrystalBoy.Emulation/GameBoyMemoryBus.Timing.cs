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
		#region Variables

		// Cycle counter
		// We count cycles on a frame basis
		// A frame always lasts 70224 cycles, excepted when the frame starts with lcd disabled, and lcd is enabled before frame end
		// In that case only, we reset the cycle counter before the frame ends, which makes a longer frame, but is needed for correct emulation...
		// When LCD is enabled, a frame begins at raster line 0, and ends at the last raster line (153, in VBlank)
		//int cycleCount; // Current clock cycle count

		// Double Speed Mode (Color Game Boy Only)
		bool doubleSpeed, prepareSpeedSwitch;
		bool frameDone;

		#endregion

		#region Properties

		public bool DoubleSpeed { get { return doubleSpeed; } }

		public bool PrepareSpeedSwitch { get { return prepareSpeedSwitch; } }

		#endregion

		#region Reset

		partial void ResetTiming()
		{
			doubleSpeed = false;
			prepareSpeedSwitch = false;
		}

		#endregion

		#region Timing

		public int LcdCycleCount { get { return rasterCycles; } }

		public bool AddCycles(int count) { return AddVariableCycles(count); }

		internal bool AddVariableCycles(int count)
		{
#if WITH_DEBUGGING && DEBUG_CYCLE_COUNTER
			debugCycleCount += count;
#endif
			AddFixedCycles(doubleSpeed ? count >> 1 : count);

			if (!frameDone) return true;
			else return frameDone = false;
		}

		internal unsafe void AddFixedCycles(int count)
		{
			if ((frameCycles += count) >= FrameDuration)
			{
				frameCycles -= FrameDuration;
				frameDone = true;
			}
			referenceTimerCycles += count; // Just ignore overflow for this…
			timerCycles += count; // Increment even if the timer is disabled, avoiding a conditional jump.
			AdjustTimer();

			if (!lcdEnabled) return;

			rasterCycles += count; // Increment the LCD cycle counter only if LCD is enabled (which should be most of the time)

			// From now on, we have either one (or more) new raster line(s) to handle, or the raster notifications to handle.
			// Notifications for new raster lines will be handler as a part of the loop, and slightly differently.
			if (rasterCycles < HorizontalLineDuration)
			{
				// Update LY *and* check for LY=LYC coincidence
				// Line 153 “ends” early
				if (lcdRealLine == 153)
				{
					// Only update if not done before…
					if (lyRegister != 0 && rasterCycles >= 8)
					{
						if ((lyRegister = 0) == portMemory[0x45] && notifyCoincidence && (videoNotifications & 0x01) == 0)
						{
							videoNotifications |= 0x01;
							InterruptRequest(0x02);
						}
						else videoNotifications &= 0x0E;
					}
				}
				// Update LY 4 cycles before next line
				else if (rasterCycles >= HorizontalLineDuration - 4)
				{
					if ((lyRegister = lcdRealLine + 1) == portMemory[0x45] && notifyCoincidence && (videoNotifications & 0x01) == 0)
					{
						videoNotifications |= 0x01;
						InterruptRequest(0x02);
					}
					else videoNotifications &= 0x0E;
				}

				// Mode 2, 3 and 0 can only happen outside of VBLANK
				if (lcdRealLine < 144)
				{
					// Check for OAM Fetch
					if (notifyMode2 && rasterCycles < Mode2Duration && (videoNotifications & 0x02) == 0)
					{
						videoNotifications |= 0x02;
						InterruptRequest(0x02);
					}
					// Check for HBLANK
					if (rasterCycles >= Mode2Duration + Mode3Duration)
					{
						if (notifyMode0 && (videoNotifications & 0x04) == 0)
						{
							videoNotifications |= 0x04;
							InterruptRequest(0x02);
						}
						// In this case, always add the cycles taken for HDMA (they are not included in the count parameter)
						if (hdmaActive && !hdmaDone) HandleHdma(true);
					}
				}
			}
			else do // This may probably be sped up a little bit for big updates (> 20 cycles) but those should not happen very often
			{
				rasterCycles -= HorizontalLineDuration;
				hdmaDone = false;

				if (lcdRealLine == 153)
				{
					lcdRealLine = 0;
					// Resume LCD drawing (after VBlank)
					videoNotifications &= 0x01; // Keep the coincidence bit
					// Raise the FrameReady event
					OnFrameReady();
					// Prepare for the new frame…
					// Clear the video access lists
					videoPortAccessList.Clear();
					paletteAccessList.Clear();
					// Create a new snapshot of the video ports
					videoStatusSnapshot.Capture();
				}
				else
				{
					lcdRealLine++;
					videoNotifications &= 0x09; // Keep the coincidence and vblank bits
				}

				// Compute the new LY value
				int lyNewValue = lcdRealLine < 153 ?
					rasterCycles >= HorizontalLineDuration - 4 ?
						lcdRealLine + 1 :
						lcdRealLine :
					rasterCycles < 8 ?
						153 :
						0;

				// TODO handle LY "jumping" from 152 to 0

				// Check for LY=LYC coincidence
				if (notifyCoincidence && lyNewValue == portMemory[0x45])
				{
					if ((videoNotifications & 0x01) == 0 || lyNewValue != lyRegister)
					{
						videoNotifications |= 0x01;
						InterruptRequest(0x02);
					}
				}
				else videoNotifications &= 0x0E;
				lyRegister = lyNewValue;

				// Check for VBLANK Interrupt
				if (lcdRealLine >= 144)
				{
					if ((videoNotifications & 0x08) == 0)
					{
						videoNotifications |= 0x08;
						InterruptRequest(notifyMode1 ? (byte)0x03 : (byte)0x01);
					}
				}
				// Check for Mode 2 or Mode 0
				else
				{
					if (notifyMode2)
					{
						videoNotifications |= 0x02;
						InterruptRequest(0x02);
					}
					if (rasterCycles >= Mode2Duration + Mode3Duration)
					{
						if (notifyMode0)
						{
							videoNotifications |= 0x04;
							InterruptRequest(0x02);
						}
						// Handle HDMA, but add cycles only if the processing here is done…
						// (If we still have another line waiting, the cycles are already included in the computation…)
						if (hdmaActive && !hdmaDone) HandleHdma(rasterCycles < HorizontalLineDuration);
					}
				}
			} while (rasterCycles >= HorizontalLineDuration);
		}

		internal int HandleProcessorStop()
		{
			if (colorMode)
			{
				doubleSpeed = !doubleSpeed;
				return 0;
			}
			else return WaitForInterrupts();
		}

		#endregion
	}
}
