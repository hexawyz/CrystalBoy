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
	public sealed class MemoryBankController3 : MemoryBankController
	{
		#region Variables

		RealTimeClockState rtcState;
		bool latchBegin;

		#endregion

		#region Constructor

		public MemoryBankController3(GameBoyMemoryBus bus)
			: base(bus)
		{
			rtcState = new RealTimeClockState();
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
						if (InterceptsRamWrites) InterceptsRamWrites = false;
						if (RamEnabled) MapRamBank(ramBankInternal);
					}
					else if (value >= 0x8 && value < 0xD)
					{
						if (RamEnabled) MapRtcRegister();
						if (!InterceptsRamWrites) InterceptsRamWrites = true;
					}
					else
					{
#if DEBUG
						throw new InvalidOperationException();
#else
						if (InterceptsRamWrites) InterceptsRamWrites = false;
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
				case 8: value = rtcState.LatchedSeconds; break;
				case 9: value = rtcState.LatchedMinutes; break;
				case 10: value = rtcState.LatchedHours; break;
				case 11: value = (byte)rtcState.LatchedDays; break;
				case 12: value = (byte)(((rtcState.LatchedDays & 0x100) != 0 ? 0x01 : 0x00) | ((rtcState.LatchedDays & 0x200) != 0 ? 0x80 : 0x00) | (rtcState.Frozen ? 0x40 : 0x00)); break;
				default: throw new InvalidOperationException();
			}

			SetPortValue(value);
			MapPort();
		}

		private void LatchRtcRegisters()
		{
			rtcState.Latch();

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

			switch (RamBank)
			{
				case 8:
					portValue = rtcState.Seconds = (byte)(value % 60);
					break;
				case 9:
					portValue = rtcState.Minutes = (byte)(value % 60);
					break;
				case 10:
					portValue = rtcState.Hours = (byte)(value % 24);
					break;
				case 11:
					rtcState.Days = (short)(rtcState.Days & 0xFF00 | (portValue = value));
					break;
				case 12:
					rtcState.Frozen = (value & 0x40) != 0;
					rtcState.Days = (short)(rtcState.Days & 0xFF | ((value & 0x1) != 0 ? 0x100 : 0) | ((value & 0x80) != 0 ? 0x200 : 0));
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
