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
using System.Text;

namespace CrystalBoy.Emulation
{
	partial class Processor
	{
		public bool Emulate(bool finishFrame)
		{
			// Register variables, cloned here for efficiency (maybe it's an error, but it is easy to remove if needed)
			byte a, b, c, d, e, h, l, opcode;
			ushort sp, pc;
			bool zeroFlag, negationFlag, halfCarryFlag, carryFlag, ime;

			// Temporary result variable
			int temp;

			// Last instruction cycle count
			int cycleCount;

			// Temporary variables used internally for bus indirect operations
			byte __temp8;
			ushort __temp16, __tempHL;

			// Clone the register values into local variables
			a = A; b = B; c = C; d = D; e = E; h = H; l = L;
			sp = SP; pc = PC;

			// And clone the flags too
			zeroFlag = ZeroFlag;
			negationFlag = NegationFlag;
			halfCarryFlag = HalfCarryFlag;
			carryFlag = CarryFlag;
			ime = InterruptMasterEnable;

			// Initialize the count at 0 to please the compiler :(
			cycleCount = 0;

			try
			{
				do
				{
					// Check for pending interrupts
					if (ime && (temp = bus.EnabledInterrupts & bus.RequestedInterrupts) != 0)
					{
						// Push PC on the stack
						bus.WriteByte(--sp, (byte)(pc >> 8));
						bus.WriteByte(--sp, (byte)pc);
						// Disable interrupts
						ime = false;
						// Set PC to new value acording to requested interrupts
						if ((temp & 0x01) != 0)
						{
							bus.InterruptHandled(0x01);
							pc = 0x0040;
						}
						else if ((temp & 0x02) != 0)
						{
							bus.InterruptHandled(0x02);
							pc = 0x0048;
						}
						else if ((temp & 0x04) != 0)
						{
							bus.InterruptHandled(0x04);
							pc = 0x0050;
						}
						else if ((temp & 0x08) != 0)
						{
							bus.InterruptHandled(0x08);
							pc = 0x0058;
						}
						else if ((temp & 0x10) != 0)
						{
							bus.InterruptHandled(0x10);
							pc = 0x0060;
						}
						cycleCount = 20; // I don't know the exact interrupt timing but I read somewhere it is 20, so instead of 4 i put 20 here...
						goto HandleBreakpoints;
					}
%FETCH%
				HandleBreakpoints:
#if WITH_DEBUGGING
					// Handle breakpoints after running at least one instruction
					if (bus.BreakpointCount > 0) // Check for breakpoints only if there are some
						if (bus.IsBreakPoint(pc))
							return false; // Break when a breakpoint is encountered
#endif
				} while (bus.AddCycles(cycleCount) && finishFrame);

				return finishFrame; // Emulated with success
			}
			finally
			{
				// Save the local register values
				A = a; B = b; C = c; D = d; E = e; H = h; L = l;
				SP = sp; PC = pc;

				ZeroFlag = zeroFlag;
				NegationFlag = negationFlag;
				HalfCarryFlag = halfCarryFlag;
				CarryFlag = carryFlag;
				InterruptMasterEnable = ime;
			}
		}
	}
}
