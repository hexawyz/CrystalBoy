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

			// Quickly exit the routine if the CPU crashed
			if (status == ProcessorStatus.Crashed) return true;

			// Clone the register values into local variables
			a = A; b = B; c = C; d = D; e = E; h = H; l = L;
			sp = SP; pc = PC;

			// And clone the flags too (I probably need to change that…)
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

					opcode = bus[pc];

					if (!skipPcIncrement) pc++;
					else skipPcIncrement = false;

					switch (opcode)
					{
						case 0x00: /* NOP */
							cycleCount = 4;
							break;
						case 0x01: /* LD BC,N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							b = (byte)(__temp16 >> 8); c = (byte)(__temp16);
							cycleCount = 12;
							break;
						case 0x02: /* LD (BC),A */
							__temp8 = a;
							bus.WriteByte(c, b, __temp8);
							cycleCount = 8;
							break;
						case 0x03: /* INC BC */
							__temp16 = (ushort)((b << 8) | c);
							__temp16++;
							b = (byte)(__temp16 >> 8); c = (byte)(__temp16);
							cycleCount = 8;
							break;
						case 0x04: /* INC B */
							b++;
							zeroFlag = b == 0;
							halfCarryFlag = (b & 0xF) == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x05: /* DEC B */
							b--;
							zeroFlag = b == 0;
							halfCarryFlag = (b ^ 0xF) == 0;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x06: /* LD B,N */
							__temp8 = bus.ReadByte(pc++);
							b = __temp8;
							cycleCount = 8;
							break;
						case 0x07: /* RLCA */
							carryFlag = (a & 0x80) != 0;
							a = carryFlag ? (byte)((a << 1) | 0x01) : a = (byte)(a << 1);
							zeroFlag = false;
							negationFlag = false;
							halfCarryFlag = false;
							cycleCount = 4;
							break;
						case 0x08: /* LD (N),SP */
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							bus[__temp16++] = (byte)(sp);
							bus[__temp16] = (byte)(sp >> 8);
							break;
						case 0x09: /* ADD HL,BC */
							__tempHL = (ushort)((h << 8) | l);
							__temp16 = (ushort)((b << 8) | c);
							temp = __tempHL + __temp16;
							halfCarryFlag = (__tempHL & 0xFFF) + (__temp16 & 0xFFF) > 0xFFF;
							carryFlag = temp > 0xFFFF;
							__tempHL = (ushort)temp;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							negationFlag = false;
							cycleCount = 8;
							break;
						case 0x0A: /* LD A,(BC) */
							__temp8 = bus.ReadByte(c, b);
							a = __temp8;
							cycleCount = 8;
							break;
						case 0x0B: /* DEC BC */
							__temp16 = (ushort)((b << 8) | c);
							__temp16--;
							b = (byte)(__temp16 >> 8); c = (byte)(__temp16);
							cycleCount = 8;
							break;
						case 0x0C: /* INC C */
							c++;
							zeroFlag = c == 0;
							halfCarryFlag = (c & 0xF) == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x0D: /* DEC C */
							c--;
							zeroFlag = c == 0;
							halfCarryFlag = (c ^ 0xF) == 0;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x0E: /* LD C,N */
							__temp8 = bus.ReadByte(pc++);
							c = __temp8;
							cycleCount = 8;
							break;
						case 0x0F: /* RRCA */
							carryFlag = (a & 0x01) != 0;
							a = carryFlag ? (byte)((a >> 1) | 0x80) : (byte)(a >> 1);
							zeroFlag = false;
							negationFlag = false;
							halfCarryFlag = false;
							cycleCount = 4;
							break;
						case 0x10: /* STOP */
							status = ProcessorStatus.Stopped;
							cycleCount = bus.HandleProcessorStop();
							if (cycleCount < 0)
								return false;
							status = ProcessorStatus.Running;
							cycleCount = 4;
							break;
						case 0x11: /* LD DE,N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							d = (byte)(__temp16 >> 8); e = (byte)(__temp16);
							cycleCount = 12;
							break;
						case 0x12: /* LD (DE),A */
							__temp8 = a;
							bus.WriteByte(e, d, __temp8);
							cycleCount = 8;
							break;
						case 0x13: /* INC DE */
							__temp16 = (ushort)((d << 8) | e);
							__temp16++;
							d = (byte)(__temp16 >> 8); e = (byte)(__temp16);
							cycleCount = 8;
							break;
						case 0x14: /* INC D */
							d++;
							zeroFlag = d == 0;
							halfCarryFlag = (d & 0xF) == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x15: /* DEC D */
							d--;
							zeroFlag = d == 0;
							halfCarryFlag = (d ^ 0xF) == 0;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x16: /* LD D,N */
							__temp8 = bus.ReadByte(pc++);
							d = __temp8;
							cycleCount = 8;
							break;
						case 0x17: /* RLA */
							temp = carryFlag ? (a << 1) | 0x01 : a << 1;
							carryFlag = (a & 0x80) != 0;
							a = (byte)temp;
							zeroFlag = false;
							negationFlag = false;
							halfCarryFlag = false;
							cycleCount = 4;
							break;
						case 0x18: /* JR N */
							__temp16 = (ushort)(sbyte)bus.ReadByte(pc++);
							pc += __temp16;
							cycleCount = 12;
							break;
						case 0x19: /* ADD HL,DE */
							__tempHL = (ushort)((h << 8) | l);
							__temp16 = (ushort)((d << 8) | e);
							temp = __tempHL + __temp16;
							halfCarryFlag = (__tempHL & 0xFFF) + (__temp16 & 0xFFF) > 0xFFF;
							carryFlag = temp > 0xFFFF;
							__tempHL = (ushort)temp;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							negationFlag = false;
							cycleCount = 8;
							break;
						case 0x1A: /* LD A,(DE) */
							__temp8 = bus.ReadByte(e, d);
							a = __temp8;
							cycleCount = 8;
							break;
						case 0x1B: /* DEC DE */
							__temp16 = (ushort)((d << 8) | e);
							__temp16--;
							d = (byte)(__temp16 >> 8); e = (byte)(__temp16);
							cycleCount = 8;
							break;
						case 0x1C: /* INC E */
							e++;
							zeroFlag = e == 0;
							halfCarryFlag = (e & 0xF) == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x1D: /* DEC E */
							e--;
							zeroFlag = e == 0;
							halfCarryFlag = (e ^ 0xF) == 0;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x1E: /* LD E,N */
							__temp8 = bus.ReadByte(pc++);
							e = __temp8;
							cycleCount = 8;
							break;
						case 0x1F: /* RRA */
							temp = carryFlag ? (a >> 1) | 0x80 : a >> 1;
							carryFlag = (a & 0x01) != 0;
							a = (byte)temp;
							zeroFlag = false;
							negationFlag = false;
							halfCarryFlag = false;
							cycleCount = 4;
							break;
						case 0x20: /* JR NZ,N */
							__temp16 = (ushort)(sbyte)bus.ReadByte(pc++);
							if (!zeroFlag)
							{
								pc += __temp16;
								cycleCount = 12;
							}
							else cycleCount = 8;
							break;
						case 0x21: /* LD HL,N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							__tempHL = __temp16;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							cycleCount = 12;
							break;
						case 0x22: /* LDI (HL),A */
							__temp8 = a;
							bus.WriteByte(l, h, __temp8);
							if (++l == 0) h++;
							cycleCount = 8;
							break;
						case 0x23: /* INC HL */
							__tempHL = (ushort)((h << 8) | l);
							__tempHL++;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							cycleCount = 8;
							break;
						case 0x24: /* INC H */
							h++;
							zeroFlag = h == 0;
							halfCarryFlag = (h & 0xF) == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x25: /* DEC H */
							h--;
							zeroFlag = h == 0;
							halfCarryFlag = (h ^ 0xF) == 0;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x26: /* LD H,N */
							__temp8 = bus.ReadByte(pc++);
							h = __temp8;
							cycleCount = 8;
							break;
						case 0x27: /* DAA */
							if (!negationFlag)
							{
								if (halfCarryFlag || (a & 0x0F) > 0x09) a += 0x06;
								if (carryFlag = (carryFlag || a > 0x99)) a += 0x60;
							}
							else
							{
								if (halfCarryFlag) a -= 0x06;
								if (carryFlag) a -= 0x60;
							}
							zeroFlag = a == 0;
							halfCarryFlag = false;
							cycleCount = 4;
							break;
						case 0x28: /* JR Z,N */
							__temp16 = (ushort)(sbyte)bus.ReadByte(pc++);
							if (zeroFlag)
							{
								pc += __temp16;
								cycleCount = 12;
							}
							else cycleCount = 8;
							break;
						case 0x29: /* ADD HL,HL */
							__tempHL = (ushort)((h << 8) | l);
							__tempHL = (ushort)((h << 8) | l);
							temp = __tempHL + __tempHL;
							halfCarryFlag = (__tempHL & 0xFFF) + (__tempHL & 0xFFF) > 0xFFF;
							carryFlag = temp > 0xFFFF;
							__tempHL = (ushort)temp;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							negationFlag = false;
							cycleCount = 8;
							break;
						case 0x2A: /* LDI A,(HL) */
							__temp8 = bus.ReadByte(l, h);
							a = __temp8;
							if (++l == 0) h++;
							cycleCount = 8;
							break;
						case 0x2B: /* DEC HL */
							__tempHL = (ushort)((h << 8) | l);
							__tempHL--;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							cycleCount = 8;
							break;
						case 0x2C: /* INC L */
							l++;
							zeroFlag = l == 0;
							halfCarryFlag = (l & 0xF) == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x2D: /* DEC L */
							l--;
							zeroFlag = l == 0;
							halfCarryFlag = (l ^ 0xF) == 0;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x2E: /* LD L,N */
							__temp8 = bus.ReadByte(pc++);
							l = __temp8;
							cycleCount = 8;
							break;
						case 0x2F: /* CPL */
							a = (byte)~a;
							negationFlag = true;
							halfCarryFlag = true;
							cycleCount = 4;
							break;
						case 0x30: /* JR NC,N */
							__temp16 = (ushort)(sbyte)bus.ReadByte(pc++);
							if (!carryFlag)
							{
								pc += __temp16;
								cycleCount = 12;
							}
							else cycleCount = 8;
							break;
						case 0x31: /* LD SP,N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							sp = __temp16;
							cycleCount = 12;
							break;
						case 0x32: /* LDD (HL),A */
							__temp8 = a;
							bus.WriteByte(l, h, __temp8);
							if (l-- == 0) h--;
							cycleCount = 8;
							break;
						case 0x33: /* INC SP */
							sp++;
							cycleCount = 8;
							break;
						case 0x34: /* INC (HL) */
							__temp8 = bus.ReadByte(l, h);
							__temp8++;
							zeroFlag = __temp8 == 0;
							halfCarryFlag = (__temp8 & 0xF) == 0;
							bus.WriteByte(l, h, __temp8);
							negationFlag = false;
							cycleCount = 12;
							break;
						case 0x35: /* DEC (HL) */
							__temp8 = bus.ReadByte(l, h);
							__temp8--;
							zeroFlag = __temp8 == 0;
							halfCarryFlag = (__temp8 ^ 0xF) == 0;
							bus.WriteByte(l, h, __temp8);
							negationFlag = true;
							cycleCount = 12;
							break;
						case 0x36: /* LD (HL),N */
							__temp8 = bus.ReadByte(pc++);
							bus.WriteByte(l, h, __temp8);
							cycleCount = 12;
							break;
						case 0x37: /* SCF */
							carryFlag = true;
							negationFlag = false;
							halfCarryFlag = false;
							cycleCount = 4;
							break;
						case 0x38: /* JR C,N */
							__temp16 = (ushort)(sbyte)bus.ReadByte(pc++);
							if (carryFlag)
							{
								pc += __temp16;
								cycleCount = 12;
							}
							else cycleCount = 8;
							break;
						case 0x39: /* ADD HL,SP */
							__tempHL = (ushort)((h << 8) | l);
							temp = __tempHL + sp;
							halfCarryFlag = (__tempHL & 0xFFF) + (sp & 0xFFF) > 0xFFF;
							carryFlag = temp > 0xFFFF;
							__tempHL = (ushort)temp;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							negationFlag = false;
							cycleCount = 8;
							break;
						case 0x3A: /* LDD A,(HL) */
							__temp8 = bus.ReadByte(l, h);
							a = __temp8;
							if (l-- == 0) h--;
							cycleCount = 8;
							break;
						case 0x3B: /* DEC SP */
							sp--;
							cycleCount = 8;
							break;
						case 0x3C: /* INC A */
							a++;
							zeroFlag = a == 0;
							halfCarryFlag = (a & 0xF) == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x3D: /* DEC A */
							a--;
							zeroFlag = a == 0;
							halfCarryFlag = (a ^ 0xF) == 0;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x3E: /* LD A,N */
							__temp8 = bus.ReadByte(pc++);
							a = __temp8;
							cycleCount = 8;
							break;
						case 0x3F: /* CCF */
							carryFlag = !carryFlag;
							negationFlag = false;
							halfCarryFlag = false;
							cycleCount = 4;
							break;
						case 0x40: /* LD B,B */
							cycleCount = 4;
							break;
						case 0x41: /* LD B,C */
							b = c;
							cycleCount = 4;
							break;
						case 0x42: /* LD B,D */
							b = d;
							cycleCount = 4;
							break;
						case 0x43: /* LD B,E */
							b = e;
							cycleCount = 4;
							break;
						case 0x44: /* LD B,H */
							b = h;
							cycleCount = 4;
							break;
						case 0x45: /* LD B,L */
							b = l;
							cycleCount = 4;
							break;
						case 0x46: /* LD B,(HL) */
							__temp8 = bus.ReadByte(l, h);
							b = __temp8;
							cycleCount = 8;
							break;
						case 0x47: /* LD B,A */
							b = a;
							cycleCount = 4;
							break;
						case 0x48: /* LD C,B */
							c = b;
							cycleCount = 4;
							break;
						case 0x49: /* LD C,C */
							cycleCount = 4;
							break;
						case 0x4A: /* LD C,D */
							c = d;
							cycleCount = 4;
							break;
						case 0x4B: /* LD C,E */
							c = e;
							cycleCount = 4;
							break;
						case 0x4C: /* LD C,H */
							c = h;
							cycleCount = 4;
							break;
						case 0x4D: /* LD C,L */
							c = l;
							cycleCount = 4;
							break;
						case 0x4E: /* LD C,(HL) */
							__temp8 = bus.ReadByte(l, h);
							c = __temp8;
							cycleCount = 8;
							break;
						case 0x4F: /* LD C,A */
							c = a;
							cycleCount = 4;
							break;
						case 0x50: /* LD D,B */
							d = b;
							cycleCount = 4;
							break;
						case 0x51: /* LD D,C */
							d = c;
							cycleCount = 4;
							break;
						case 0x52: /* LD D,D */
							cycleCount = 4;
							break;
						case 0x53: /* LD D,E */
							d = e;
							cycleCount = 4;
							break;
						case 0x54: /* LD D,H */
							d = h;
							cycleCount = 4;
							break;
						case 0x55: /* LD D,L */
							d = l;
							cycleCount = 4;
							break;
						case 0x56: /* LD D,(HL) */
							__temp8 = bus.ReadByte(l, h);
							d = __temp8;
							cycleCount = 8;
							break;
						case 0x57: /* LD D,A */
							d = a;
							cycleCount = 4;
							break;
						case 0x58: /* LD E,B */
							e = b;
							cycleCount = 4;
							break;
						case 0x59: /* LD E,C */
							e = c;
							cycleCount = 4;
							break;
						case 0x5A: /* LD E,D */
							e = d;
							cycleCount = 4;
							break;
						case 0x5B: /* LD E,E */
							cycleCount = 4;
							break;
						case 0x5C: /* LD E,H */
							e = h;
							cycleCount = 4;
							break;
						case 0x5D: /* LD E,L */
							e = l;
							cycleCount = 4;
							break;
						case 0x5E: /* LD E,(HL) */
							__temp8 = bus.ReadByte(l, h);
							e = __temp8;
							cycleCount = 8;
							break;
						case 0x5F: /* LD E,A */
							e = a;
							cycleCount = 4;
							break;
						case 0x60: /* LD H,B */
							h = b;
							cycleCount = 4;
							break;
						case 0x61: /* LD H,C */
							h = c;
							cycleCount = 4;
							break;
						case 0x62: /* LD H,D */
							h = d;
							cycleCount = 4;
							break;
						case 0x63: /* LD H,E */
							h = e;
							cycleCount = 4;
							break;
						case 0x64: /* LD H,H */
							cycleCount = 4;
							break;
						case 0x65: /* LD H,L */
							h = l;
							cycleCount = 4;
							break;
						case 0x66: /* LD H,(HL) */
							__temp8 = bus.ReadByte(l, h);
							h = __temp8;
							cycleCount = 8;
							break;
						case 0x67: /* LD H,A */
							h = a;
							cycleCount = 4;
							break;
						case 0x68: /* LD L,B */
							l = b;
							cycleCount = 4;
							break;
						case 0x69: /* LD L,C */
							l = c;
							cycleCount = 4;
							break;
						case 0x6A: /* LD L,D */
							l = d;
							cycleCount = 4;
							break;
						case 0x6B: /* LD L,E */
							l = e;
							cycleCount = 4;
							break;
						case 0x6C: /* LD L,H */
							l = h;
							cycleCount = 4;
							break;
						case 0x6D: /* LD L,L */
							cycleCount = 4;
							break;
						case 0x6E: /* LD L,(HL) */
							__temp8 = bus.ReadByte(l, h);
							l = __temp8;
							cycleCount = 8;
							break;
						case 0x6F: /* LD L,A */
							l = a;
							cycleCount = 4;
							break;
						case 0x70: /* LD (HL),B */
							__temp8 = b;
							bus.WriteByte(l, h, __temp8);
							cycleCount = 8;
							break;
						case 0x71: /* LD (HL),C */
							__temp8 = c;
							bus.WriteByte(l, h, __temp8);
							cycleCount = 8;
							break;
						case 0x72: /* LD (HL),D */
							__temp8 = d;
							bus.WriteByte(l, h, __temp8);
							cycleCount = 8;
							break;
						case 0x73: /* LD (HL),E */
							__temp8 = e;
							bus.WriteByte(l, h, __temp8);
							cycleCount = 8;
							break;
						case 0x74: /* LD (HL),H */
							__temp8 = h;
							bus.WriteByte(l, h, __temp8);
							cycleCount = 8;
							break;
						case 0x75: /* LD (HL),L */
							__temp8 = l;
							bus.WriteByte(l, h, __temp8);
							cycleCount = 8;
							break;
						case 0x76: /* HALT */
							if (enableInterruptDelay > 0) pc--; // Case where HALT directly follows EI
							// Still need a better emulation of the HALT opcode, but this one will work for now
							else if (ime || bus.EnabledInterrupts != 0)
							{
								if ((bus.EnabledInterrupts & bus.RequestedInterrupts) == 0)
								{
									status = ProcessorStatus.Halted;
									cycleCount = bus.WaitForInterrupts();
									if (cycleCount < 0) return false;
									if ((cycleCount & 0x3) != 0) cycleCount += 4 - (cycleCount & 0x3); // Keep the cycle count as a multiple of 4
									status = ProcessorStatus.Running;
								}
								else cycleCount = 4;
							}
							break;
						case 0x77: /* LD (HL),A */
							__temp8 = a;
							bus.WriteByte(l, h, __temp8);
							cycleCount = 8;
							break;
						case 0x78: /* LD A,B */
							a = b;
							cycleCount = 4;
							break;
						case 0x79: /* LD A,C */
							a = c;
							cycleCount = 4;
							break;
						case 0x7A: /* LD A,D */
							a = d;
							cycleCount = 4;
							break;
						case 0x7B: /* LD A,E */
							a = e;
							cycleCount = 4;
							break;
						case 0x7C: /* LD A,H */
							a = h;
							cycleCount = 4;
							break;
						case 0x7D: /* LD A,L */
							a = l;
							cycleCount = 4;
							break;
						case 0x7E: /* LD A,(HL) */
							__temp8 = bus.ReadByte(l, h);
							a = __temp8;
							cycleCount = 8;
							break;
						case 0x7F: /* LD A,A */
							cycleCount = 4;
							break;
						case 0x80: /* ADD A,B */
							temp = a + b;
							carryFlag = temp > 0xFF;
							halfCarryFlag = (a & 0xF) + (b & 0xF) > 0xF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x81: /* ADD A,C */
							temp = a + c;
							carryFlag = temp > 0xFF;
							halfCarryFlag = (a & 0xF) + (c & 0xF) > 0xF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x82: /* ADD A,D */
							temp = a + d;
							carryFlag = temp > 0xFF;
							halfCarryFlag = (a & 0xF) + (d & 0xF) > 0xF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x83: /* ADD A,E */
							temp = a + e;
							carryFlag = temp > 0xFF;
							halfCarryFlag = (a & 0xF) + (e & 0xF) > 0xF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x84: /* ADD A,H */
							temp = a + h;
							carryFlag = temp > 0xFF;
							halfCarryFlag = (a & 0xF) + (h & 0xF) > 0xF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x85: /* ADD A,L */
							temp = a + l;
							carryFlag = temp > 0xFF;
							halfCarryFlag = (a & 0xF) + (l & 0xF) > 0xF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x86: /* ADD A,(HL) */
							__temp8 = bus.ReadByte(l, h);
							temp = a + __temp8;
							carryFlag = temp > 0xFF;
							halfCarryFlag = (a & 0xF) + (__temp8 & 0xF) > 0xF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 8;
							break;
						case 0x87: /* ADD A,A */
							temp = a + a;
							carryFlag = temp > 0xFF;
							halfCarryFlag = (a & 0xF) + (a & 0xF) > 0xF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x88: /* ADC A,B */
							if (carryFlag)
							{
								temp = a + b + 1;
								halfCarryFlag = (a & 0xF) + (b & 0xF) > 0xE;
							}
							else
							{
								temp = a + b;
								halfCarryFlag = (a & 0xF) + (b & 0xF) > 0xF;
							}
							carryFlag = temp > 0xFF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x89: /* ADC A,C */
							if (carryFlag)
							{
								temp = a + c + 1;
								halfCarryFlag = (a & 0xF) + (c & 0xF) > 0xE;
							}
							else
							{
								temp = a + c;
								halfCarryFlag = (a & 0xF) + (c & 0xF) > 0xF;
							}
							carryFlag = temp > 0xFF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x8A: /* ADC A,D */
							if (carryFlag)
							{
								temp = a + d + 1;
								halfCarryFlag = (a & 0xF) + (d & 0xF) > 0xE;
							}
							else
							{
								temp = a + d;
								halfCarryFlag = (a & 0xF) + (d & 0xF) > 0xF;
							}
							carryFlag = temp > 0xFF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x8B: /* ADC A,E */
							if (carryFlag)
							{
								temp = a + e + 1;
								halfCarryFlag = (a & 0xF) + (e & 0xF) > 0xE;
							}
							else
							{
								temp = a + e;
								halfCarryFlag = (a & 0xF) + (e & 0xF) > 0xF;
							}
							carryFlag = temp > 0xFF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x8C: /* ADC A,H */
							if (carryFlag)
							{
								temp = a + h + 1;
								halfCarryFlag = (a & 0xF) + (h & 0xF) > 0xE;
							}
							else
							{
								temp = a + h;
								halfCarryFlag = (a & 0xF) + (h & 0xF) > 0xF;
							}
							carryFlag = temp > 0xFF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x8D: /* ADC A,L */
							if (carryFlag)
							{
								temp = a + l + 1;
								halfCarryFlag = (a & 0xF) + (l & 0xF) > 0xE;
							}
							else
							{
								temp = a + l;
								halfCarryFlag = (a & 0xF) + (l & 0xF) > 0xF;
							}
							carryFlag = temp > 0xFF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x8E: /* ADC A,(HL) */
							__temp8 = bus.ReadByte(l, h);
							if (carryFlag)
							{
								temp = a + __temp8 + 1;
								halfCarryFlag = (a & 0xF) + (__temp8 & 0xF) > 0xE;
							}
							else
							{
								temp = a + __temp8;
								halfCarryFlag = (a & 0xF) + (__temp8 & 0xF) > 0xF;
							}
							carryFlag = temp > 0xFF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 8;
							break;
						case 0x8F: /* ADC A,A */
							if (carryFlag)
							{
								temp = a + a + 1;
								halfCarryFlag = (a & 0xF) + (a & 0xF) > 0xE;
							}
							else
							{
								temp = a + a;
								halfCarryFlag = (a & 0xF) + (a & 0xF) > 0xF;
							}
							carryFlag = temp > 0xFF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 4;
							break;
						case 0x90: /* SUB A,B */
							halfCarryFlag = (a & 0xF) < (b & 0xF);
							carryFlag = a < b;
							zeroFlag = (a -= b) == 0;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x91: /* SUB A,C */
							halfCarryFlag = (a & 0xF) < (c & 0xF);
							carryFlag = a < c;
							zeroFlag = (a -= c) == 0;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x92: /* SUB A,D */
							halfCarryFlag = (a & 0xF) < (d & 0xF);
							carryFlag = a < d;
							zeroFlag = (a -= d) == 0;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x93: /* SUB A,E */
							halfCarryFlag = (a & 0xF) < (e & 0xF);
							carryFlag = a < e;
							zeroFlag = (a -= e) == 0;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x94: /* SUB A,H */
							halfCarryFlag = (a & 0xF) < (h & 0xF);
							carryFlag = a < h;
							zeroFlag = (a -= h) == 0;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x95: /* SUB A,L */
							halfCarryFlag = (a & 0xF) < (l & 0xF);
							carryFlag = a < l;
							zeroFlag = (a -= l) == 0;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x96: /* SUB A,(HL) */
							__temp8 = bus.ReadByte(l, h);
							halfCarryFlag = (a & 0xF) < (__temp8 & 0xF);
							carryFlag = a < __temp8;
							zeroFlag = (a -= __temp8) == 0;
							negationFlag = true;
							cycleCount = 8;
							break;
						case 0x97: /* SUB A,A */
							halfCarryFlag = false;
							carryFlag = false;
							a = 0;
							zeroFlag = true;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x98: /* SBC A,B */
							if (carryFlag)
							{
								halfCarryFlag = (a & 0xF) - (b & 0xF) < 1;
								carryFlag = a - b < 1;
								zeroFlag = (a = (byte)(a - b - 1)) == 0;
							}
							else
							{
								halfCarryFlag = (a & 0xF) < (b & 0xF);
								carryFlag = a < b;
								zeroFlag = (a -= b) == 0;
							}
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x99: /* SBC A,C */
							if (carryFlag)
							{
								halfCarryFlag = (a & 0xF) - (c & 0xF) < 1;
								carryFlag = a - c < 1;
								zeroFlag = (a = (byte)(a - c - 1)) == 0;
							}
							else
							{
								halfCarryFlag = (a & 0xF) < (c & 0xF);
								carryFlag = a < c;
								zeroFlag = (a -= c) == 0;
							}
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x9A: /* SBC A,D */
							if (carryFlag)
							{
								halfCarryFlag = (a & 0xF) - (d & 0xF) < 1;
								carryFlag = a - d < 1;
								zeroFlag = (a = (byte)(a - d - 1)) == 0;
							}
							else
							{
								halfCarryFlag = (a & 0xF) < (d & 0xF);
								carryFlag = a < d;
								zeroFlag = (a -= d) == 0;
							}
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x9B: /* SBC A,E */
							if (carryFlag)
							{
								halfCarryFlag = (a & 0xF) - (e & 0xF) < 1;
								carryFlag = a - e < 1;
								zeroFlag = (a = (byte)(a - e - 1)) == 0;
							}
							else
							{
								halfCarryFlag = (a & 0xF) < (e & 0xF);
								carryFlag = a < e;
								zeroFlag = (a -= e) == 0;
							}
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x9C: /* SBC A,H */
							if (carryFlag)
							{
								halfCarryFlag = (a & 0xF) - (h & 0xF) < 1;
								carryFlag = a - h < 1;
								zeroFlag = (a = (byte)(a - h - 1)) == 0;
							}
							else
							{
								halfCarryFlag = (a & 0xF) < (h & 0xF);
								carryFlag = a < h;
								zeroFlag = (a -= h) == 0;
							}
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x9D: /* SBC A,L */
							if (carryFlag)
							{
								halfCarryFlag = (a & 0xF) - (l & 0xF) < 1;
								carryFlag = a - l < 1;
								zeroFlag = (a = (byte)(a - l - 1)) == 0;
							}
							else
							{
								halfCarryFlag = (a & 0xF) < (l & 0xF);
								carryFlag = a < l;
								zeroFlag = (a -= l) == 0;
							}
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0x9E: /* SBC A,(HL) */
							__temp8 = bus.ReadByte(l, h);
							if (carryFlag)
							{
								halfCarryFlag = (a & 0xF) - (__temp8 & 0xF) < 1;
								carryFlag = a - __temp8 < 1;
								zeroFlag = (a = (byte)(a - __temp8 - 1)) == 0;
							}
							else
							{
								halfCarryFlag = (a & 0xF) < (__temp8 & 0xF);
								carryFlag = a < __temp8;
								zeroFlag = (a -= __temp8) == 0;
							}
							negationFlag = true;
							cycleCount = 8;
							break;
						case 0x9F: /* SBC A,A */
							if (carryFlag)
							{
								halfCarryFlag = true;
								//carryFlag = true;
								a = 0xFF;
								zeroFlag = false;
							}
							else
							{
								halfCarryFlag = false;
								//carryFlag = false;
								a = 0;
								zeroFlag = true;
							}
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0xA0: /* AND A,B */
							a &= b;
							zeroFlag = a == 0;
							halfCarryFlag = true;
							negationFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xA1: /* AND A,C */
							a &= c;
							zeroFlag = a == 0;
							halfCarryFlag = true;
							negationFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xA2: /* AND A,D */
							a &= d;
							zeroFlag = a == 0;
							halfCarryFlag = true;
							negationFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xA3: /* AND A,E */
							a &= e;
							zeroFlag = a == 0;
							halfCarryFlag = true;
							negationFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xA4: /* AND A,H */
							a &= h;
							zeroFlag = a == 0;
							halfCarryFlag = true;
							negationFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xA5: /* AND A,L */
							a &= l;
							zeroFlag = a == 0;
							halfCarryFlag = true;
							negationFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xA6: /* AND A,(HL) */
							__temp8 = bus.ReadByte(l, h);
							a &= __temp8;
							zeroFlag = a == 0;
							halfCarryFlag = true;
							negationFlag = false;
							carryFlag = false;
							cycleCount = 8;
							break;
						case 0xA7: /* AND A,A */
							zeroFlag = a == 0;
							halfCarryFlag = true;
							negationFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xA8: /* XOR A,B */
							a ^= b;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xA9: /* XOR A,C */
							a ^= c;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xAA: /* XOR A,D */
							a ^= d;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xAB: /* XOR A,E */
							a ^= e;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xAC: /* XOR A,H */
							a ^= h;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xAD: /* XOR A,L */
							a ^= l;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xAE: /* XOR A,(HL) */
							__temp8 = bus.ReadByte(l, h);
							a ^= __temp8;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 8;
							break;
						case 0xAF: /* XOR A,A */
							a = 0;
							zeroFlag = true;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xB0: /* OR A,B */
							a |= b;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xB1: /* OR A,C */
							a |= c;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xB2: /* OR A,D */
							a |= d;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xB3: /* OR A,E */
							a |= e;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xB4: /* OR A,H */
							a |= h;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xB5: /* OR A,L */
							a |= l;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xB6: /* OR A,(HL) */
							__temp8 = bus.ReadByte(l, h);
							a |= __temp8;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 8;
							break;
						case 0xB7: /* OR A,A */
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 4;
							break;
						case 0xB8: /* CP A,B */
							halfCarryFlag = (a & 0xF) < (b & 0xF);
							carryFlag = a < b;
							zeroFlag = a == b;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0xB9: /* CP A,C */
							halfCarryFlag = (a & 0xF) < (c & 0xF);
							carryFlag = a < c;
							zeroFlag = a == c;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0xBA: /* CP A,D */
							halfCarryFlag = (a & 0xF) < (d & 0xF);
							carryFlag = a < d;
							zeroFlag = a == d;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0xBB: /* CP A,E */
							halfCarryFlag = (a & 0xF) < (e & 0xF);
							carryFlag = a < e;
							zeroFlag = a == e;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0xBC: /* CP A,H */
							halfCarryFlag = (a & 0xF) < (h & 0xF);
							carryFlag = a < h;
							zeroFlag = a == h;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0xBD: /* CP A,L */
							halfCarryFlag = (a & 0xF) < (l & 0xF);
							carryFlag = a < l;
							zeroFlag = a == l;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0xBE: /* CP A,(HL) */
							__temp8 = bus.ReadByte(l, h);
							halfCarryFlag = (a & 0xF) < (__temp8 & 0xF);
							carryFlag = a < __temp8;
							zeroFlag = a == __temp8;
							negationFlag = true;
							cycleCount = 8;
							break;
						case 0xBF: /* CP A,A */
							halfCarryFlag = false;
							carryFlag = false;
							zeroFlag = true;
							negationFlag = true;
							cycleCount = 4;
							break;
						case 0xC0: /* RET NZ */
							if (!zeroFlag)
							{
								pc = (ushort)(bus[sp++] | (bus[sp++] << 8));
								cycleCount = 20;
							}
							else cycleCount = 8;
							break;
						case 0xC1: /* POP BC */
							__temp16 = (ushort)(bus[sp++] | (bus[sp++] << 8));
							b = (byte)(__temp16 >> 8); c = (byte)(__temp16);
							cycleCount = 12;
							break;
						case 0xC2: /* JP NZ,N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							if (!zeroFlag)
							{
								pc = __temp16;
								cycleCount = 16;
							}
							else cycleCount = 12;
							break;
						case 0xC3: /* JP N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							pc = __temp16;
							cycleCount = 16;
							break;
						case 0xC4: /* CALL NZ,N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							if (!zeroFlag)
							{
								bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = __temp16;
								cycleCount = 24;
							}
							else cycleCount = 12;
							break;
						case 0xC5: /* PUSH BC */
							__temp16 = (ushort)((b << 8) | c);
							bus[--sp] = (byte)(__temp16 >> 8);
							bus[--sp] = (byte)__temp16;
							cycleCount = 16;
							break;
						case 0xC6: /* ADD A,N */
							__temp8 = bus.ReadByte(pc++);
							temp = a + __temp8;
							carryFlag = temp > 0xFF;
							halfCarryFlag = (a & 0xF) + (__temp8 & 0xF) > 0xF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 8;
							break;
						case 0xC7: /* RST $00 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 0;
							cycleCount = 16;
							break;
						case 0xC8: /* RET Z */
							if (zeroFlag)
							{
								pc = (ushort)(bus[sp++] | (bus[sp++] << 8));
								cycleCount = 20;
							}
							else cycleCount = 8;
							break;
						case 0xC9: /* RET */
							pc = (ushort)(bus[sp++] | (bus[sp++] << 8));
							cycleCount = 16;
							break;
						case 0xCA: /* JP Z,N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							if (zeroFlag)
							{
								pc = __temp16;
								cycleCount = 16;
							}
							else cycleCount = 12;
							break;
						case 0xCB: /* Extended opcodes */
							opcode = bus[pc++];

							switch (opcode)
							{
								case /* 0xCB */ 0x00: /* RLC B */
									carryFlag = (b & 0x80) != 0;
									zeroFlag = (b = carryFlag ? (byte)((b << 1) | 0x01) : (byte)(b << 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x01: /* RLC C */
									carryFlag = (c & 0x80) != 0;
									zeroFlag = (c = carryFlag ? (byte)((c << 1) | 0x01) : (byte)(c << 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x02: /* RLC D */
									carryFlag = (d & 0x80) != 0;
									zeroFlag = (d = carryFlag ? (byte)((d << 1) | 0x01) : (byte)(d << 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x03: /* RLC E */
									carryFlag = (e & 0x80) != 0;
									zeroFlag = (e = carryFlag ? (byte)((e << 1) | 0x01) : (byte)(e << 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x04: /* RLC H */
									carryFlag = (h & 0x80) != 0;
									zeroFlag = (h = carryFlag ? (byte)((h << 1) | 0x01) : (byte)(h << 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x05: /* RLC L */
									carryFlag = (l & 0x80) != 0;
									zeroFlag = (l = carryFlag ? (byte)((l << 1) | 0x01) : (byte)(l << 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x06: /* RLC (HL) */
									__temp8 = bus.ReadByte(l, h);
									carryFlag = (__temp8 & 0x80) != 0;
									zeroFlag = (__temp8 = carryFlag ? (byte)((__temp8 << 1) | 0x01) : (byte)(__temp8 << 1)) == 0;
									bus.WriteByte(l, h, __temp8);
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x07: /* RLC A */
									carryFlag = (a & 0x80) != 0;
									zeroFlag = (a = carryFlag ? (byte)((a << 1) | 0x01) : (byte)(a << 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x08: /* RRC B */
									carryFlag = (b & 0x01) != 0;
									zeroFlag = (b = carryFlag ? (byte)((b >> 1) | 0x80) : (byte)(b >> 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x09: /* RRC C */
									carryFlag = (c & 0x01) != 0;
									zeroFlag = (c = carryFlag ? (byte)((c >> 1) | 0x80) : (byte)(c >> 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x0A: /* RRC D */
									carryFlag = (d & 0x01) != 0;
									zeroFlag = (d = carryFlag ? (byte)((d >> 1) | 0x80) : (byte)(d >> 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x0B: /* RRC E */
									carryFlag = (e & 0x01) != 0;
									zeroFlag = (e = carryFlag ? (byte)((e >> 1) | 0x80) : (byte)(e >> 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x0C: /* RRC H */
									carryFlag = (h & 0x01) != 0;
									zeroFlag = (h = carryFlag ? (byte)((h >> 1) | 0x80) : (byte)(h >> 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x0D: /* RRC L */
									carryFlag = (l & 0x01) != 0;
									zeroFlag = (l = carryFlag ? (byte)((l >> 1) | 0x80) : (byte)(l >> 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x0E: /* RRC (HL) */
									__temp8 = bus.ReadByte(l, h);
									carryFlag = (__temp8 & 0x01) != 0;
									zeroFlag = (__temp8 = carryFlag ? (byte)((__temp8 >> 1) | 0x80) : (byte)(__temp8 >> 1)) == 0;
									bus.WriteByte(l, h, __temp8);
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x0F: /* RRC A */
									carryFlag = (a & 0x01) != 0;
									zeroFlag = (a = carryFlag ? (byte)((a >> 1) | 0x80) : (byte)(a >> 1)) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x10: /* RL B */
									temp = carryFlag ? (b << 1) | 0x01 : b << 1;
									carryFlag = (b & 0x80) != 0;
									zeroFlag = (b = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x11: /* RL C */
									temp = carryFlag ? (c << 1) | 0x01 : c << 1;
									carryFlag = (c & 0x80) != 0;
									zeroFlag = (c = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x12: /* RL D */
									temp = carryFlag ? (d << 1) | 0x01 : d << 1;
									carryFlag = (d & 0x80) != 0;
									zeroFlag = (d = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x13: /* RL E */
									temp = carryFlag ? (e << 1) | 0x01 : e << 1;
									carryFlag = (e & 0x80) != 0;
									zeroFlag = (e = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x14: /* RL H */
									temp = carryFlag ? (h << 1) | 0x01 : h << 1;
									carryFlag = (h & 0x80) != 0;
									zeroFlag = (h = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x15: /* RL L */
									temp = carryFlag ? (l << 1) | 0x01 : l << 1;
									carryFlag = (l & 0x80) != 0;
									zeroFlag = (l = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x16: /* RL (HL) */
									__temp8 = bus.ReadByte(l, h);
									temp = carryFlag ? (__temp8 << 1) | 0x01 : __temp8 << 1;
									carryFlag = (__temp8 & 0x80) != 0;
									zeroFlag = (__temp8 = (byte)temp) == 0;
									bus.WriteByte(l, h, __temp8);
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x17: /* RL A */
									temp = carryFlag ? (a << 1) | 0x01 : a << 1;
									carryFlag = (a & 0x80) != 0;
									zeroFlag = (a = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x18: /* RR B */
									temp = carryFlag ? (b >> 1) | 0x80 : b >> 1;
									carryFlag = (b & 0x01) != 0;
									zeroFlag = (b = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x19: /* RR C */
									temp = carryFlag ? (c >> 1) | 0x80 : c >> 1;
									carryFlag = (c & 0x01) != 0;
									zeroFlag = (c = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x1A: /* RR D */
									temp = carryFlag ? (d >> 1) | 0x80 : d >> 1;
									carryFlag = (d & 0x01) != 0;
									zeroFlag = (d = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x1B: /* RR E */
									temp = carryFlag ? (e >> 1) | 0x80 : e >> 1;
									carryFlag = (e & 0x01) != 0;
									zeroFlag = (e = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x1C: /* RR H */
									temp = carryFlag ? (h >> 1) | 0x80 : h >> 1;
									carryFlag = (h & 0x01) != 0;
									zeroFlag = (h = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x1D: /* RR L */
									temp = carryFlag ? (l >> 1) | 0x80 : l >> 1;
									carryFlag = (l & 0x01) != 0;
									zeroFlag = (l = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x1E: /* RR (HL) */
									__temp8 = bus.ReadByte(l, h);
									temp = carryFlag ? (__temp8 >> 1) | 0x80 : __temp8 >> 1;
									carryFlag = (__temp8 & 0x01) != 0;
									zeroFlag = (__temp8 = (byte)temp) == 0;
									bus.WriteByte(l, h, __temp8);
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x1F: /* RR A */
									temp = carryFlag ? (a >> 1) | 0x80 : a >> 1;
									carryFlag = (a & 0x01) != 0;
									zeroFlag = (a = (byte)temp) == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x20: /* SLA B */
									carryFlag = (b & 0x80) != 0;
									b = (byte)(b << 1);
									zeroFlag = b == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x21: /* SLA C */
									carryFlag = (c & 0x80) != 0;
									c = (byte)(c << 1);
									zeroFlag = c == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x22: /* SLA D */
									carryFlag = (d & 0x80) != 0;
									d = (byte)(d << 1);
									zeroFlag = d == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x23: /* SLA E */
									carryFlag = (e & 0x80) != 0;
									e = (byte)(e << 1);
									zeroFlag = e == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x24: /* SLA H */
									carryFlag = (h & 0x80) != 0;
									h = (byte)(h << 1);
									zeroFlag = h == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x25: /* SLA L */
									carryFlag = (l & 0x80) != 0;
									l = (byte)(l << 1);
									zeroFlag = l == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x26: /* SLA (HL) */
									__temp8 = bus.ReadByte(l, h);
									carryFlag = (__temp8 & 0x80) != 0;
									__temp8 = (byte)(__temp8 << 1);
									zeroFlag = __temp8 == 0;
									bus.WriteByte(l, h, __temp8);
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x27: /* SLA A */
									carryFlag = (a & 0x80) != 0;
									a = (byte)(a << 1);
									zeroFlag = a == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x28: /* SRA B */
									carryFlag = (b & 0x01) != 0;
									b = (byte)((sbyte)b >> 1);
									zeroFlag = b == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x29: /* SRA C */
									carryFlag = (c & 0x01) != 0;
									c = (byte)((sbyte)c >> 1);
									zeroFlag = c == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x2A: /* SRA D */
									carryFlag = (d & 0x01) != 0;
									d = (byte)((sbyte)d >> 1);
									zeroFlag = d == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x2B: /* SRA E */
									carryFlag = (e & 0x01) != 0;
									e = (byte)((sbyte)e >> 1);
									zeroFlag = e == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x2C: /* SRA H */
									carryFlag = (h & 0x01) != 0;
									h = (byte)((sbyte)h >> 1);
									zeroFlag = h == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x2D: /* SRA L */
									carryFlag = (l & 0x01) != 0;
									l = (byte)((sbyte)l >> 1);
									zeroFlag = l == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x2E: /* SRA (HL) */
									__temp8 = bus.ReadByte(l, h);
									carryFlag = (__temp8 & 0x01) != 0;
									__temp8 = (byte)((sbyte)__temp8 >> 1);
									zeroFlag = __temp8 == 0;
									bus.WriteByte(l, h, __temp8);
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x2F: /* SRA A */
									carryFlag = (a & 0x01) != 0;
									a = (byte)((sbyte)a >> 1);
									zeroFlag = a == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x30: /* SWAP B */
									b = (byte)((b >> 4) | (b << 4));
									zeroFlag = b == 0;
									negationFlag = false;
									halfCarryFlag = false;
									carryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x31: /* SWAP C */
									c = (byte)((c >> 4) | (c << 4));
									zeroFlag = c == 0;
									negationFlag = false;
									halfCarryFlag = false;
									carryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x32: /* SWAP D */
									d = (byte)((d >> 4) | (d << 4));
									zeroFlag = d == 0;
									negationFlag = false;
									halfCarryFlag = false;
									carryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x33: /* SWAP E */
									e = (byte)((e >> 4) | (e << 4));
									zeroFlag = e == 0;
									negationFlag = false;
									halfCarryFlag = false;
									carryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x34: /* SWAP H */
									h = (byte)((h >> 4) | (h << 4));
									zeroFlag = h == 0;
									negationFlag = false;
									halfCarryFlag = false;
									carryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x35: /* SWAP L */
									l = (byte)((l >> 4) | (l << 4));
									zeroFlag = l == 0;
									negationFlag = false;
									halfCarryFlag = false;
									carryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x36: /* SWAP (HL) */
									__temp8 = bus.ReadByte(l, h);
									__temp8 = (byte)((__temp8 >> 4) | (__temp8 << 4));
									zeroFlag = __temp8 == 0;
									bus.WriteByte(l, h, __temp8);
									negationFlag = false;
									halfCarryFlag = false;
									carryFlag = false;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x37: /* SWAP A */
									a = (byte)((a >> 4) | (a << 4));
									zeroFlag = a == 0;
									negationFlag = false;
									halfCarryFlag = false;
									carryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x38: /* SRL B */
									carryFlag = (b & 0x01) != 0;
									b = (byte)(b >> 1);
									zeroFlag = b == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x39: /* SRL C */
									carryFlag = (c & 0x01) != 0;
									c = (byte)(c >> 1);
									zeroFlag = c == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x3A: /* SRL D */
									carryFlag = (d & 0x01) != 0;
									d = (byte)(d >> 1);
									zeroFlag = d == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x3B: /* SRL E */
									carryFlag = (e & 0x01) != 0;
									e = (byte)(e >> 1);
									zeroFlag = e == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x3C: /* SRL H */
									carryFlag = (h & 0x01) != 0;
									h = (byte)(h >> 1);
									zeroFlag = h == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x3D: /* SRL L */
									carryFlag = (l & 0x01) != 0;
									l = (byte)(l >> 1);
									zeroFlag = l == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x3E: /* SRL (HL) */
									__temp8 = bus.ReadByte(l, h);
									carryFlag = (__temp8 & 0x01) != 0;
									__temp8 = (byte)(__temp8 >> 1);
									zeroFlag = __temp8 == 0;
									bus.WriteByte(l, h, __temp8);
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x3F: /* SRL A */
									carryFlag = (a & 0x01) != 0;
									a = (byte)(a >> 1);
									zeroFlag = a == 0;
									negationFlag = false;
									halfCarryFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x40: /* BIT 0,B */
									zeroFlag = (b & (1 << 0)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x41: /* BIT 0,C */
									zeroFlag = (c & (1 << 0)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x42: /* BIT 0,D */
									zeroFlag = (d & (1 << 0)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x43: /* BIT 0,E */
									zeroFlag = (e & (1 << 0)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x44: /* BIT 0,H */
									zeroFlag = (h & (1 << 0)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x45: /* BIT 0,L */
									zeroFlag = (l & (1 << 0)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x46: /* BIT 0,(HL) */
									__temp8 = bus.ReadByte(l, h);
									zeroFlag = (__temp8 & (1 << 0)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x47: /* BIT 0,A */
									zeroFlag = (a & (1 << 0)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x48: /* BIT 1,B */
									zeroFlag = (b & (1 << 1)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x49: /* BIT 1,C */
									zeroFlag = (c & (1 << 1)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x4A: /* BIT 1,D */
									zeroFlag = (d & (1 << 1)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x4B: /* BIT 1,E */
									zeroFlag = (e & (1 << 1)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x4C: /* BIT 1,H */
									zeroFlag = (h & (1 << 1)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x4D: /* BIT 1,L */
									zeroFlag = (l & (1 << 1)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x4E: /* BIT 1,(HL) */
									__temp8 = bus.ReadByte(l, h);
									zeroFlag = (__temp8 & (1 << 1)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x4F: /* BIT 1,A */
									zeroFlag = (a & (1 << 1)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x50: /* BIT 2,B */
									zeroFlag = (b & (1 << 2)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x51: /* BIT 2,C */
									zeroFlag = (c & (1 << 2)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x52: /* BIT 2,D */
									zeroFlag = (d & (1 << 2)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x53: /* BIT 2,E */
									zeroFlag = (e & (1 << 2)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x54: /* BIT 2,H */
									zeroFlag = (h & (1 << 2)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x55: /* BIT 2,L */
									zeroFlag = (l & (1 << 2)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x56: /* BIT 2,(HL) */
									__temp8 = bus.ReadByte(l, h);
									zeroFlag = (__temp8 & (1 << 2)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x57: /* BIT 2,A */
									zeroFlag = (a & (1 << 2)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x58: /* BIT 3,B */
									zeroFlag = (b & (1 << 3)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x59: /* BIT 3,C */
									zeroFlag = (c & (1 << 3)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x5A: /* BIT 3,D */
									zeroFlag = (d & (1 << 3)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x5B: /* BIT 3,E */
									zeroFlag = (e & (1 << 3)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x5C: /* BIT 3,H */
									zeroFlag = (h & (1 << 3)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x5D: /* BIT 3,L */
									zeroFlag = (l & (1 << 3)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x5E: /* BIT 3,(HL) */
									__temp8 = bus.ReadByte(l, h);
									zeroFlag = (__temp8 & (1 << 3)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x5F: /* BIT 3,A */
									zeroFlag = (a & (1 << 3)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x60: /* BIT 4,B */
									zeroFlag = (b & (1 << 4)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x61: /* BIT 4,C */
									zeroFlag = (c & (1 << 4)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x62: /* BIT 4,D */
									zeroFlag = (d & (1 << 4)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x63: /* BIT 4,E */
									zeroFlag = (e & (1 << 4)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x64: /* BIT 4,H */
									zeroFlag = (h & (1 << 4)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x65: /* BIT 4,L */
									zeroFlag = (l & (1 << 4)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x66: /* BIT 4,(HL) */
									__temp8 = bus.ReadByte(l, h);
									zeroFlag = (__temp8 & (1 << 4)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x67: /* BIT 4,A */
									zeroFlag = (a & (1 << 4)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x68: /* BIT 5,B */
									zeroFlag = (b & (1 << 5)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x69: /* BIT 5,C */
									zeroFlag = (c & (1 << 5)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x6A: /* BIT 5,D */
									zeroFlag = (d & (1 << 5)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x6B: /* BIT 5,E */
									zeroFlag = (e & (1 << 5)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x6C: /* BIT 5,H */
									zeroFlag = (h & (1 << 5)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x6D: /* BIT 5,L */
									zeroFlag = (l & (1 << 5)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x6E: /* BIT 5,(HL) */
									__temp8 = bus.ReadByte(l, h);
									zeroFlag = (__temp8 & (1 << 5)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x6F: /* BIT 5,A */
									zeroFlag = (a & (1 << 5)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x70: /* BIT 6,B */
									zeroFlag = (b & (1 << 6)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x71: /* BIT 6,C */
									zeroFlag = (c & (1 << 6)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x72: /* BIT 6,D */
									zeroFlag = (d & (1 << 6)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x73: /* BIT 6,E */
									zeroFlag = (e & (1 << 6)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x74: /* BIT 6,H */
									zeroFlag = (h & (1 << 6)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x75: /* BIT 6,L */
									zeroFlag = (l & (1 << 6)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x76: /* BIT 6,(HL) */
									__temp8 = bus.ReadByte(l, h);
									zeroFlag = (__temp8 & (1 << 6)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x77: /* BIT 6,A */
									zeroFlag = (a & (1 << 6)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x78: /* BIT 7,B */
									zeroFlag = (b & (1 << 7)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x79: /* BIT 7,C */
									zeroFlag = (c & (1 << 7)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x7A: /* BIT 7,D */
									zeroFlag = (d & (1 << 7)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x7B: /* BIT 7,E */
									zeroFlag = (e & (1 << 7)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x7C: /* BIT 7,H */
									zeroFlag = (h & (1 << 7)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x7D: /* BIT 7,L */
									zeroFlag = (l & (1 << 7)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x7E: /* BIT 7,(HL) */
									__temp8 = bus.ReadByte(l, h);
									zeroFlag = (__temp8 & (1 << 7)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x7F: /* BIT 7,A */
									zeroFlag = (a & (1 << 7)) == 0;
									halfCarryFlag = true;
									negationFlag = false;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x80: /* RES 0,B */
									unchecked { b &= (byte)(~(1 << 0)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x81: /* RES 0,C */
									unchecked { c &= (byte)(~(1 << 0)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x82: /* RES 0,D */
									unchecked { d &= (byte)(~(1 << 0)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x83: /* RES 0,E */
									unchecked { e &= (byte)(~(1 << 0)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x84: /* RES 0,H */
									unchecked { h &= (byte)(~(1 << 0)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x85: /* RES 0,L */
									unchecked { l &= (byte)(~(1 << 0)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x86: /* RES 0,(HL) */
									__temp8 = bus.ReadByte(l, h);
									unchecked { __temp8 &= (byte)(~(1 << 0)); };
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x87: /* RES 0,A */
									unchecked { a &= (byte)(~(1 << 0)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x88: /* RES 1,B */
									unchecked { b &= (byte)(~(1 << 1)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x89: /* RES 1,C */
									unchecked { c &= (byte)(~(1 << 1)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x8A: /* RES 1,D */
									unchecked { d &= (byte)(~(1 << 1)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x8B: /* RES 1,E */
									unchecked { e &= (byte)(~(1 << 1)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x8C: /* RES 1,H */
									unchecked { h &= (byte)(~(1 << 1)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x8D: /* RES 1,L */
									unchecked { l &= (byte)(~(1 << 1)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x8E: /* RES 1,(HL) */
									__temp8 = bus.ReadByte(l, h);
									unchecked { __temp8 &= (byte)(~(1 << 1)); };
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x8F: /* RES 1,A */
									unchecked { a &= (byte)(~(1 << 1)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x90: /* RES 2,B */
									unchecked { b &= (byte)(~(1 << 2)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x91: /* RES 2,C */
									unchecked { c &= (byte)(~(1 << 2)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x92: /* RES 2,D */
									unchecked { d &= (byte)(~(1 << 2)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x93: /* RES 2,E */
									unchecked { e &= (byte)(~(1 << 2)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x94: /* RES 2,H */
									unchecked { h &= (byte)(~(1 << 2)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x95: /* RES 2,L */
									unchecked { l &= (byte)(~(1 << 2)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x96: /* RES 2,(HL) */
									__temp8 = bus.ReadByte(l, h);
									unchecked { __temp8 &= (byte)(~(1 << 2)); };
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x97: /* RES 2,A */
									unchecked { a &= (byte)(~(1 << 2)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x98: /* RES 3,B */
									unchecked { b &= (byte)(~(1 << 3)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x99: /* RES 3,C */
									unchecked { c &= (byte)(~(1 << 3)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x9A: /* RES 3,D */
									unchecked { d &= (byte)(~(1 << 3)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x9B: /* RES 3,E */
									unchecked { e &= (byte)(~(1 << 3)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x9C: /* RES 3,H */
									unchecked { h &= (byte)(~(1 << 3)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x9D: /* RES 3,L */
									unchecked { l &= (byte)(~(1 << 3)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x9E: /* RES 3,(HL) */
									__temp8 = bus.ReadByte(l, h);
									unchecked { __temp8 &= (byte)(~(1 << 3)); };
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x9F: /* RES 3,A */
									unchecked { a &= (byte)(~(1 << 3)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA0: /* RES 4,B */
									unchecked { b &= (byte)(~(1 << 4)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA1: /* RES 4,C */
									unchecked { c &= (byte)(~(1 << 4)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA2: /* RES 4,D */
									unchecked { d &= (byte)(~(1 << 4)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA3: /* RES 4,E */
									unchecked { e &= (byte)(~(1 << 4)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA4: /* RES 4,H */
									unchecked { h &= (byte)(~(1 << 4)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA5: /* RES 4,L */
									unchecked { l &= (byte)(~(1 << 4)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA6: /* RES 4,(HL) */
									__temp8 = bus.ReadByte(l, h);
									unchecked { __temp8 &= (byte)(~(1 << 4)); };
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xA7: /* RES 4,A */
									unchecked { a &= (byte)(~(1 << 4)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA8: /* RES 5,B */
									unchecked { b &= (byte)(~(1 << 5)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA9: /* RES 5,C */
									unchecked { c &= (byte)(~(1 << 5)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xAA: /* RES 5,D */
									unchecked { d &= (byte)(~(1 << 5)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xAB: /* RES 5,E */
									unchecked { e &= (byte)(~(1 << 5)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xAC: /* RES 5,H */
									unchecked { h &= (byte)(~(1 << 5)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xAD: /* RES 5,L */
									unchecked { l &= (byte)(~(1 << 5)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xAE: /* RES 5,(HL) */
									__temp8 = bus.ReadByte(l, h);
									unchecked { __temp8 &= (byte)(~(1 << 5)); };
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xAF: /* RES 5,A */
									unchecked { a &= (byte)(~(1 << 5)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB0: /* RES 6,B */
									unchecked { b &= (byte)(~(1 << 6)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB1: /* RES 6,C */
									unchecked { c &= (byte)(~(1 << 6)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB2: /* RES 6,D */
									unchecked { d &= (byte)(~(1 << 6)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB3: /* RES 6,E */
									unchecked { e &= (byte)(~(1 << 6)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB4: /* RES 6,H */
									unchecked { h &= (byte)(~(1 << 6)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB5: /* RES 6,L */
									unchecked { l &= (byte)(~(1 << 6)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB6: /* RES 6,(HL) */
									__temp8 = bus.ReadByte(l, h);
									unchecked { __temp8 &= (byte)(~(1 << 6)); };
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xB7: /* RES 6,A */
									unchecked { a &= (byte)(~(1 << 6)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB8: /* RES 7,B */
									unchecked { b &= (byte)(~(1 << 7)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB9: /* RES 7,C */
									unchecked { c &= (byte)(~(1 << 7)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xBA: /* RES 7,D */
									unchecked { d &= (byte)(~(1 << 7)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xBB: /* RES 7,E */
									unchecked { e &= (byte)(~(1 << 7)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xBC: /* RES 7,H */
									unchecked { h &= (byte)(~(1 << 7)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xBD: /* RES 7,L */
									unchecked { l &= (byte)(~(1 << 7)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xBE: /* RES 7,(HL) */
									__temp8 = bus.ReadByte(l, h);
									unchecked { __temp8 &= (byte)(~(1 << 7)); };
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xBF: /* RES 7,A */
									unchecked { a &= (byte)(~(1 << 7)); };
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC0: /* SET 0,B */
									b |= (byte)(1 << 0);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC1: /* SET 0,C */
									c |= (byte)(1 << 0);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC2: /* SET 0,D */
									d |= (byte)(1 << 0);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC3: /* SET 0,E */
									e |= (byte)(1 << 0);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC4: /* SET 0,H */
									h |= (byte)(1 << 0);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC5: /* SET 0,L */
									l |= (byte)(1 << 0);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC6: /* SET 0,(HL) */
									__temp8 = bus.ReadByte(l, h);
									__temp8 |= (byte)(1 << 0);
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xC7: /* SET 0,A */
									a |= (byte)(1 << 0);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC8: /* SET 1,B */
									b |= (byte)(1 << 1);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC9: /* SET 1,C */
									c |= (byte)(1 << 1);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xCA: /* SET 1,D */
									d |= (byte)(1 << 1);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xCB: /* SET 1,E */
									e |= (byte)(1 << 1);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xCC: /* SET 1,H */
									h |= (byte)(1 << 1);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xCD: /* SET 1,L */
									l |= (byte)(1 << 1);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xCE: /* SET 1,(HL) */
									__temp8 = bus.ReadByte(l, h);
									__temp8 |= (byte)(1 << 1);
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xCF: /* SET 1,A */
									a |= (byte)(1 << 1);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD0: /* SET 2,B */
									b |= (byte)(1 << 2);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD1: /* SET 2,C */
									c |= (byte)(1 << 2);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD2: /* SET 2,D */
									d |= (byte)(1 << 2);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD3: /* SET 2,E */
									e |= (byte)(1 << 2);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD4: /* SET 2,H */
									h |= (byte)(1 << 2);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD5: /* SET 2,L */
									l |= (byte)(1 << 2);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD6: /* SET 2,(HL) */
									__temp8 = bus.ReadByte(l, h);
									__temp8 |= (byte)(1 << 2);
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xD7: /* SET 2,A */
									a |= (byte)(1 << 2);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD8: /* SET 3,B */
									b |= (byte)(1 << 3);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD9: /* SET 3,C */
									c |= (byte)(1 << 3);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xDA: /* SET 3,D */
									d |= (byte)(1 << 3);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xDB: /* SET 3,E */
									e |= (byte)(1 << 3);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xDC: /* SET 3,H */
									h |= (byte)(1 << 3);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xDD: /* SET 3,L */
									l |= (byte)(1 << 3);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xDE: /* SET 3,(HL) */
									__temp8 = bus.ReadByte(l, h);
									__temp8 |= (byte)(1 << 3);
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xDF: /* SET 3,A */
									a |= (byte)(1 << 3);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE0: /* SET 4,B */
									b |= (byte)(1 << 4);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE1: /* SET 4,C */
									c |= (byte)(1 << 4);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE2: /* SET 4,D */
									d |= (byte)(1 << 4);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE3: /* SET 4,E */
									e |= (byte)(1 << 4);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE4: /* SET 4,H */
									h |= (byte)(1 << 4);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE5: /* SET 4,L */
									l |= (byte)(1 << 4);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE6: /* SET 4,(HL) */
									__temp8 = bus.ReadByte(l, h);
									__temp8 |= (byte)(1 << 4);
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xE7: /* SET 4,A */
									a |= (byte)(1 << 4);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE8: /* SET 5,B */
									b |= (byte)(1 << 5);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE9: /* SET 5,C */
									c |= (byte)(1 << 5);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xEA: /* SET 5,D */
									d |= (byte)(1 << 5);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xEB: /* SET 5,E */
									e |= (byte)(1 << 5);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xEC: /* SET 5,H */
									h |= (byte)(1 << 5);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xED: /* SET 5,L */
									l |= (byte)(1 << 5);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xEE: /* SET 5,(HL) */
									__temp8 = bus.ReadByte(l, h);
									__temp8 |= (byte)(1 << 5);
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xEF: /* SET 5,A */
									a |= (byte)(1 << 5);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF0: /* SET 6,B */
									b |= (byte)(1 << 6);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF1: /* SET 6,C */
									c |= (byte)(1 << 6);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF2: /* SET 6,D */
									d |= (byte)(1 << 6);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF3: /* SET 6,E */
									e |= (byte)(1 << 6);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF4: /* SET 6,H */
									h |= (byte)(1 << 6);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF5: /* SET 6,L */
									l |= (byte)(1 << 6);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF6: /* SET 6,(HL) */
									__temp8 = bus.ReadByte(l, h);
									__temp8 |= (byte)(1 << 6);
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xF7: /* SET 6,A */
									a |= (byte)(1 << 6);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF8: /* SET 7,B */
									b |= (byte)(1 << 7);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF9: /* SET 7,C */
									c |= (byte)(1 << 7);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xFA: /* SET 7,D */
									d |= (byte)(1 << 7);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xFB: /* SET 7,E */
									e |= (byte)(1 << 7);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xFC: /* SET 7,H */
									h |= (byte)(1 << 7);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xFD: /* SET 7,L */
									l |= (byte)(1 << 7);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xFE: /* SET 7,(HL) */
									__temp8 = bus.ReadByte(l, h);
									__temp8 |= (byte)(1 << 7);
									bus.WriteByte(l, h, __temp8);
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xFF: /* SET 7,A */
									a |= (byte)(1 << 7);
									cycleCount = 8;
									break;
							}
							break;
						case 0xCC: /* CALL Z,N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							if (zeroFlag)
							{
								bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = __temp16;
								cycleCount = 24;
							}
							else cycleCount = 12;
							break;
						case 0xCD: /* CALL N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = __temp16;
							cycleCount = 24;
							break;
						case 0xCE: /* ADC A,N */
							__temp8 = bus.ReadByte(pc++);
							if (carryFlag)
							{
								temp = a + __temp8 + 1;
								halfCarryFlag = (a & 0xF) + (__temp8 & 0xF) > 0xE;
							}
							else
							{
								temp = a + __temp8;
								halfCarryFlag = (a & 0xF) + (__temp8 & 0xF) > 0xF;
							}
							carryFlag = temp > 0xFF;
							a = (byte)temp;
							zeroFlag = a == 0;
							negationFlag = false;
							cycleCount = 8;
							break;
						case 0xCF: /* RST $08 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 8;
							cycleCount = 16;
							break;
						case 0xD0: /* RET NC */
							if (!carryFlag)
							{
								pc = (ushort)(bus[sp++] | (bus[sp++] << 8));
								cycleCount = 20;
							}
							else cycleCount = 8;
							break;
						case 0xD1: /* POP DE */
							__temp16 = (ushort)(bus[sp++] | (bus[sp++] << 8));
							d = (byte)(__temp16 >> 8); e = (byte)(__temp16);
							cycleCount = 12;
							break;
						case 0xD2: /* JP NC,N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							if (!carryFlag)
							{
								pc = __temp16;
								cycleCount = 16;
							}
							else cycleCount = 12;
							break;
						case 0xD4: /* CALL NC,N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							if (!carryFlag)
							{
								bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = __temp16;
								cycleCount = 24;
							}
							else cycleCount = 12;
							break;
						case 0xD5: /* PUSH DE */
							__temp16 = (ushort)((d << 8) | e);
							bus[--sp] = (byte)(__temp16 >> 8);
							bus[--sp] = (byte)__temp16;
							cycleCount = 16;
							break;
						case 0xD6: /* SUB A,N */
							__temp8 = bus.ReadByte(pc++);
							halfCarryFlag = (a & 0xF) < (__temp8 & 0xF);
							carryFlag = a < __temp8;
							zeroFlag = (a -= __temp8) == 0;
							negationFlag = true;
							cycleCount = 8;
							break;
						case 0xD7: /* RST $10 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 16;
							cycleCount = 16;
							break;
						case 0xD8: /* RET C */
							if (carryFlag)
							{
								pc = (ushort)(bus[sp++] | (bus[sp++] << 8));
								cycleCount = 20;
							}
							else cycleCount = 8;
							break;
						case 0xD9: /* RETI */
							pc = (ushort)(bus[sp++] | (bus[sp++] << 8));
							ime = true;
							cycleCount = 16;
							break;
						case 0xDA: /* JP C,N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							if (carryFlag)
							{
								pc = __temp16;
								cycleCount = 16;
							}
							else cycleCount = 12;
							break;
						case 0xDC: /* CALL C,N */
							__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));
							if (carryFlag)
							{
								bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = __temp16;
								cycleCount = 24;
							}
							else cycleCount = 12;
							break;
						case 0xDE: /* SBC A,N */
							__temp8 = bus.ReadByte(pc++);
							if (carryFlag)
							{
								halfCarryFlag = (a & 0xF) - (__temp8 & 0xF) < 1;
								carryFlag = a - __temp8 < 1;
								zeroFlag = (a = (byte)(a - __temp8 - 1)) == 0;
							}
							else
							{
								halfCarryFlag = (a & 0xF) < (__temp8 & 0xF);
								carryFlag = a < __temp8;
								zeroFlag = (a -= __temp8) == 0;
							}
							negationFlag = true;
							cycleCount = 8;
							break;
						case 0xDF: /* RST $18 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 24;
							cycleCount = 16;
							break;
						case 0xE0: /* LD ($FF00+N),A */
							__temp8 = a;
							bus.WritePort(bus.ReadByte(pc++), __temp8);
							cycleCount = 12;
							break;
						case 0xE1: /* POP HL */
							__tempHL = (ushort)(bus[sp++] | (bus[sp++] << 8));
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							cycleCount = 12;
							break;
						case 0xE2: /* LD (C),A */
							__temp8 = a;
							bus.WritePort(c, __temp8);
							cycleCount = 8;
							break;
						case 0xE5: /* PUSH HL */
							__tempHL = (ushort)((h << 8) | l);
							bus[--sp] = (byte)(__tempHL >> 8);
							bus[--sp] = (byte)__tempHL;
							cycleCount = 16;
							break;
						case 0xE6: /* AND A,N */
							__temp8 = bus.ReadByte(pc++);
							a &= __temp8;
							zeroFlag = a == 0;
							halfCarryFlag = true;
							negationFlag = false;
							carryFlag = false;
							cycleCount = 8;
							break;
						case 0xE7: /* RST $20 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 32;
							cycleCount = 16;
							break;
						case 0xE8: /* ADD SP,N */
							__temp16 = (ushort)(sbyte)bus.ReadByte(pc++);
							temp = sp + __temp16;
							halfCarryFlag = (sp & 0xFFF) + (__temp16 & 0xFFF) > 0xFFF;
							carryFlag = temp > 0xFFFF;
							sp = (ushort)temp;
							zeroFlag = false;
							negationFlag = false;
							cycleCount = 16;
							break;
						case 0xE9: /* JP HL */
							__tempHL = (ushort)((h << 8) | l);
							pc = __tempHL;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							cycleCount = 4;
							break;
						case 0xEA: /* LD (N),A */
							__temp8 = a;
							bus.WriteByte(bus.ReadByte(pc++), bus.ReadByte(pc++), __temp8);
							cycleCount = 16;
							break;
						case 0xEE: /* XOR A,N */
							__temp8 = bus.ReadByte(pc++);
							a ^= __temp8;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 8;
							break;
						case 0xEF: /* RST $28 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 40;
							cycleCount = 16;
							break;
						case 0xF0: /* LD A,($FF00+N) */
							__temp8 = bus.ReadPort(bus.ReadByte(pc++));
							a = __temp8;
							cycleCount = 12;
							break;
						case 0xF1: /* POP AF */
							__temp16 = (ushort)(bus[sp++] | (bus[sp++] << 8));
							a = (byte)(__temp16 >> 8); zeroFlag = (__temp16 & 0x80) != 0; negationFlag = (__temp16 & 0x40) != 0; halfCarryFlag = (__temp16 & 0x20) != 0; carryFlag = (__temp16 & 0x10) != 0;
							cycleCount = 12;
							break;
						case 0xF2: /* LD A,(C) */
							__temp8 = bus.ReadPort(c);
							a = __temp8;
							cycleCount = 8;
							break;
						case 0xF3: /* DI */
							ime = false;
							cycleCount = 4;
							break;
						case 0xF5: /* PUSH AF */
							__temp16 = (ushort)((a << 8) | (zeroFlag ? 0x80 : 0) | (negationFlag ? 0x40 : 0) | (halfCarryFlag ? 0x20 : 0) | (carryFlag ? 0x10 : 0));
							bus[--sp] = (byte)(__temp16 >> 8);
							bus[--sp] = (byte)__temp16;
							cycleCount = 16;
							break;
						case 0xF6: /* OR A,N */
							__temp8 = bus.ReadByte(pc++);
							a |= __temp8;
							zeroFlag = a == 0;
							negationFlag = false;
							halfCarryFlag = false;
							carryFlag = false;
							cycleCount = 8;
							break;
						case 0xF7: /* RST $30 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 48;
							cycleCount = 16;
							break;
						case 0xF8: /* LD HL,SP+N */
							temp = (short)(sbyte)bus[pc++] & 0xFFFF; // Sign extension to 16 bits
							halfCarryFlag = (temp & 0xFFF) + (sp & 0xFFF) > 0xFFF;
							temp += sp;
							carryFlag = temp > 0xFFFF;
							__tempHL = (ushort)temp;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							zeroFlag = false;
							negationFlag = false;
							cycleCount = 12;
							break;
						case 0xF9: /* LD SP,HL */
							__tempHL = (ushort)((h << 8) | l);
							sp = __tempHL;
							cycleCount = 8;
							break;
						case 0xFA: /* LD A,(N) */
							__temp8 = bus.ReadByte(bus.ReadByte(pc++), bus.ReadByte(pc++));
							a = __temp8;
							cycleCount = 16;
							break;
						case 0xFB: /* EI */
							// Will enable interrupts one instruction later, or directly after this one if EI has been repeated.
							if (enableInterruptDelay == 0) enableInterruptDelay = 2;
							cycleCount = 4;
							break;
						case 0xFE: /* CP A,N */
							__temp8 = bus.ReadByte(pc++);
							halfCarryFlag = (a & 0xF) < (__temp8 & 0xF);
							carryFlag = a < __temp8;
							zeroFlag = a == __temp8;
							negationFlag = true;
							cycleCount = 8;
							break;
						case 0xFF: /* RST $38 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 56;
							cycleCount = 16;
							break;
						/* Invalid Opcodes */
						case 0xD3:
						case 0xDB:
						case 0xDD:
						case 0xE3:
						case 0xE4:
						case 0xEB:
						case 0xEC:
						case 0xED:
						case 0xF4:
						case 0xFC:
						case 0xFD:
							status = ProcessorStatus.Crashed;
							pc--; // Revert changes to PC
							return true;
					}

					if (enableInterruptDelay != 0 && --enableInterruptDelay == 0) ime = true;
					
				HandleBreakpoints:
#if WITH_DEBUGGING
					// Handle breakpoints after running at least one instruction
					if (bus.BreakpointCount > 0) // Check for breakpoints only if there are some
						if (bus.IsBreakPoint(pc))
							return false; // Break when a breakpoint is encountered
#else
					;
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
