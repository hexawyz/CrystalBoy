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

		// Variables used for the GB Programmable Timer
		// Global cycle counter, used to synchronize the various timer sources
		int referenceTimerCycles; // Ellapsed clock cycles for the reference timer
		int dividerCycleOffset; // Offset between the reference cycle counter and the divider's first cycle.
		int timerCycles; // Ellapsed clock cycles for the timer
		int timerOverflowCycles; // Number of clock cycles required for an overflow to happen
		int timerResolution; // Clock cycle count between each timer tick
		bool timerEnabled; // Determines if the timer is enabled

		#endregion

		#region Reset

		partial void ResetTimer()
		{
			referenceTimerCycles = 0;
			dividerCycleOffset = 0;
			timerCycles = 0;
			timerResolution = 1024;
			timerOverflowCycles = 256 * timerResolution;
			timerEnabled = false;
		}

		#endregion

		#region GameBoy Timer

		private void ResetDivider() { dividerCycleOffset = (referenceTimerCycles & 0xFF) - referenceTimerCycles;}

		private byte GetDividerValue() { return (byte)((referenceTimerCycles + dividerCycleOffset) >> (colorMode ? 7 : 8)); }

		private unsafe void DisableTimer()
		{
			// This operation is useless if the timer has already been disabled, but it will not harm. (Just waste a few real CPU clock cycles)
			portMemory[0x05] = GetTimerValue();
			timerEnabled = false;
		}

		private unsafe void EnableTimer(int timerResolution)
		{
			// Store the current timer value somewhere safe
			byte value = portMemory[0x05] = GetTimerValue();

			// Initialize the timer cycle counter together with the timer overflow cycle count.
			timerCycles = (referenceTimerCycles % timerResolution) + (256 - value) * timerResolution - (timerOverflowCycles = (256 - value) * timerResolution);
			// Define the new timer resolution
			this.timerResolution = timerResolution;
			// Enable the timer
			timerEnabled = true;
		}

		private unsafe void SetTimerValue(byte value)
		{
			// Set the timer base value
			portMemory[0x05] = value;
			// Adjust the timer before doing anything else
			AdjustTimer();
			// Adjust the timer cycle counter if the timer is enabled
			if (timerEnabled) timerCycles = (timerCycles % timerResolution) + (256 - value) * timerResolution - timerOverflowCycles;
		}

		private void SetTimerOverflowValue(byte value)
		{
			// Adjust the timer before doing anything else
			AdjustTimer();
			// Adjust the timer cycle counter with the new timer overflow cycle count.
			timerCycles = timerCycles - timerOverflowCycles + (timerOverflowCycles = (256 - value) * timerResolution);
		}

		private unsafe byte GetTimerValue()
		{
			// Calculate the timer value if the timer is enabled, or return the stored value otherwise
			return timerEnabled ?
				(byte)(256 - (timerOverflowCycles + timerCycles) / timerResolution) :
				portMemory[0x05]; // The timer value is stored in TMA
		}

		private void AdjustTimer()
		{
			if (timerEnabled && timerCycles >= timerOverflowCycles)
			{
				InterruptRequest(0x04); // Request the TIMER interrupt
				timerCycles = timerCycles % timerOverflowCycles; // Adjust the cycle counter
			}
		}

		#endregion
	}
}
