namespace CrystalBoy.Emulation
{
    // It seems the CPU used in GB hardware is Sharp LR35902 (aka GB-Z80)
    public sealed partial class Processor
	{
		public const byte CFlag = 0x10;
		public const byte HFlag = 0x20;
		public const byte NFlag = 0x40;
		public const byte ZFlag = 0x80;
		public const byte NotCFlag = 0xE0;
		public const byte NotHFlag = 0xD0;
		public const byte NotNFlag = 0xB0;
		public const byte NotZFlag = 0x70;

		byte a, b, c, d, e, f, h, l;
		ushort sp, pc;
		bool ime;
		byte enableInterruptDelay;
		bool skipPcIncrement;
		GameBoyMemoryBus bus;
		ProcessorStatus status;

		internal Processor(GameBoyMemoryBus bus) { this.bus = bus; }

		internal void Reset()
		{
			// Processor starts at zero

			AF = 0x0000;
			BC = 0x0000;
			DE = 0x0000;
			HL = 0x0000;
			SP = 0x0000;
			PC = 0x0000;
			InterruptMasterEnable = false;

			status = ProcessorStatus.Running;
		}

		public GameBoyMemoryBus Bus { get { return bus; } }

		public ProcessorStatus Status { get { return status; } }

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

		public ushort AF
		{
			get { return (ushort)((a << 8) | f); }
			set
			{
				a = (byte)(value >> 8);
				f = (byte)(value & 0xFF);
			}
		}

		public ushort BC
		{
			get { return (ushort)((b << 8) | c); }
			set
			{
				b = (byte)(value >> 8);
				c = (byte)(value & 0xFF);
			}
		}

		public ushort DE
		{
			get { return (ushort)((d << 8) | e); }
			set
			{
				d = (byte)(value >> 8);
				e = (byte)(value & 0xFF);
			}
		}

		public ushort HL
		{
			get { return (ushort)((h << 8) | l); }
			set
			{
				h = (byte)(value >> 8);
				l = (byte)(value & 0xFF);
			}
		}

		public ushort SP
		{
			get { return sp; }
			set { sp = value; }
		}

		public ushort PC
		{
			get { return pc; }
			set { pc = value; }
		}

		#endregion

		#region Flags

		public bool ZeroFlag
		{
			get { return (f & ZFlag) != 0; }
			set
			{
				if (value) f |= ZFlag;
				else f &= NotZFlag;
			}
		}

		public bool NegationFlag
		{
			get { return (f & NFlag) != 0; }
			set
			{
				if (value) f |= NFlag;
				else f &= NotNFlag;
			}
		}

		public bool HalfCarryFlag
		{
			get { return (f & HFlag) != 0; }
			set
			{
				if (value) f |= HFlag;
				else f &= NotHFlag;
			}
		}

		public bool CarryFlag
		{
			get { return (f & CFlag) != 0; }
			set
			{
				if (value) f |= CFlag;
				else f &= NotCFlag;
			}
		}

		public bool InterruptMasterEnable
		{
			get { return ime; }
			set { ime = value; }
		}

		#endregion
	}
}
