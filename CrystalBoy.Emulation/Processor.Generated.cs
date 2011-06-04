﻿#region Copyright Notice
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
			byte a, f, b, c, d, e, h, l, opcode;
			ushort sp, pc;
			bool ime;

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
			a = A; f = F; b = B; c = C; d = D; e = E; h = H; l = L;
			sp = SP; pc = PC;

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
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							b = (byte)(__temp16 >> 8); c = (byte)(__temp16);
							cycleCount = 12;
							break;
						case 0x02: /* LD (BC),A */
							__temp8 = a;
							bus[c, b] = __temp8;
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
							f = (byte)(f & CFlag | (b != 0 ? (b & 0xF) == 0 ? HFlag : 0 : ZFlag | HFlag));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x05: /* DEC B */
							b--;
							f = (byte)(f & CFlag | (b != 0 ? (b & 0xF) != 0xF ? 0 : HFlag : ZFlag) | NFlag);
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x06: /* LD B,N */
							__temp8 = bus[pc++];
							b = __temp8;
							cycleCount = 8;
							break;
						case 0x07: /* RLCA */
							if ((a & 0x80) != 0)
							{
								f = CFlag;
								a = (byte)((a << 1) | 0x01);
							}
							else
							{
								f = 0;
								a <<= 1;
							}
							f &= 0x10;
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
							f = (byte)(f & 0x80 | ((__tempHL & 0xFFF) + (__temp16 & 0xFFF) > 0xFFF ? HFlag : 0) | (temp > 0xFFFF ? CFlag : 0));
							__tempHL = (ushort)temp;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							f &= 0xB0;
							cycleCount = 8;
							break;
						case 0x0A: /* LD A,(BC) */
							__temp8 = bus[c, b];
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
							f = (byte)(f & CFlag | (c != 0 ? (c & 0xF) == 0 ? HFlag : 0 : ZFlag | HFlag));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x0D: /* DEC C */
							c--;
							f = (byte)(f & CFlag | (c != 0 ? (c & 0xF) != 0xF ? 0 : HFlag : ZFlag) | NFlag);
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x0E: /* LD C,N */
							__temp8 = bus[pc++];
							c = __temp8;
							cycleCount = 8;
							break;
						case 0x0F: /* RRCA */
							if ((a & 0x01) != 0)
							{
								f = CFlag;
								a = (byte)((a >> 1) | 0x80);
							}
							else
							{
								f = 0;
								a >>= 1;
							}
							f &= 0x10;
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
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							d = (byte)(__temp16 >> 8); e = (byte)(__temp16);
							cycleCount = 12;
							break;
						case 0x12: /* LD (DE),A */
							__temp8 = a;
							bus[e, d] = __temp8;
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
							f = (byte)(f & CFlag | (d != 0 ? (d & 0xF) == 0 ? HFlag : 0 : ZFlag | HFlag));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x15: /* DEC D */
							d--;
							f = (byte)(f & CFlag | (d != 0 ? (d & 0xF) != 0xF ? 0 : HFlag : ZFlag) | NFlag);
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x16: /* LD D,N */
							__temp8 = bus[pc++];
							d = __temp8;
							cycleCount = 8;
							break;
						case 0x17: /* RLA */
							if ((f & CFlag) != 0)
							{
								f = (byte)((a & 0x80) != 0 ? CFlag : 0);
								a = (byte)((a << 1) | 0x01);
							}
							else
							{
								f = (byte)((a & 0x80) != 0 ? CFlag : 0);
								a <<= 1;
							}
							f &= 0x10;
							cycleCount = 4;
							break;
						case 0x18: /* JR N */
							__temp16 = (ushort)(sbyte)bus[pc++];
							pc += __temp16;
							cycleCount = 12;
							break;
						case 0x19: /* ADD HL,DE */
							__tempHL = (ushort)((h << 8) | l);
							__temp16 = (ushort)((d << 8) | e);
							temp = __tempHL + __temp16;
							f = (byte)(f & 0x80 | ((__tempHL & 0xFFF) + (__temp16 & 0xFFF) > 0xFFF ? HFlag : 0) | (temp > 0xFFFF ? CFlag : 0));
							__tempHL = (ushort)temp;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							f &= 0xB0;
							cycleCount = 8;
							break;
						case 0x1A: /* LD A,(DE) */
							__temp8 = bus[e, d];
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
							f = (byte)(f & CFlag | (e != 0 ? (e & 0xF) == 0 ? HFlag : 0 : ZFlag | HFlag));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x1D: /* DEC E */
							e--;
							f = (byte)(f & CFlag | (e != 0 ? (e & 0xF) != 0xF ? 0 : HFlag : ZFlag) | NFlag);
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x1E: /* LD E,N */
							__temp8 = bus[pc++];
							e = __temp8;
							cycleCount = 8;
							break;
						case 0x1F: /* RRA */
							if ((f & CFlag) != 0)
							{
								f = (byte)((a & 0x01) != 0 ? CFlag : 0);
								a = (byte)((a >> 1) | 0x80);
							}
							else
							{
								f = (byte)((a & 0x01) != 0 ? CFlag : 0);
								a >>= 1;
							}
							f &= 0x10;
							cycleCount = 4;
							break;
						case 0x20: /* JR NZ,N */
							__temp16 = (ushort)(sbyte)bus[pc++];
							if (((f & ZFlag) == 0))
							{
								pc += __temp16;
								cycleCount = 12;
							}
							else cycleCount = 8;
							break;
						case 0x21: /* LD HL,N */
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							__tempHL = __temp16;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							cycleCount = 12;
							break;
						case 0x22: /* LDI (HL),A */
							__temp8 = a;
							bus[l, h] = __temp8;
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
							f = (byte)(f & CFlag | (h != 0 ? (h & 0xF) == 0 ? HFlag : 0 : ZFlag | HFlag));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x25: /* DEC H */
							h--;
							f = (byte)(f & CFlag | (h != 0 ? (h & 0xF) != 0xF ? 0 : HFlag : ZFlag) | NFlag);
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x26: /* LD H,N */
							__temp8 = bus[pc++];
							h = __temp8;
							cycleCount = 8;
							break;
						case 0x27: /* DAA */
							if ((f & NFlag) != 0)
							{
								if ((f & HFlag) != 0) a -= 0x06;
								if ((f & CFlag) != 0) a -= 0x60;
							}
							else
							{
								if ((f & CFlag) != 0 || a > 0x99)
								{
									a += (f & HFlag) != 0 || (a & 0x0F) > 0x09 ? (byte)0x66 : (byte)0x60;
									f |= CFlag;
								}
								else if ((f & HFlag) != 0 || (a & 0x0F) > 0x09) a += 0x06;
							}
							f = (byte)(a == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0xD0;
							cycleCount = 4;
							break;
						case 0x28: /* JR Z,N */
							__temp16 = (ushort)(sbyte)bus[pc++];
							if (((f & ZFlag) != 0))
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
							f = (byte)(f & 0x80 | ((__tempHL & 0xFFF) + (__tempHL & 0xFFF) > 0xFFF ? HFlag : 0) | (temp > 0xFFFF ? CFlag : 0));
							__tempHL = (ushort)temp;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							f &= 0xB0;
							cycleCount = 8;
							break;
						case 0x2A: /* LDI A,(HL) */
							__temp8 = bus[l, h];
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
							f = (byte)(f & CFlag | (l != 0 ? (l & 0xF) == 0 ? HFlag : 0 : ZFlag | HFlag));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x2D: /* DEC L */
							l--;
							f = (byte)(f & CFlag | (l != 0 ? (l & 0xF) != 0xF ? 0 : HFlag : ZFlag) | NFlag);
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x2E: /* LD L,N */
							__temp8 = bus[pc++];
							l = __temp8;
							cycleCount = 8;
							break;
						case 0x2F: /* CPL */
							a = (byte)~a;
							f |= 0x60;
							cycleCount = 4;
							break;
						case 0x30: /* JR NC,N */
							__temp16 = (ushort)(sbyte)bus[pc++];
							if (((f & CFlag) == 0))
							{
								pc += __temp16;
								cycleCount = 12;
							}
							else cycleCount = 8;
							break;
						case 0x31: /* LD SP,N */
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							sp = __temp16;
							cycleCount = 12;
							break;
						case 0x32: /* LDD (HL),A */
							__temp8 = a;
							bus[l, h] = __temp8;
							if (l-- == 0) h--;
							cycleCount = 8;
							break;
						case 0x33: /* INC SP */
							sp++;
							cycleCount = 8;
							break;
						case 0x34: /* INC (HL) */
							__temp8 = bus[l, h];
							__temp8++;
							f = (byte)(f & CFlag | (__temp8 != 0 ? (__temp8 & 0xF) == 0 ? HFlag : 0 : ZFlag | HFlag));
							bus[l, h] = __temp8;
							f &= 0xB0;
							cycleCount = 12;
							break;
						case 0x35: /* DEC (HL) */
							__temp8 = bus[l, h];
							__temp8--;
							f = (byte)(f & CFlag | (__temp8 != 0 ? (__temp8 & 0xF) != 0xF ? 0 : HFlag : ZFlag) | NFlag);
							bus[l, h] = __temp8;
							f |= 0x40;
							cycleCount = 12;
							break;
						case 0x36: /* LD (HL),N */
							__temp8 = bus[pc++];
							bus[l, h] = __temp8;
							cycleCount = 12;
							break;
						case 0x37: /* SCF */
							f = (byte)(f & 0x90 | 0x10);
							cycleCount = 4;
							break;
						case 0x38: /* JR C,N */
							__temp16 = (ushort)(sbyte)bus[pc++];
							if (((f & CFlag) != 0))
							{
								pc += __temp16;
								cycleCount = 12;
							}
							else cycleCount = 8;
							break;
						case 0x39: /* ADD HL,SP */
							__tempHL = (ushort)((h << 8) | l);
							temp = __tempHL + sp;
							f = (byte)(f & 0x80 | ((__tempHL & 0xFFF) + (sp & 0xFFF) > 0xFFF ? HFlag : 0) | (temp > 0xFFFF ? CFlag : 0));
							__tempHL = (ushort)temp;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							f &= 0xB0;
							cycleCount = 8;
							break;
						case 0x3A: /* LDD A,(HL) */
							__temp8 = bus[l, h];
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
							f = (byte)(f & CFlag | (a != 0 ? (a & 0xF) == 0 ? HFlag : 0 : ZFlag | HFlag));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x3D: /* DEC A */
							a--;
							f = (byte)(f & CFlag | (a != 0 ? (a & 0xF) != 0xF ? 0 : HFlag : ZFlag) | NFlag);
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x3E: /* LD A,N */
							__temp8 = bus[pc++];
							a = __temp8;
							cycleCount = 8;
							break;
						case 0x3F: /* CCF */
							f ^= CFlag;
							f &= 0x90;
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
							__temp8 = bus[l, h];
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
							__temp8 = bus[l, h];
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
							__temp8 = bus[l, h];
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
							__temp8 = bus[l, h];
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
							__temp8 = bus[l, h];
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
							__temp8 = bus[l, h];
							l = __temp8;
							cycleCount = 8;
							break;
						case 0x6F: /* LD L,A */
							l = a;
							cycleCount = 4;
							break;
						case 0x70: /* LD (HL),B */
							__temp8 = b;
							bus[l, h] = __temp8;
							cycleCount = 8;
							break;
						case 0x71: /* LD (HL),C */
							__temp8 = c;
							bus[l, h] = __temp8;
							cycleCount = 8;
							break;
						case 0x72: /* LD (HL),D */
							__temp8 = d;
							bus[l, h] = __temp8;
							cycleCount = 8;
							break;
						case 0x73: /* LD (HL),E */
							__temp8 = e;
							bus[l, h] = __temp8;
							cycleCount = 8;
							break;
						case 0x74: /* LD (HL),H */
							__temp8 = h;
							bus[l, h] = __temp8;
							cycleCount = 8;
							break;
						case 0x75: /* LD (HL),L */
							__temp8 = l;
							bus[l, h] = __temp8;
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
							bus[l, h] = __temp8;
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
							__temp8 = bus[l, h];
							a = __temp8;
							cycleCount = 8;
							break;
						case 0x7F: /* LD A,A */
							cycleCount = 4;
							break;
						case 0x80: /* ADD A,B */
							temp = a + b;
							f = (byte)(((a & 0xF) + (b & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x81: /* ADD A,C */
							temp = a + c;
							f = (byte)(((a & 0xF) + (c & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x82: /* ADD A,D */
							temp = a + d;
							f = (byte)(((a & 0xF) + (d & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x83: /* ADD A,E */
							temp = a + e;
							f = (byte)(((a & 0xF) + (e & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x84: /* ADD A,H */
							temp = a + h;
							f = (byte)(((a & 0xF) + (h & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x85: /* ADD A,L */
							temp = a + l;
							f = (byte)(((a & 0xF) + (l & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x86: /* ADD A,(HL) */
							__temp8 = bus[l, h];
							temp = a + __temp8;
							f = (byte)(((a & 0xF) + (__temp8 & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							f &= 0xB0;
							cycleCount = 8;
							break;
						case 0x87: /* ADD A,A */
							temp = a + a;
							f = (byte)(((a & 0xF) + (a & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x88: /* ADC A,B */
							if ((f & CFlag) != 0)
							{
								temp = a + b + 1;
								f = (byte)(((a & 0xF) + (b & 0xF) > 0xE ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							else
							{
								temp = a + b;
								f = (byte)(((a & 0xF) + (b & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x89: /* ADC A,C */
							if ((f & CFlag) != 0)
							{
								temp = a + c + 1;
								f = (byte)(((a & 0xF) + (c & 0xF) > 0xE ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							else
							{
								temp = a + c;
								f = (byte)(((a & 0xF) + (c & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x8A: /* ADC A,D */
							if ((f & CFlag) != 0)
							{
								temp = a + d + 1;
								f = (byte)(((a & 0xF) + (d & 0xF) > 0xE ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							else
							{
								temp = a + d;
								f = (byte)(((a & 0xF) + (d & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x8B: /* ADC A,E */
							if ((f & CFlag) != 0)
							{
								temp = a + e + 1;
								f = (byte)(((a & 0xF) + (e & 0xF) > 0xE ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							else
							{
								temp = a + e;
								f = (byte)(((a & 0xF) + (e & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x8C: /* ADC A,H */
							if ((f & CFlag) != 0)
							{
								temp = a + h + 1;
								f = (byte)(((a & 0xF) + (h & 0xF) > 0xE ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							else
							{
								temp = a + h;
								f = (byte)(((a & 0xF) + (h & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x8D: /* ADC A,L */
							if ((f & CFlag) != 0)
							{
								temp = a + l + 1;
								f = (byte)(((a & 0xF) + (l & 0xF) > 0xE ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							else
							{
								temp = a + l;
								f = (byte)(((a & 0xF) + (l & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x8E: /* ADC A,(HL) */
							__temp8 = bus[l, h];
							if ((f & CFlag) != 0)
							{
								temp = a + __temp8 + 1;
								f = (byte)(((a & 0xF) + (__temp8 & 0xF) > 0xE ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							else
							{
								temp = a + __temp8;
								f = (byte)(((a & 0xF) + (__temp8 & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							f &= 0xB0;
							cycleCount = 8;
							break;
						case 0x8F: /* ADC A,A */
							if ((f & CFlag) != 0)
							{
								temp = a + a + 1;
								f = (byte)(((a & 0xF) + (a & 0xF) > 0xE ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							else
							{
								temp = a + a;
								f = (byte)(((a & 0xF) + (a & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							f &= 0xB0;
							cycleCount = 4;
							break;
						case 0x90: /* SUB A,B */
							f = (byte)(((a & 0xF) < (b & 0xF) ? HFlag : 0) | (a < b ? CFlag : 0) | NFlag | ((a -= b) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x91: /* SUB A,C */
							f = (byte)(((a & 0xF) < (c & 0xF) ? HFlag : 0) | (a < c ? CFlag : 0) | NFlag | ((a -= c) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x92: /* SUB A,D */
							f = (byte)(((a & 0xF) < (d & 0xF) ? HFlag : 0) | (a < d ? CFlag : 0) | NFlag | ((a -= d) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x93: /* SUB A,E */
							f = (byte)(((a & 0xF) < (e & 0xF) ? HFlag : 0) | (a < e ? CFlag : 0) | NFlag | ((a -= e) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x94: /* SUB A,H */
							f = (byte)(((a & 0xF) < (h & 0xF) ? HFlag : 0) | (a < h ? CFlag : 0) | NFlag | ((a -= h) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x95: /* SUB A,L */
							f = (byte)(((a & 0xF) < (l & 0xF) ? HFlag : 0) | (a < l ? CFlag : 0) | NFlag | ((a -= l) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x96: /* SUB A,(HL) */
							__temp8 = bus[l, h];
							f = (byte)(((a & 0xF) < (__temp8 & 0xF) ? HFlag : 0) | (a < __temp8 ? CFlag : 0) | NFlag | ((a -= __temp8) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 8;
							break;
						case 0x97: /* SUB A,A */
							a = 0;
							f = 0xC0;
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x98: /* SBC A,B */
							if ((f & CFlag) != 0) f = (byte)(((a & 0xF) - (b & 0xF) < 1 ? HFlag : 0) | (a - b < 1 ? CFlag : 0) | NFlag | ((a = (byte)(a - b - 1)) == 0 ? ZFlag : 0));
							else f = (byte)(((a & 0xF) < (b & 0xF) ? HFlag : 0) | (a < b ? CFlag : 0) | NFlag | ((a -= b) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x99: /* SBC A,C */
							if ((f & CFlag) != 0) f = (byte)(((a & 0xF) - (c & 0xF) < 1 ? HFlag : 0) | (a - c < 1 ? CFlag : 0) | NFlag | ((a = (byte)(a - c - 1)) == 0 ? ZFlag : 0));
							else f = (byte)(((a & 0xF) < (c & 0xF) ? HFlag : 0) | (a < c ? CFlag : 0) | NFlag | ((a -= c) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x9A: /* SBC A,D */
							if ((f & CFlag) != 0) f = (byte)(((a & 0xF) - (d & 0xF) < 1 ? HFlag : 0) | (a - d < 1 ? CFlag : 0) | NFlag | ((a = (byte)(a - d - 1)) == 0 ? ZFlag : 0));
							else f = (byte)(((a & 0xF) < (d & 0xF) ? HFlag : 0) | (a < d ? CFlag : 0) | NFlag | ((a -= d) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x9B: /* SBC A,E */
							if ((f & CFlag) != 0) f = (byte)(((a & 0xF) - (e & 0xF) < 1 ? HFlag : 0) | (a - e < 1 ? CFlag : 0) | NFlag | ((a = (byte)(a - e - 1)) == 0 ? ZFlag : 0));
							else f = (byte)(((a & 0xF) < (e & 0xF) ? HFlag : 0) | (a < e ? CFlag : 0) | NFlag | ((a -= e) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x9C: /* SBC A,H */
							if ((f & CFlag) != 0) f = (byte)(((a & 0xF) - (h & 0xF) < 1 ? HFlag : 0) | (a - h < 1 ? CFlag : 0) | NFlag | ((a = (byte)(a - h - 1)) == 0 ? ZFlag : 0));
							else f = (byte)(((a & 0xF) < (h & 0xF) ? HFlag : 0) | (a < h ? CFlag : 0) | NFlag | ((a -= h) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x9D: /* SBC A,L */
							if ((f & CFlag) != 0) f = (byte)(((a & 0xF) - (l & 0xF) < 1 ? HFlag : 0) | (a - l < 1 ? CFlag : 0) | NFlag | ((a = (byte)(a - l - 1)) == 0 ? ZFlag : 0));
							else f = (byte)(((a & 0xF) < (l & 0xF) ? HFlag : 0) | (a < l ? CFlag : 0) | NFlag | ((a -= l) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0x9E: /* SBC A,(HL) */
							__temp8 = bus[l, h];
							if ((f & CFlag) != 0) f = (byte)(((a & 0xF) - (__temp8 & 0xF) < 1 ? HFlag : 0) | (a - __temp8 < 1 ? CFlag : 0) | NFlag | ((a = (byte)(a - __temp8 - 1)) == 0 ? ZFlag : 0));
							else f = (byte)(((a & 0xF) < (__temp8 & 0xF) ? HFlag : 0) | (a < __temp8 ? CFlag : 0) | NFlag | ((a -= __temp8) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 8;
							break;
						case 0x9F: /* SBC A,A */
							if ((f & CFlag) != 0)
							{
								a = 0xFF;
								f = 0x70;
							}
							else
							{
								a = 0;
								f = 0xC0;
							}
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0xA0: /* AND A,B */
							f = (byte)((a &= b) == 0 ? f | ZFlag : f & NotZFlag);
							f = (byte)(f & 0xA0 | 0x20);
							cycleCount = 4;
							break;
						case 0xA1: /* AND A,C */
							f = (byte)((a &= c) == 0 ? f | ZFlag : f & NotZFlag);
							f = (byte)(f & 0xA0 | 0x20);
							cycleCount = 4;
							break;
						case 0xA2: /* AND A,D */
							f = (byte)((a &= d) == 0 ? f | ZFlag : f & NotZFlag);
							f = (byte)(f & 0xA0 | 0x20);
							cycleCount = 4;
							break;
						case 0xA3: /* AND A,E */
							f = (byte)((a &= e) == 0 ? f | ZFlag : f & NotZFlag);
							f = (byte)(f & 0xA0 | 0x20);
							cycleCount = 4;
							break;
						case 0xA4: /* AND A,H */
							f = (byte)((a &= h) == 0 ? f | ZFlag : f & NotZFlag);
							f = (byte)(f & 0xA0 | 0x20);
							cycleCount = 4;
							break;
						case 0xA5: /* AND A,L */
							f = (byte)((a &= l) == 0 ? f | ZFlag : f & NotZFlag);
							f = (byte)(f & 0xA0 | 0x20);
							cycleCount = 4;
							break;
						case 0xA6: /* AND A,(HL) */
							__temp8 = bus[l, h];
							f = (byte)((a &= __temp8) == 0 ? f | ZFlag : f & NotZFlag);
							f = (byte)(f & 0xA0 | 0x20);
							cycleCount = 8;
							break;
						case 0xA7: /* AND A,A */
							f = (byte)(a == 0 ? f | ZFlag : f & NotZFlag);
							f = (byte)(f & 0xA0 | 0x20);
							cycleCount = 4;
							break;
						case 0xA8: /* XOR A,B */
							f = (byte)((a ^= b) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xA9: /* XOR A,C */
							f = (byte)((a ^= c) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xAA: /* XOR A,D */
							f = (byte)((a ^= d) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xAB: /* XOR A,E */
							f = (byte)((a ^= e) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xAC: /* XOR A,H */
							f = (byte)((a ^= h) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xAD: /* XOR A,L */
							f = (byte)((a ^= l) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xAE: /* XOR A,(HL) */
							__temp8 = bus[l, h];
							f = (byte)((a ^= __temp8) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 8;
							break;
						case 0xAF: /* XOR A,A */
							a = 0;
							f |= ZFlag;
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xB0: /* OR A,B */
							f = (byte)((a |= b) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xB1: /* OR A,C */
							f = (byte)((a |= c) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xB2: /* OR A,D */
							f = (byte)((a |= d) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xB3: /* OR A,E */
							f = (byte)((a |= e) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xB4: /* OR A,H */
							f = (byte)((a |= h) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xB5: /* OR A,L */
							f = (byte)((a |= l) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xB6: /* OR A,(HL) */
							__temp8 = bus[l, h];
							f = (byte)((a |= __temp8) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 8;
							break;
						case 0xB7: /* OR A,A */
							f = (byte)(a == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 4;
							break;
						case 0xB8: /* CP A,B */
							f = (byte)(((a & 0xF) < (b & 0xF) ? HFlag : 0) | (a < b ? CFlag : 0) | NFlag | (a == b ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0xB9: /* CP A,C */
							f = (byte)(((a & 0xF) < (c & 0xF) ? HFlag : 0) | (a < c ? CFlag : 0) | NFlag | (a == c ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0xBA: /* CP A,D */
							f = (byte)(((a & 0xF) < (d & 0xF) ? HFlag : 0) | (a < d ? CFlag : 0) | NFlag | (a == d ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0xBB: /* CP A,E */
							f = (byte)(((a & 0xF) < (e & 0xF) ? HFlag : 0) | (a < e ? CFlag : 0) | NFlag | (a == e ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0xBC: /* CP A,H */
							f = (byte)(((a & 0xF) < (h & 0xF) ? HFlag : 0) | (a < h ? CFlag : 0) | NFlag | (a == h ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0xBD: /* CP A,L */
							f = (byte)(((a & 0xF) < (l & 0xF) ? HFlag : 0) | (a < l ? CFlag : 0) | NFlag | (a == l ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0xBE: /* CP A,(HL) */
							__temp8 = bus[l, h];
							f = (byte)(((a & 0xF) < (__temp8 & 0xF) ? HFlag : 0) | (a < __temp8 ? CFlag : 0) | NFlag | (a == __temp8 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 8;
							break;
						case 0xBF: /* CP A,A */
							f = 0xC0;
							f |= 0x40;
							cycleCount = 4;
							break;
						case 0xC0: /* RET NZ */
							if (((f & ZFlag) == 0))
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
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							if (((f & ZFlag) == 0))
							{
								pc = __temp16;
								cycleCount = 16;
							}
							else cycleCount = 12;
							break;
						case 0xC3: /* JP N */
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							pc = __temp16;
							cycleCount = 16;
							break;
						case 0xC4: /* CALL NZ,N */
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							if (((f & ZFlag) == 0))
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
							__temp8 = bus[pc++];
							temp = a + __temp8;
							f = (byte)(((a & 0xF) + (__temp8 & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							f &= 0xB0;
							cycleCount = 8;
							break;
						case 0xC7: /* RST $00 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 0;
							cycleCount = 16;
							break;
						case 0xC8: /* RET Z */
							if (((f & ZFlag) != 0))
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
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							if (((f & ZFlag) != 0))
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
									f = (byte)(((b & 0x80) != 0 ? CFlag : 0) | ((b = (byte)((b & 0x80) != 0 ? (b << 1) | 0x01 : b << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x01: /* RLC C */
									f = (byte)(((c & 0x80) != 0 ? CFlag : 0) | ((c = (byte)((c & 0x80) != 0 ? (c << 1) | 0x01 : c << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x02: /* RLC D */
									f = (byte)(((d & 0x80) != 0 ? CFlag : 0) | ((d = (byte)((d & 0x80) != 0 ? (d << 1) | 0x01 : d << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x03: /* RLC E */
									f = (byte)(((e & 0x80) != 0 ? CFlag : 0) | ((e = (byte)((e & 0x80) != 0 ? (e << 1) | 0x01 : e << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x04: /* RLC H */
									f = (byte)(((h & 0x80) != 0 ? CFlag : 0) | ((h = (byte)((h & 0x80) != 0 ? (h << 1) | 0x01 : h << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x05: /* RLC L */
									f = (byte)(((l & 0x80) != 0 ? CFlag : 0) | ((l = (byte)((l & 0x80) != 0 ? (l << 1) | 0x01 : l << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x06: /* RLC (HL) */
									__temp8 = bus[l, h];
									f = (byte)(((__temp8 & 0x80) != 0 ? CFlag : 0) | ((__temp8 = (byte)((__temp8 & 0x80) != 0 ? (__temp8 << 1) | 0x01 : __temp8 << 1)) == 0 ? ZFlag : 0));
									bus[l, h] = __temp8;
									f &= 0x90;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x07: /* RLC A */
									f = (byte)(((a & 0x80) != 0 ? CFlag : 0) | ((a = (byte)((a & 0x80) != 0 ? (a << 1) | 0x01 : a << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x08: /* RRC B */
									f = (byte)(((b & 0x01) != 0 ? CFlag : 0) | ((b = (byte)((b & 0x01) != 0 ? (b >> 1) | 0x80 : b >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x09: /* RRC C */
									f = (byte)(((c & 0x01) != 0 ? CFlag : 0) | ((c = (byte)((c & 0x01) != 0 ? (c >> 1) | 0x80 : c >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x0A: /* RRC D */
									f = (byte)(((d & 0x01) != 0 ? CFlag : 0) | ((d = (byte)((d & 0x01) != 0 ? (d >> 1) | 0x80 : d >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x0B: /* RRC E */
									f = (byte)(((e & 0x01) != 0 ? CFlag : 0) | ((e = (byte)((e & 0x01) != 0 ? (e >> 1) | 0x80 : e >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x0C: /* RRC H */
									f = (byte)(((h & 0x01) != 0 ? CFlag : 0) | ((h = (byte)((h & 0x01) != 0 ? (h >> 1) | 0x80 : h >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x0D: /* RRC L */
									f = (byte)(((l & 0x01) != 0 ? CFlag : 0) | ((l = (byte)((l & 0x01) != 0 ? (l >> 1) | 0x80 : l >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x0E: /* RRC (HL) */
									__temp8 = bus[l, h];
									f = (byte)(((__temp8 & 0x01) != 0 ? CFlag : 0) | ((__temp8 = (byte)((__temp8 & 0x01) != 0 ? (__temp8 >> 1) | 0x80 : __temp8 >> 1)) == 0 ? ZFlag : 0));
									bus[l, h] = __temp8;
									f &= 0x90;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x0F: /* RRC A */
									f = (byte)(((a & 0x01) != 0 ? CFlag : 0) | ((a = (byte)((a & 0x01) != 0 ? (a >> 1) | 0x80 : a >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x10: /* RL B */
									f = (byte)(((b & 0x80) != 0 ? CFlag : 0) | ((b = (byte)((f & CFlag) != 0 ? (b << 1) | 0x01 : b << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x11: /* RL C */
									f = (byte)(((c & 0x80) != 0 ? CFlag : 0) | ((c = (byte)((f & CFlag) != 0 ? (c << 1) | 0x01 : c << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x12: /* RL D */
									f = (byte)(((d & 0x80) != 0 ? CFlag : 0) | ((d = (byte)((f & CFlag) != 0 ? (d << 1) | 0x01 : d << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x13: /* RL E */
									f = (byte)(((e & 0x80) != 0 ? CFlag : 0) | ((e = (byte)((f & CFlag) != 0 ? (e << 1) | 0x01 : e << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x14: /* RL H */
									f = (byte)(((h & 0x80) != 0 ? CFlag : 0) | ((h = (byte)((f & CFlag) != 0 ? (h << 1) | 0x01 : h << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x15: /* RL L */
									f = (byte)(((l & 0x80) != 0 ? CFlag : 0) | ((l = (byte)((f & CFlag) != 0 ? (l << 1) | 0x01 : l << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x16: /* RL (HL) */
									__temp8 = bus[l, h];
									f = (byte)(((__temp8 & 0x80) != 0 ? CFlag : 0) | ((__temp8 = (byte)((f & CFlag) != 0 ? (__temp8 << 1) | 0x01 : __temp8 << 1)) == 0 ? ZFlag : 0));
									bus[l, h] = __temp8;
									f &= 0x90;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x17: /* RL A */
									f = (byte)(((a & 0x80) != 0 ? CFlag : 0) | ((a = (byte)((f & CFlag) != 0 ? (a << 1) | 0x01 : a << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x18: /* RR B */
									f = (byte)(((b & 0x01) != 0 ? CFlag : 0) | ((b = (byte)((f & CFlag) != 0 ? (b >> 1) | 0x80 : b >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x19: /* RR C */
									f = (byte)(((c & 0x01) != 0 ? CFlag : 0) | ((c = (byte)((f & CFlag) != 0 ? (c >> 1) | 0x80 : c >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x1A: /* RR D */
									f = (byte)(((d & 0x01) != 0 ? CFlag : 0) | ((d = (byte)((f & CFlag) != 0 ? (d >> 1) | 0x80 : d >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x1B: /* RR E */
									f = (byte)(((e & 0x01) != 0 ? CFlag : 0) | ((e = (byte)((f & CFlag) != 0 ? (e >> 1) | 0x80 : e >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x1C: /* RR H */
									f = (byte)(((h & 0x01) != 0 ? CFlag : 0) | ((h = (byte)((f & CFlag) != 0 ? (h >> 1) | 0x80 : h >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x1D: /* RR L */
									f = (byte)(((l & 0x01) != 0 ? CFlag : 0) | ((l = (byte)((f & CFlag) != 0 ? (l >> 1) | 0x80 : l >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x1E: /* RR (HL) */
									__temp8 = bus[l, h];
									f = (byte)(((__temp8 & 0x01) != 0 ? CFlag : 0) | ((__temp8 = (byte)((f & CFlag) != 0 ? (__temp8 >> 1) | 0x80 : __temp8 >> 1)) == 0 ? ZFlag : 0));
									bus[l, h] = __temp8;
									f &= 0x90;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x1F: /* RR A */
									f = (byte)(((a & 0x01) != 0 ? CFlag : 0) | ((a = (byte)((f & CFlag) != 0 ? (a >> 1) | 0x80 : a >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x20: /* SLA B */
									f = (byte)(((b & 0x80) != 0 ? CFlag : 0) | ((b = (byte)(b << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x21: /* SLA C */
									f = (byte)(((c & 0x80) != 0 ? CFlag : 0) | ((c = (byte)(c << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x22: /* SLA D */
									f = (byte)(((d & 0x80) != 0 ? CFlag : 0) | ((d = (byte)(d << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x23: /* SLA E */
									f = (byte)(((e & 0x80) != 0 ? CFlag : 0) | ((e = (byte)(e << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x24: /* SLA H */
									f = (byte)(((h & 0x80) != 0 ? CFlag : 0) | ((h = (byte)(h << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x25: /* SLA L */
									f = (byte)(((l & 0x80) != 0 ? CFlag : 0) | ((l = (byte)(l << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x26: /* SLA (HL) */
									__temp8 = bus[l, h];
									f = (byte)(((__temp8 & 0x80) != 0 ? CFlag : 0) | ((__temp8 = (byte)(__temp8 << 1)) == 0 ? ZFlag : 0));
									bus[l, h] = __temp8;
									f &= 0x90;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x27: /* SLA A */
									f = (byte)(((a & 0x80) != 0 ? CFlag : 0) | ((a = (byte)(a << 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x28: /* SRA B */
									f = (byte)(((b & 0x01) != 0 ? CFlag : 0) | ((b = (byte)((sbyte)b >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x29: /* SRA C */
									f = (byte)(((c & 0x01) != 0 ? CFlag : 0) | ((c = (byte)((sbyte)c >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x2A: /* SRA D */
									f = (byte)(((d & 0x01) != 0 ? CFlag : 0) | ((d = (byte)((sbyte)d >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x2B: /* SRA E */
									f = (byte)(((e & 0x01) != 0 ? CFlag : 0) | ((e = (byte)((sbyte)e >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x2C: /* SRA H */
									f = (byte)(((h & 0x01) != 0 ? CFlag : 0) | ((h = (byte)((sbyte)h >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x2D: /* SRA L */
									f = (byte)(((l & 0x01) != 0 ? CFlag : 0) | ((l = (byte)((sbyte)l >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x2E: /* SRA (HL) */
									__temp8 = bus[l, h];
									f = (byte)(((__temp8 & 0x01) != 0 ? CFlag : 0) | ((__temp8 = (byte)((sbyte)__temp8 >> 1)) == 0 ? ZFlag : 0));
									bus[l, h] = __temp8;
									f &= 0x90;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x2F: /* SRA A */
									f = (byte)(((a & 0x01) != 0 ? CFlag : 0) | ((a = (byte)((sbyte)a >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x30: /* SWAP B */
									f = (byte)((b = (byte)((b >> 4) | (b << 4))) == 0 ? ZFlag : 0);
									f &= 0x80;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x31: /* SWAP C */
									f = (byte)((c = (byte)((c >> 4) | (c << 4))) == 0 ? ZFlag : 0);
									f &= 0x80;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x32: /* SWAP D */
									f = (byte)((d = (byte)((d >> 4) | (d << 4))) == 0 ? ZFlag : 0);
									f &= 0x80;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x33: /* SWAP E */
									f = (byte)((e = (byte)((e >> 4) | (e << 4))) == 0 ? ZFlag : 0);
									f &= 0x80;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x34: /* SWAP H */
									f = (byte)((h = (byte)((h >> 4) | (h << 4))) == 0 ? ZFlag : 0);
									f &= 0x80;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x35: /* SWAP L */
									f = (byte)((l = (byte)((l >> 4) | (l << 4))) == 0 ? ZFlag : 0);
									f &= 0x80;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x36: /* SWAP (HL) */
									__temp8 = bus[l, h];
									f = (byte)((__temp8 = (byte)((__temp8 >> 4) | (__temp8 << 4))) == 0 ? ZFlag : 0);
									bus[l, h] = __temp8;
									f &= 0x80;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x37: /* SWAP A */
									f = (byte)((a = (byte)((a >> 4) | (a << 4))) == 0 ? ZFlag : 0);
									f &= 0x80;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x38: /* SRL B */
									f = (byte)(((b & 0x01) != 0 ? CFlag : 0) | ((b = (byte)(b >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x39: /* SRL C */
									f = (byte)(((c & 0x01) != 0 ? CFlag : 0) | ((c = (byte)(c >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x3A: /* SRL D */
									f = (byte)(((d & 0x01) != 0 ? CFlag : 0) | ((d = (byte)(d >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x3B: /* SRL E */
									f = (byte)(((e & 0x01) != 0 ? CFlag : 0) | ((e = (byte)(e >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x3C: /* SRL H */
									f = (byte)(((h & 0x01) != 0 ? CFlag : 0) | ((h = (byte)(h >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x3D: /* SRL L */
									f = (byte)(((l & 0x01) != 0 ? CFlag : 0) | ((l = (byte)(l >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x3E: /* SRL (HL) */
									__temp8 = bus[l, h];
									f = (byte)(((__temp8 & 0x01) != 0 ? CFlag : 0) | ((__temp8 = (byte)(__temp8 >> 1)) == 0 ? ZFlag : 0));
									bus[l, h] = __temp8;
									f &= 0x90;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x3F: /* SRL A */
									f = (byte)(((a & 0x01) != 0 ? CFlag : 0) | ((a = (byte)(a >> 1)) == 0 ? ZFlag : 0));
									f &= 0x90;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x40: /* BIT 0,B */
									f = (byte)((b & 0x01) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x41: /* BIT 0,C */
									f = (byte)((c & 0x01) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x42: /* BIT 0,D */
									f = (byte)((d & 0x01) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x43: /* BIT 0,E */
									f = (byte)((e & 0x01) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x44: /* BIT 0,H */
									f = (byte)((h & 0x01) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x45: /* BIT 0,L */
									f = (byte)((l & 0x01) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x46: /* BIT 0,(HL) */
									__temp8 = bus[l, h];
									f = (byte)((__temp8 & 0x01) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x47: /* BIT 0,A */
									f = (byte)((a & 0x01) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x48: /* BIT 1,B */
									f = (byte)((b & 0x02) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x49: /* BIT 1,C */
									f = (byte)((c & 0x02) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x4A: /* BIT 1,D */
									f = (byte)((d & 0x02) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x4B: /* BIT 1,E */
									f = (byte)((e & 0x02) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x4C: /* BIT 1,H */
									f = (byte)((h & 0x02) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x4D: /* BIT 1,L */
									f = (byte)((l & 0x02) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x4E: /* BIT 1,(HL) */
									__temp8 = bus[l, h];
									f = (byte)((__temp8 & 0x02) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x4F: /* BIT 1,A */
									f = (byte)((a & 0x02) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x50: /* BIT 2,B */
									f = (byte)((b & 0x04) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x51: /* BIT 2,C */
									f = (byte)((c & 0x04) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x52: /* BIT 2,D */
									f = (byte)((d & 0x04) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x53: /* BIT 2,E */
									f = (byte)((e & 0x04) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x54: /* BIT 2,H */
									f = (byte)((h & 0x04) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x55: /* BIT 2,L */
									f = (byte)((l & 0x04) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x56: /* BIT 2,(HL) */
									__temp8 = bus[l, h];
									f = (byte)((__temp8 & 0x04) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x57: /* BIT 2,A */
									f = (byte)((a & 0x04) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x58: /* BIT 3,B */
									f = (byte)((b & 0x08) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x59: /* BIT 3,C */
									f = (byte)((c & 0x08) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x5A: /* BIT 3,D */
									f = (byte)((d & 0x08) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x5B: /* BIT 3,E */
									f = (byte)((e & 0x08) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x5C: /* BIT 3,H */
									f = (byte)((h & 0x08) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x5D: /* BIT 3,L */
									f = (byte)((l & 0x08) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x5E: /* BIT 3,(HL) */
									__temp8 = bus[l, h];
									f = (byte)((__temp8 & 0x08) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x5F: /* BIT 3,A */
									f = (byte)((a & 0x08) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x60: /* BIT 4,B */
									f = (byte)((b & 0x10) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x61: /* BIT 4,C */
									f = (byte)((c & 0x10) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x62: /* BIT 4,D */
									f = (byte)((d & 0x10) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x63: /* BIT 4,E */
									f = (byte)((e & 0x10) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x64: /* BIT 4,H */
									f = (byte)((h & 0x10) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x65: /* BIT 4,L */
									f = (byte)((l & 0x10) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x66: /* BIT 4,(HL) */
									__temp8 = bus[l, h];
									f = (byte)((__temp8 & 0x10) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x67: /* BIT 4,A */
									f = (byte)((a & 0x10) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x68: /* BIT 5,B */
									f = (byte)((b & 0x20) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x69: /* BIT 5,C */
									f = (byte)((c & 0x20) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x6A: /* BIT 5,D */
									f = (byte)((d & 0x20) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x6B: /* BIT 5,E */
									f = (byte)((e & 0x20) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x6C: /* BIT 5,H */
									f = (byte)((h & 0x20) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x6D: /* BIT 5,L */
									f = (byte)((l & 0x20) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x6E: /* BIT 5,(HL) */
									__temp8 = bus[l, h];
									f = (byte)((__temp8 & 0x20) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x6F: /* BIT 5,A */
									f = (byte)((a & 0x20) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x70: /* BIT 6,B */
									f = (byte)((b & 0x40) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x71: /* BIT 6,C */
									f = (byte)((c & 0x40) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x72: /* BIT 6,D */
									f = (byte)((d & 0x40) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x73: /* BIT 6,E */
									f = (byte)((e & 0x40) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x74: /* BIT 6,H */
									f = (byte)((h & 0x40) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x75: /* BIT 6,L */
									f = (byte)((l & 0x40) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x76: /* BIT 6,(HL) */
									__temp8 = bus[l, h];
									f = (byte)((__temp8 & 0x40) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x77: /* BIT 6,A */
									f = (byte)((a & 0x40) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x78: /* BIT 7,B */
									f = (byte)((b & 0x80) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x79: /* BIT 7,C */
									f = (byte)((c & 0x80) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x7A: /* BIT 7,D */
									f = (byte)((d & 0x80) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x7B: /* BIT 7,E */
									f = (byte)((e & 0x80) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x7C: /* BIT 7,H */
									f = (byte)((h & 0x80) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x7D: /* BIT 7,L */
									f = (byte)((l & 0x80) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x7E: /* BIT 7,(HL) */
									__temp8 = bus[l, h];
									f = (byte)((__temp8 & 0x80) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 12;
									break;
								case /* 0xCB */ 0x7F: /* BIT 7,A */
									f = (byte)((a & 0x80) == 0 ? f | ZFlag : f & NotZFlag);
									f = (byte)(f & 0xB0 | 0x20);
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x80: /* RES 0,B */
									b &= 0xFE;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x81: /* RES 0,C */
									c &= 0xFE;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x82: /* RES 0,D */
									d &= 0xFE;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x83: /* RES 0,E */
									e &= 0xFE;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x84: /* RES 0,H */
									h &= 0xFE;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x85: /* RES 0,L */
									l &= 0xFE;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x86: /* RES 0,(HL) */
									__temp8 = bus[l, h];
									__temp8 &= 0xFE;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x87: /* RES 0,A */
									a &= 0xFE;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x88: /* RES 1,B */
									b &= 0xFD;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x89: /* RES 1,C */
									c &= 0xFD;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x8A: /* RES 1,D */
									d &= 0xFD;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x8B: /* RES 1,E */
									e &= 0xFD;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x8C: /* RES 1,H */
									h &= 0xFD;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x8D: /* RES 1,L */
									l &= 0xFD;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x8E: /* RES 1,(HL) */
									__temp8 = bus[l, h];
									__temp8 &= 0xFD;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x8F: /* RES 1,A */
									a &= 0xFD;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x90: /* RES 2,B */
									b &= 0xFB;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x91: /* RES 2,C */
									c &= 0xFB;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x92: /* RES 2,D */
									d &= 0xFB;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x93: /* RES 2,E */
									e &= 0xFB;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x94: /* RES 2,H */
									h &= 0xFB;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x95: /* RES 2,L */
									l &= 0xFB;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x96: /* RES 2,(HL) */
									__temp8 = bus[l, h];
									__temp8 &= 0xFB;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x97: /* RES 2,A */
									a &= 0xFB;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x98: /* RES 3,B */
									b &= 0xF7;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x99: /* RES 3,C */
									c &= 0xF7;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x9A: /* RES 3,D */
									d &= 0xF7;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x9B: /* RES 3,E */
									e &= 0xF7;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x9C: /* RES 3,H */
									h &= 0xF7;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x9D: /* RES 3,L */
									l &= 0xF7;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0x9E: /* RES 3,(HL) */
									__temp8 = bus[l, h];
									__temp8 &= 0xF7;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0x9F: /* RES 3,A */
									a &= 0xF7;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA0: /* RES 4,B */
									b &= 0xEF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA1: /* RES 4,C */
									c &= 0xEF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA2: /* RES 4,D */
									d &= 0xEF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA3: /* RES 4,E */
									e &= 0xEF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA4: /* RES 4,H */
									h &= 0xEF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA5: /* RES 4,L */
									l &= 0xEF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA6: /* RES 4,(HL) */
									__temp8 = bus[l, h];
									__temp8 &= 0xEF;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xA7: /* RES 4,A */
									a &= 0xEF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA8: /* RES 5,B */
									b &= 0xDF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xA9: /* RES 5,C */
									c &= 0xDF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xAA: /* RES 5,D */
									d &= 0xDF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xAB: /* RES 5,E */
									e &= 0xDF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xAC: /* RES 5,H */
									h &= 0xDF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xAD: /* RES 5,L */
									l &= 0xDF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xAE: /* RES 5,(HL) */
									__temp8 = bus[l, h];
									__temp8 &= 0xDF;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xAF: /* RES 5,A */
									a &= 0xDF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB0: /* RES 6,B */
									b &= 0xBF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB1: /* RES 6,C */
									c &= 0xBF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB2: /* RES 6,D */
									d &= 0xBF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB3: /* RES 6,E */
									e &= 0xBF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB4: /* RES 6,H */
									h &= 0xBF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB5: /* RES 6,L */
									l &= 0xBF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB6: /* RES 6,(HL) */
									__temp8 = bus[l, h];
									__temp8 &= 0xBF;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xB7: /* RES 6,A */
									a &= 0xBF;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB8: /* RES 7,B */
									b &= 0x7F;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xB9: /* RES 7,C */
									c &= 0x7F;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xBA: /* RES 7,D */
									d &= 0x7F;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xBB: /* RES 7,E */
									e &= 0x7F;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xBC: /* RES 7,H */
									h &= 0x7F;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xBD: /* RES 7,L */
									l &= 0x7F;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xBE: /* RES 7,(HL) */
									__temp8 = bus[l, h];
									__temp8 &= 0x7F;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xBF: /* RES 7,A */
									a &= 0x7F;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC0: /* SET 0,B */
									b |= 0x01;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC1: /* SET 0,C */
									c |= 0x01;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC2: /* SET 0,D */
									d |= 0x01;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC3: /* SET 0,E */
									e |= 0x01;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC4: /* SET 0,H */
									h |= 0x01;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC5: /* SET 0,L */
									l |= 0x01;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC6: /* SET 0,(HL) */
									__temp8 = bus[l, h];
									__temp8 |= 0x01;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xC7: /* SET 0,A */
									a |= 0x01;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC8: /* SET 1,B */
									b |= 0x02;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xC9: /* SET 1,C */
									c |= 0x02;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xCA: /* SET 1,D */
									d |= 0x02;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xCB: /* SET 1,E */
									e |= 0x02;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xCC: /* SET 1,H */
									h |= 0x02;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xCD: /* SET 1,L */
									l |= 0x02;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xCE: /* SET 1,(HL) */
									__temp8 = bus[l, h];
									__temp8 |= 0x02;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xCF: /* SET 1,A */
									a |= 0x02;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD0: /* SET 2,B */
									b |= 0x04;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD1: /* SET 2,C */
									c |= 0x04;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD2: /* SET 2,D */
									d |= 0x04;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD3: /* SET 2,E */
									e |= 0x04;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD4: /* SET 2,H */
									h |= 0x04;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD5: /* SET 2,L */
									l |= 0x04;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD6: /* SET 2,(HL) */
									__temp8 = bus[l, h];
									__temp8 |= 0x04;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xD7: /* SET 2,A */
									a |= 0x04;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD8: /* SET 3,B */
									b |= 0x08;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xD9: /* SET 3,C */
									c |= 0x08;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xDA: /* SET 3,D */
									d |= 0x08;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xDB: /* SET 3,E */
									e |= 0x08;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xDC: /* SET 3,H */
									h |= 0x08;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xDD: /* SET 3,L */
									l |= 0x08;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xDE: /* SET 3,(HL) */
									__temp8 = bus[l, h];
									__temp8 |= 0x08;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xDF: /* SET 3,A */
									a |= 0x08;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE0: /* SET 4,B */
									b |= 0x10;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE1: /* SET 4,C */
									c |= 0x10;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE2: /* SET 4,D */
									d |= 0x10;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE3: /* SET 4,E */
									e |= 0x10;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE4: /* SET 4,H */
									h |= 0x10;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE5: /* SET 4,L */
									l |= 0x10;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE6: /* SET 4,(HL) */
									__temp8 = bus[l, h];
									__temp8 |= 0x10;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xE7: /* SET 4,A */
									a |= 0x10;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE8: /* SET 5,B */
									b |= 0x20;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xE9: /* SET 5,C */
									c |= 0x20;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xEA: /* SET 5,D */
									d |= 0x20;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xEB: /* SET 5,E */
									e |= 0x20;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xEC: /* SET 5,H */
									h |= 0x20;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xED: /* SET 5,L */
									l |= 0x20;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xEE: /* SET 5,(HL) */
									__temp8 = bus[l, h];
									__temp8 |= 0x20;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xEF: /* SET 5,A */
									a |= 0x20;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF0: /* SET 6,B */
									b |= 0x40;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF1: /* SET 6,C */
									c |= 0x40;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF2: /* SET 6,D */
									d |= 0x40;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF3: /* SET 6,E */
									e |= 0x40;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF4: /* SET 6,H */
									h |= 0x40;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF5: /* SET 6,L */
									l |= 0x40;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF6: /* SET 6,(HL) */
									__temp8 = bus[l, h];
									__temp8 |= 0x40;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xF7: /* SET 6,A */
									a |= 0x40;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF8: /* SET 7,B */
									b |= 0x80;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xF9: /* SET 7,C */
									c |= 0x80;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xFA: /* SET 7,D */
									d |= 0x80;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xFB: /* SET 7,E */
									e |= 0x80;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xFC: /* SET 7,H */
									h |= 0x80;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xFD: /* SET 7,L */
									l |= 0x80;
									cycleCount = 8;
									break;
								case /* 0xCB */ 0xFE: /* SET 7,(HL) */
									__temp8 = bus[l, h];
									__temp8 |= 0x80;
									bus[l, h] = __temp8;
									cycleCount = 16;
									break;
								case /* 0xCB */ 0xFF: /* SET 7,A */
									a |= 0x80;
									cycleCount = 8;
									break;
							}
							break;
						case 0xCC: /* CALL Z,N */
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							if (((f & ZFlag) != 0))
							{
								bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = __temp16;
								cycleCount = 24;
							}
							else cycleCount = 12;
							break;
						case 0xCD: /* CALL N */
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = __temp16;
							cycleCount = 24;
							break;
						case 0xCE: /* ADC A,N */
							__temp8 = bus[pc++];
							if ((f & CFlag) != 0)
							{
								temp = a + __temp8 + 1;
								f = (byte)(((a & 0xF) + (__temp8 & 0xF) > 0xE ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							else
							{
								temp = a + __temp8;
								f = (byte)(((a & 0xF) + (__temp8 & 0xF) > 0xF ? HFlag : 0) | (temp > 0xFF ? CFlag : 0) | ((a = (byte)temp) == 0 ? ZFlag : 0));
							}
							f &= 0xB0;
							cycleCount = 8;
							break;
						case 0xCF: /* RST $08 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 8;
							cycleCount = 16;
							break;
						case 0xD0: /* RET NC */
							if (((f & CFlag) == 0))
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
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							if (((f & CFlag) == 0))
							{
								pc = __temp16;
								cycleCount = 16;
							}
							else cycleCount = 12;
							break;
						case 0xD4: /* CALL NC,N */
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							if (((f & CFlag) == 0))
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
							__temp8 = bus[pc++];
							f = (byte)(((a & 0xF) < (__temp8 & 0xF) ? HFlag : 0) | (a < __temp8 ? CFlag : 0) | NFlag | ((a -= __temp8) == 0 ? ZFlag : 0));
							f |= 0x40;
							cycleCount = 8;
							break;
						case 0xD7: /* RST $10 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 16;
							cycleCount = 16;
							break;
						case 0xD8: /* RET C */
							if (((f & CFlag) != 0))
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
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							if (((f & CFlag) != 0))
							{
								pc = __temp16;
								cycleCount = 16;
							}
							else cycleCount = 12;
							break;
						case 0xDC: /* CALL C,N */
							__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
							if (((f & CFlag) != 0))
							{
								bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = __temp16;
								cycleCount = 24;
							}
							else cycleCount = 12;
							break;
						case 0xDE: /* SBC A,N */
							__temp8 = bus[pc++];
							if ((f & CFlag) != 0) f = (byte)(((a & 0xF) - (__temp8 & 0xF) < 1 ? HFlag : 0) | (a - __temp8 < 1 ? CFlag : 0) | NFlag | ((a = (byte)(a - __temp8 - 1)) == 0 ? ZFlag : 0));
							else f = (byte)(((a & 0xF) < (__temp8 & 0xF) ? HFlag : 0) | (a < __temp8 ? CFlag : 0) | NFlag | ((a -= __temp8) == 0 ? ZFlag : 0));
							f |= 0x40;
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
							bus.WritePort(bus[pc++], __temp8);
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
							__temp8 = bus[pc++];
							f = (byte)((a &= __temp8) == 0 ? f | ZFlag : f & NotZFlag);
							f = (byte)(f & 0xA0 | 0x20);
							cycleCount = 8;
							break;
						case 0xE7: /* RST $20 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 32;
							cycleCount = 16;
							break;
						case 0xE8: /* ADD SP,N */
							__temp16 = (ushort)(sbyte)bus[pc++];
							temp = sp + __temp16;
							f = (byte)((((sp ^ __temp16 ^ temp) & 0x10) != 0 ? HFlag : 0) | (((sp ^ __temp16 ^ temp) & 0x100) != 0 ? CFlag : 0));
							sp = (ushort)temp;
							f &= 0x30;
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
							bus[bus[pc++], bus[pc++]] = __temp8;
							cycleCount = 16;
							break;
						case 0xEE: /* XOR A,N */
							__temp8 = bus[pc++];
							f = (byte)((a ^= __temp8) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 8;
							break;
						case 0xEF: /* RST $28 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 40;
							cycleCount = 16;
							break;
						case 0xF0: /* LD A,($FF00+N) */
							__temp8 = bus.ReadPort(bus[pc++]);
							a = __temp8;
							cycleCount = 12;
							break;
						case 0xF1: /* POP AF */
							__temp16 = (ushort)(bus[sp++] | (bus[sp++] << 8));
							a = (byte)(__temp16 >> 8); f = (byte)(__temp16 & 0x0F0);
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
							__temp16 = (ushort)((a << 8) | f);
							bus[--sp] = (byte)(__temp16 >> 8);
							bus[--sp] = (byte)__temp16;
							cycleCount = 16;
							break;
						case 0xF6: /* OR A,N */
							__temp8 = bus[pc++];
							f = (byte)((a |= __temp8) == 0 ? f | ZFlag : f & NotZFlag);
							f &= 0x80;
							cycleCount = 8;
							break;
						case 0xF7: /* RST $30 */
							bus[--sp] = (byte)(pc >> 8);
							bus[--sp] = (byte)pc;
							pc = 48;
							cycleCount = 16;
							break;
						case 0xF8: /* LD HL,SP+N */
							__temp16 = (ushort)(sbyte)bus[pc++];
							temp = sp + __temp16;
							f = (byte)((((sp ^ __temp16 ^ temp) & 0x10) != 0 ? HFlag : 0) | (((sp ^ __temp16 ^ temp) & 0x100) != 0 ? CFlag : 0));
							__tempHL = (ushort)temp;
							h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);
							f &= 0x30;
							cycleCount = 12;
							break;
						case 0xF9: /* LD SP,HL */
							__tempHL = (ushort)((h << 8) | l);
							sp = __tempHL;
							cycleCount = 8;
							break;
						case 0xFA: /* LD A,(N) */
							__temp8 = bus[bus[pc++], bus[pc++]];
							a = __temp8;
							cycleCount = 16;
							break;
						case 0xFB: /* EI */
							// Will enable interrupts one instruction later, or directly after this one if EI has been repeated.
							if (enableInterruptDelay == 0) enableInterruptDelay = 2;
							cycleCount = 4;
							break;
						case 0xFE: /* CP A,N */
							__temp8 = bus[pc++];
							f = (byte)(((a & 0xF) < (__temp8 & 0xF) ? HFlag : 0) | (a < __temp8 ? CFlag : 0) | NFlag | (a == __temp8 ? ZFlag : 0));
							f |= 0x40;
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
				A = a; F = f; B = b; C = c; D = d; E = e; H = h; L = l;
				SP = sp; PC = pc;

				InterruptMasterEnable = ime;
			}
		}
	}
}
