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

			internal RealTimeClockState(MemoryBankController3 mbc) { this.mbc = mbc; }

			public DateTime DateTime { get { return mbc.dateTime; } }

			public short Days { get { return mbc.rtcDays; } }

			public byte Hours { get { return mbc.rtcHours; } }

			public byte Minutes { get { return mbc.rtcMinutes; } }

			public byte Seconds { get { return mbc.rtcSeconds; } }

			public short LatchedDays { get { return mbc.latchedRtcDays; } }

			public byte LatchedHours { get { return mbc.latchedRtcHours; } }

			public byte LatchedMinutes { get { return mbc.latchedRtcMinutes; } }

			public byte LatchedSeconds { get { return mbc.latchedRtcSeconds; } }

			public bool Frozen { get { return mbc.rtcFreeze; } }
		}

		#endregion

		#region Variables

		RealTimeClockState rtcState;
		DateTime dateTime;
		short rtcDays;
		byte rtcSeconds, rtcMinutes, rtcHours;
		short latchedRtcDays;
		byte latchedRtcSeconds, latchedRtcMinutes, latchedRtcHours;
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
			dateTime = DateTime.UtcNow;
		}

		#endregion

		#region Properties

		public RealTimeClockState RtcState { get { return rtcState; } }

		public override int RamBank
		{
			get { return ramBankInternal; }
			protected set
			{
				if (value != ramBankInternal)
				{
					ramBankInternal = value;

					if (value < 0x4)
					{
						if (HandlesRamWrites) HandlesRamWrites = false;
						if (RamEnabled) MapRamBank(ramBankInternal);
					}
					else if (value >= 0x8 && value < 0xD)
					{
						if (RamEnabled) MapRtcRegister();
						if (!HandlesRamWrites) HandlesRamWrites = true;
					}
					else
					{
#if DEBUG
						throw new InvalidOperationException();
#else
						if (HandlesRamWrites) HandlesRamWrites = false;
						UnmapRam();
#endif
					}
				}
			}
		}

		public override bool RamEnabled
		{
			get { return ramEnabledInternal; }
			protected set
			{
				if (value != ramEnabledInternal)
				{
					if (ramEnabledInternal = value)
					{
						if (RamBank < 0x4) MapRamBank(RamBank);
						else if (RamBank >= 0x8 && RamBank < 0xD) MapRtcRegister();
					}
					else UnmapRam();
				}
			}
		}

		#endregion

		private void MapRtcRegister()
		{
			byte value;

			switch (RamBank)
			{
				case 8: value = latchedRtcSeconds; break;
				case 9: value = latchedRtcMinutes; break;
				case 10: value = latchedRtcHours; break;
				case 11: value = (byte)latchedRtcDays; break;
				case 12: value = (byte)(((latchedRtcDays & 0x100) != 0 ? 0x01 : 0x00) | ((latchedRtcDays & 0x200) != 0 ? 0x80 : 0x00) | (rtcFreeze ? 0x40 : 0x00)); break;
				default: throw new InvalidOperationException();
			}

			SetPortValue(value);
			MapPort();
		}

		private void AdjustRtc()
		{
			TimeSpan timeSpan = DateTime.UtcNow - dateTime;

			rtcSeconds = (byte)(rtcSeconds + timeSpan.Seconds);
			rtcMinutes = (byte)(rtcMinutes + timeSpan.Minutes);
			rtcHours = (byte)(rtcHours + timeSpan.Hours);
			rtcDays = (short)(rtcDays + timeSpan.Days);

			// Adjusts the values if needed
			// (An overflow might have happened when we added the time span values…)
			if (rtcSeconds > 60)
			{
				rtcSeconds -= 60;
				rtcMinutes++;
			}
			if (rtcMinutes > 60)
			{
				rtcMinutes -= 60;
				rtcHours++;
			}
			if (rtcHours > 24)
			{
				rtcHours -= 24;
				rtcDays++;
			}
			dateTime = DateTime.UtcNow;
		}

		private void LatchRtcRegisters()
		{
			if (!rtcFreeze)
			{
				TimeSpan timeSpan = DateTime.UtcNow - dateTime;

				latchedRtcSeconds = (byte)(rtcSeconds + timeSpan.Seconds);
				latchedRtcMinutes = (byte)(rtcMinutes + timeSpan.Minutes);
				latchedRtcHours = (byte)(rtcHours + timeSpan.Hours);
				latchedRtcDays = (short)(rtcDays + timeSpan.Days);

				// Adjusts the latched values if needed
				// (An overflow might have happened when we added the time span values…)
				if (latchedRtcSeconds > 60)
				{
					latchedRtcSeconds -= 60;
					latchedRtcMinutes++;
				}
				if (latchedRtcMinutes > 60)
				{
					latchedRtcMinutes -= 60;
					latchedRtcHours++;
				}
				if (latchedRtcHours > 24)
				{
					latchedRtcHours -= 24;
					rtcDays++;
				}
			}
			else
			{
				latchedRtcSeconds = rtcSeconds;
				latchedRtcMinutes = rtcMinutes;
				latchedRtcHours = rtcHours;
				latchedRtcDays = rtcDays;
			}

			if (RamEnabled && RamBank >= 8) MapRtcRegister();
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
				if (value == 0) latchBegin = true;
				else if (latchBegin && value == 1)
				{
					LatchRtcRegisters();
					latchBegin = false;
				}
				else latchBegin = false;
			}
		}

		public override void HandleRamWrite(byte offsetLow, byte offsetHigh, byte value)
		{
			byte portValue;

			if (!RamEnabled) return;

			if (rtcFreeze) dateTime = DateTime.UtcNow;

			switch (RamBank)
			{
				case 8:
					if (!rtcFreeze) AdjustRtc();
					portValue = rtcSeconds = (byte)(value % 60);
					dateTime = DateTime.UtcNow;
					break;
				case 9:
					if (!rtcFreeze) AdjustRtc();
					portValue = rtcMinutes = (byte)(value % 60);
					dateTime = DateTime.UtcNow;
					break;
				case 10:
					if (!rtcFreeze) AdjustRtc();
					portValue = rtcHours = (byte)(value % 24);
					dateTime = DateTime.UtcNow;
					break;
				case 11:
					if (!rtcFreeze) AdjustRtc();
					rtcDays = (short)(rtcDays & 0xFF00 | (portValue = value));
					dateTime = DateTime.UtcNow;
					break;
				case 12:
					if (rtcFreeze = (value & 0x40) != 0) AdjustRtc();
					else dateTime = DateTime.UtcNow;
					rtcDays = (short)(rtcDays & 0xFF | ((value & 0x1) != 0 ? 0x100 : 0) | ((value & 0x80) != 0 ? 0x200 : 0));
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
