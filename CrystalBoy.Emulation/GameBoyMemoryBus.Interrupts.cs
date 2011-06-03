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

		byte requestedInterrupts;

		#endregion

		#region Interrupt Management

		public int WaitForInterrupts()
		{
			int vbi, stat, timer;

			if (lcdEnabled && (EnabledInterrupts & 0x3) != 0)
			{
				vbi = FrameDuration - VerticalBlankDuration;
				if (cycleCount > vbi)
					vbi += FrameDuration;
				if (statInterruptEnabled)
					stat = statInterruptCycle;
				else
					stat = int.MaxValue;
			}
			else
			{
				vbi = int.MaxValue;
				stat = int.MaxValue;
			}

			timer = timerEnabled && (EnabledInterrupts & 0x4) != 0 ? timerInterruptCycle : int.MaxValue;

			vbi = Math.Min(vbi, Math.Min(stat, timer));

			return vbi == int.MaxValue ? -1 : vbi - cycleCount;
		}

		public void InterruptRequest(Interrupt interrupt) { requestedInterrupts |= (byte)interrupt; }

		public void InterruptRequest(byte interrupt) { requestedInterrupts |= interrupt; }

		public void InterruptHandled(Interrupt interrupt) { requestedInterrupts &= (byte)~interrupt; }

		public void InterruptHandled(byte interrupt) { requestedInterrupts &= (byte)~interrupt; }

		public byte RequestedInterrupts
		{
			get
			{
				// Check for VBLANK Interrupt
				if (lcdDrawing && cycleCount >= FrameDuration - VerticalBlankDuration)
				{
					requestedInterrupts |= 0x01;
					lcdDrawing = false;
				}
				// Check for STAT Interrupt
				if (statInterruptEnabled && cycleCount >= statInterruptCycle)
				{
					requestedInterrupts |= 0x02; // Request LCD status interrupt
					UpdateVideoStatusInterrupt(); // Update timings
				}
				// Check for TIMER Interrupt
				if (timerEnabled && cycleCount >= timerInterruptCycle)
				{
					requestedInterrupts |= 0x04; // Request timer interrupt
					TimerOverflow(); // Update timings
				}

				return requestedInterrupts;
			}
			set { requestedInterrupts = (byte)(value & 0x1F); }
		}

		public unsafe byte EnabledInterrupts { get { return portMemory[0xFF]; } }

		#endregion
	}
}
