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
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	// It seems the CPU used in GB hardware is Sharp LR35902 (aka GB-Z80)
	public sealed partial class Processor
	{
		byte a, b, c, d, e, f, h, l;
		ushort sp, pc;
		bool ime;
		GameBoyMemoryBus bus;
		ProcessorStatus status;

		internal Processor(GameBoyMemoryBus bus)
		{
			this.bus = bus;
		}

		internal void Reset()
		{
			if (bus.HardwareType == HardwareType.GameBoyPocket)
				AF = 0xFFB0;
			else if (bus.HardwareType == HardwareType.GameBoyColor || bus.HardwareType == HardwareType.GameBoyAdvance)
				AF = 0x11B0;
			else
				AF = 0x01B0;

			if (bus.HardwareType == HardwareType.GameBoyAdvance)
				BC = 0x0113;
			else
				BC = 0x0013;

			DE = 0x00D8;
			HL = 0x014D;
			SP = 0xFFFE;
			PC = 0x0100;
		}

		public GameBoyMemoryBus Bus
		{
			get
			{
				return bus;
			}
		}

		public ProcessorStatus Status
		{
			get
			{
				return status;
			}
			set
			{
				status = value;
			}
		}

		#region 8 bit Registers

		public byte A { get { return a; } set { a = value; } }
		public byte F { get { return f; } set { f = (byte)(value & 0xF0); } }
		public byte B { get { return b; } set { b = value; } }
		public byte C { get { return c; } set { c = value; } }
		public byte D { get { return d; } set { d = value; } }
		public byte E { get { return e; } set { e = value; } }
		public byte H { get { return h; } set { h = value; } }
		public byte L { get { return l; } set { l = value; } }

		#endregion

		#region 16 bit Registers

		[CLSCompliant(false)]
		public ushort AF
		{
			get
			{
				return (ushort)((a << 8) | f);
			}
			set
			{
				a = (byte)(value >> 8);
				f = (byte)(value & 0xFF);
			}
		}

		[CLSCompliant(false)]
		public ushort BC
		{
			get
			{
				return (ushort)((b << 8) | c);
			}
			set
			{
				b = (byte)(value >> 8);
				c = (byte)(value & 0xFF);
			}
		}

		[CLSCompliant(false)]
		public ushort DE
		{
			get
			{
				return (ushort)((d << 8) | e);
			}
			set
			{
				d = (byte)(value >> 8);
				e = (byte)(value & 0xFF);
			}
		}

		[CLSCompliant(false)]
		public ushort HL
		{
			get
			{
				return (ushort)((h << 8) | l);
			}
			set
			{
				h = (byte)(value >> 8);
				l = (byte)(value & 0xFF);
			}
		}

		[CLSCompliant(false)]
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

		[CLSCompliant(false)]
		public ushort PC
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
					f &= 0x70;
			}
		}

		public bool NegationFlag
		{
			get
			{
				return (f & 0x40) != 0;
			}
			set
			{
				if (value)
					f |= 0x40;
				else
					f &= 0xB0;
			}
		}

		public bool HalfCarryFlag
		{
			get
			{
				return (f & 0x20) != 0;
			}
			set
			{
				if (value)
					f |= 0x20;
				else
					f &= 0xD0;
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


		public bool InterruptMasterEnable
		{
			get
			{
				return ime;
			}
			set
			{
				ime = value;
			}
		}

		#endregion
	}
}
