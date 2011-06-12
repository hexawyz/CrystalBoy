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

		public const int ReferenceTimerTickCycles = 1024;
		public const int ReferenceTimerFrameShift = FrameDuration % ReferenceTimerTickCycles; // Shift amount for each frame

		#endregion

		#region Variables

		// Variables used for the reference 4096 Hz Timer
		// Since this timer is not synchronized with Refresh, it is necessary to keep track of the shift for each frame
		int referenceTimerShift; // Current frame timer shift
		// Variables used for the GB Programmable Timer
		// This timer is kept synchronized with the 4096 Hz reference timer
		int dividerBaseCycle, // Clock cycle used as reference for the divider (DIV) 0 value
			timerInterruptCycle, // Clock cycle representing timer overflow, will generate a TIMER interrupt
			timerBaseCycle, // Clock cycle corresponding to the last known timer value
			timerResolution; // Clock cycle amount between each timer tick
		byte timerBaseValue; // Last known timer value
		bool timerEnabled; // Determines if the timer is enabled

		#endregion

		#region Reset

		partial void ResetTimer()
		{
			referenceTimerShift = 0;
		}

		#endregion

		#region GameBoy Timer

		private void ResetDivider()
		{
			int reference;

			// Calculate the reference timer cycle
			reference = cycleCount + referenceTimerShift;
			// Calculate the divider base cycle, and make it negative
			dividerBaseCycle = reference - reference % 256; // The divider has a resolution of 256 clock cycles
		}

		private byte GetDividerValue()
		{
			if (doubleSpeed)
				return (byte)((cycleCount - dividerBaseCycle) / 128);
			else
				return (byte)((cycleCount - dividerBaseCycle) / 256);
		}

		private void TimerOverflow()
		{
			int reference;

			// Read timer modulo and reset the timer
			unsafe { timerBaseValue = portMemory[0x06]; } // TMA
			// Calculate the reference timer cycle
			reference = cycleCount + referenceTimerShift;
			// Use this to calculate the programmable timer cycle according to the requested resolution
			timerBaseCycle = reference - reference % timerResolution;
			// Calculate the timer interrupt cycle
			timerInterruptCycle = timerBaseCycle + (256 - timerBaseValue) * timerResolution;
		}

		private void DisableTimer()
		{
			// Define the timer base value
			timerBaseValue = GetTimerValue();
			// Disable the timer
			timerEnabled = false;
		}

		private void EnableTimer(int timerResolution)
		{
			int reference;

			// Set the current timer value as the base value
			timerBaseValue = GetTimerValue();
			// Calculate the reference timer cycle
			reference = cycleCount + referenceTimerShift;
			// Use this to calculate the programmable timer cycle according to the requested resolution
			timerBaseCycle = reference - reference % timerResolution;
			// Calculate the timer interrupt cycle
			timerInterruptCycle = timerBaseCycle + (256 - timerBaseValue) * timerResolution;
			// Define the new timer resolution
			this.timerResolution = timerResolution;
			// Enable the timer
			timerEnabled = true;
		}

		private void SetTimerValue(byte value)
		{
			int reference;

			// Set the timer base value
			timerBaseValue = value;
			// Adjust the timings if the timer is enabled
			if (timerEnabled)
			{
				// Calculate the reference timer cycle
				reference = cycleCount + referenceTimerShift;
				// Use this to calculate the programmable timer cycle according to the requested resolution
				timerBaseCycle = reference - reference % timerResolution;
				// Calculate the timer interrupt cycle
				timerInterruptCycle = timerBaseCycle + (256 - timerBaseValue) * timerResolution;
			}
		}

		private byte GetTimerValue()
		{
			// Calculate the timer value if the timer is enabled, or return the base value otherwise
			return timerEnabled ?
				(byte)(timerBaseValue + (cycleCount - timerBaseCycle) / timerResolution) :
				timerBaseValue;
		}

		#endregion
	}
}
