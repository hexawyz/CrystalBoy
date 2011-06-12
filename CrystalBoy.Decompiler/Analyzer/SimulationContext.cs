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
using System.Text;

namespace CrystalBoy.Decompiler.Analyzer
{
	sealed class SimulationContext : ICloneable
	{
		byte a, b, c, d, e, f, h, l;
		ushort sp;
		int pc;
		byte romBank;

		public SimulationContext()
		{
			// AF = 0x01B0
			a = 0x01; f = 0xB0;
			// BC = 0x0013
			b = 0x00; c = 0x13;
			// DE = 0x00D8
			d = 0x00; e = 0xD8;
			// HL = 0x014D
			h = 0x01; l = 0x4D;
			// SP = 0xFFFE
			sp = 0xFFFE;
			// PC = 0x0100
			pc = 0x0100;

			romBank = 1;
		}

		public SimulationContext(SimulationContext source)
		{
			a = source.a;
			b = source.b;
			c = source.c;
			d = source.d;
			e = source.e;
			f = source.f;
			h = source.h;
			l = source.l;
			sp = source.sp;
			pc = source.pc;
			romBank = source.romBank;
		}

		#region 8 bit Registers

		public byte A { get { return a; } set { a = value; } }
		public byte F {get { return f; } set { f = (byte)(value & 0xF0); } }
		public byte B { get { return b; } set { b = value; } }
		public byte C { get { return c; } set { c = value; } }
		public byte D { get { return d; } set { d = value; } }
		public byte E { get { return e; } set { e = value; } }
		public byte H { get { return h; } set { h = value; } }
		public byte L { get { return l; } set { l = value; } }

		#endregion

		#region 16 bit Registers

		public ushort AF
		{
			get
			{
				return (ushort)((a << 8) | f);
			}
			set
			{
				a = (byte)(value >> 8);
				f =  (byte)(value & 0xF0);
			}
		}

		public ushort BC
		{
			get
			{
				return (ushort)((b << 8) | c);
			}
			set
			{
				b =  (byte)(value >> 8);
				c =  (byte)(value & 0xFF);
			}
		}

		public ushort DE
		{
			get
			{
				return (ushort)((d << 8) | e);
			}
			set
			{
				d =  (byte)(value >> 8);
				e =  (byte)(value & 0xFF);
			}
		}

		public ushort HL
		{
			get
			{
				return (ushort)((h << 8) | l);
			}
			set
			{
				h =  (byte)(value >> 8);
				l =  (byte)(value & 0xFF);
			}
		}

		public ushort SP
		{
			get
			{
				return sp;
			}
			set
			{
				sp = value;
			}
		}

		public int PC
		{
			get
			{
				return pc;
			}
			set
			{
				pc = value;
			}
		}

		#endregion

		#region Flags

		public bool ZeroFlag
		{
			get
			{
				return (f & 0x80) != 0;
			}
			set
			{
				if (value)
					f |= 0x80;
				else
					f &= 0x7F;
			}
		}

		public bool CarryFlag
		{
			get
			{
				return (f & 0x10) != 0;
			}
			set
			{
				if (value)
					f |= 0x10;
				else
					f &= 0xE0;
			}
		}

		#endregion

		public byte RomBank
		{
			get
			{
				return romBank;
			}
			set
			{
				romBank = value;
			}
		}

		#region Register Manipulation Functions

		#region 16 bit Registers

		public ushort GetRegister16(Register16 register)
		{
			switch (register)
			{
				case Register16.Af: return AF;
				case Register16.Bc: return BC;
				case Register16.De: return DE;
				case Register16.Hl: return HL;
				case Register16.Sp: return SP;
				default: return 0;
			}
		}

		public void SetRegister16(Register16 register, ushort value)
		{
			switch (register)
			{
				case Register16.Af: AF = value; break;
				case Register16.Bc: BC = value; break;
				case Register16.De: DE = value; break;
				case Register16.Hl: HL = value; break;
				case Register16.Sp: SP = value; break;
			}
		}

		#endregion

		#region 8 bit Registers

		public byte GetRegister8(Register8 register)
		{
			switch (register)
			{
				case Register8.A: return A;
				case Register8.B: return B;
				case Register8.C: return C;
				case Register8.D: return D;
				case Register8.E: return E;
				case Register8.H: return H;
				case Register8.L: return L;
				default: return 0;
			}
		}

		public void SetRegister8(Register8 register, byte value)
		{
			switch (register)
			{
				case Register8.A: A = value; break;
				case Register8.B: B = value; break;
				case Register8.C: C = value; break;
				case Register8.D: D = value; break;
				case Register8.E: E = value; break;
				case Register8.H: H = value; break;
				case Register8.L: L = value; break;
			}
		}

		#endregion

		#endregion

		object ICloneable.Clone()
		{
			return this.MemberwiseClone();
		}

		public SimulationContext Clone()
		{
			return new SimulationContext(this);
		}
	}
}
