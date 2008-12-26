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

namespace CrystalBoy.Emulation.Mappers
{
	sealed class MemoryBankController3 : MemoryBankController
	{
		#region RealTimeClockState Class

		public sealed class RealTimeClockState
		{
			MemoryBankController3 mbc;

			internal RealTimeClockState(MemoryBankController3 mbc)
			{
				this.mbc = mbc;
			}

			public DateTime DateTime
			{
				get
				{
					return mbc.dateTime;
				}
			}

			public int Days
			{
				get
				{
					return mbc.rtcDays;
				}
			}

			public int Hours
			{
				get
				{
					return mbc.rtcHours;
				}
			}

			public int Minutes
			{
				get
				{
					return mbc.rtcMinutes;
				}
			}

			public int Seconds
			{
				get
				{
					return mbc.rtcSeconds;
				}
			}

			public bool Frozen
			{
				get
				{
					return mbc.rtcFreeze;
				}
			}
		}

		#endregion

		#region Variables

		RealTimeClockState rtcState;
		DateTime dateTime;
		short rtcDays;
		byte rtcSeconds, rtcMinutes, rtcHours;
		bool rtcFreeze, latchBegin;

		#endregion

		#region Constructor

		public MemoryBankController3(GameBoyMemoryBus bus)
			: base(bus)
		{
			rtcState = new RealTimeClockState(this);
			rtcSeconds = 0;
			rtcMinutes = 0;
			rtcHours = 0;
			dateTime = DateTime.Now;
		}

		#endregion

		#region Properties

		public RealTimeClockState RtcState
		{
			get
			{
				return rtcState;
			}
		}

		public override int RamBank
		{
			get
			{
				return ramBankInternal;
			}
			protected set
			{
				if (value != ramBankInternal)
				{
					ramBankInternal = value;

					if (value < 0x4)
					{
						if (HandlesRamWrites)
							HandlesRamWrites = false;
						if (RamEnabled)
							MapRamBank(ramBankInternal);
					}
					else if (value >= 0x8 && value < 0xD)
					{
						if (RamEnabled)
							MapRtcRegister();
						if (!HandlesRamWrites)
							HandlesRamWrites = true;
					}
					else
					{
#if DEBUG
						throw new InvalidOperationException();
#else
						if (HandlesRamWrites)
							HandlesRamWrites = false;
						UnmapRam();
#endif
					}
				}
			}
		}

		public override bool RamEnabled
		{
			get
			{
				return ramEnabledInternal;
			}
			protected set
			{
				if (value != ramEnabledInternal)
				{
					if (ramEnabledInternal = value)
					{
						if (RamBank < 0x4)
							MapRamBank(RamBank);
						else if (RamBank >= 0x8 && RamBank < 0xD)
							MapRtcRegister();
					}
					else
						UnmapRam();
				}
			}
		}

		#endregion

		public override void Reset()
		{
			base.Reset();
			HandlesRamWrites = false;
		}

		private void MapRtcRegister()
		{
			byte value;

			switch (RamBank)
			{
				case 8: value = rtcSeconds; break;
				case 9: value = rtcMinutes; break;
				case 10: value = rtcHours; break;
				case 11: value = (byte)rtcDays; break;
				case 12: value = (byte)(((rtcDays & 0x100) != 0 ? 0x01 : 0x00) | ((rtcDays & 0x200) != 0 ? 0x80 : 0x00) | (rtcFreeze ? 0x40 : 0x00)); break;
				default: throw new InvalidOperationException();
			}

			SetPortValue(value);
			MapPort();
		}

		private void LatchRtcRegisters()
		{
			if (!rtcFreeze)
			{
				TimeSpan timeSpan = DateTime.Now - dateTime;

				rtcSeconds = (byte)((rtcSeconds + timeSpan.Seconds) % 60);
				rtcMinutes = (byte)((rtcMinutes + timeSpan.Minutes) % 60);
				rtcHours = (byte)((rtcHours + timeSpan.Hours) % 24);
				rtcDays = (short)(rtcDays + timeSpan.Days);

				dateTime = DateTime.Now;
			}

			if (RamEnabled && RamBank >= 8)
				MapRtcRegister();
		}

		public override void HandleRomWrite(byte offsetLow, byte offsetHigh, byte value)
		{
			if (offsetHigh < 0x20)
			{
				value &= 0x0F; // Take only the 4 lower bits of the value

				RamEnabled = value == 0x0A; // Enable RAM only if value is 0x0A
			}
			else if (offsetHigh < 0x40)
			{
				value &= 0x7F; // Take only the 7 lower bits of the value

				// Prevents the mapping of bank 0 in the 4000-7FFF area
				if (value == 0)
					value = 1;

				// Update the rom bank
				RomBank = value;
			}
			else if (offsetHigh < 0x60)
			{
				RamBank = (byte)(value & 0xF);
			}
			else /* if (offsetHigh < 0x80) */
			{
				if (value == 0)
					latchBegin = true;
				else if (latchBegin && value == 1)
				{
					LatchRtcRegisters();
					latchBegin = false;
				}
				else
					latchBegin = false;
			}
		}

		public override void HandleRamWrite(byte offsetLow, byte offsetHigh, byte value)
		{
			byte portValue;

			if (!RamEnabled)
				return;

			if (rtcFreeze)
				dateTime = DateTime.Now;

			switch (RamBank)
			{
				case 8:
					portValue = rtcSeconds = (byte)(value % 60);
					break;
				case 9:
					portValue = rtcMinutes = (byte)(value % 60);
					break;
				case 10:
					portValue = rtcHours = (byte)(value % 24);
					break;
				case 11:
					rtcDays = (short)(rtcDays & 0xFF00 | value);
					portValue = value;
					break;
				case 12:
					rtcDays = (short)(rtcDays & 0xFF | ((value & 0x1) != 0 ? 0x100 : 0) | ((value & 0x80) != 0 ? 0x200 : 0));
					rtcFreeze = (value & 0x40) != 0;
					portValue = (byte)(value & 0xC1);
					break;
				default:
					// This should never happen. RAM writes handling should be disabled when RTC is not mapped.
					throw new InvalidOperationException("Bug in the MBC3 emulation");
			}

			SetPortValue(portValue);
		}
	}
}
