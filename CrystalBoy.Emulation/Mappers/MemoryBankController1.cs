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
	sealed class MemoryBankController1 : MemoryBankController
	{
		byte extraBits;
		bool bankedRamMode;

		public MemoryBankController1(GameBoyMemoryBus bus)
			: base(bus)
		{
			/* this.extraBits = 0; */
			/* this.bankedRamMode = false; */
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
				value &= 0x1F; // Take only the 5 lower bits of the value

				// Prevents the mapping of bank 0 in the 4000-7FFF area, but also prevents banks 0x20, 0x40 and 0x60 from being mapped
				// Since this is a known MBC1 bug, we reproduce it here
				if (value == 0)
					value = 1;

				// Update the 5 lower bits of rom bank
				RomBank = (byte)(RomBank & 0x60 | value);
			}
			else if (offsetHigh < 0x60)
			{
				extraBits = (byte)(value & 0x3); // Take the 2 lower bits of value for the extra bits

				// Meaning of the extra bits depends on the ram mode
				if (bankedRamMode) // Extra bits are a ram bank index if banked ram mode is active
					RamBank = extraBits;
				else // Otherwise they are the bits 5-6 of the rom bank index
					RomBank = (byte)(RomBank & 0x1F | (extraBits << 5));
			}
			else /* if (offsetHigh < 0x80) */ // offset should always be < 8000 when calling this function, we can safely skip the test
			{
				// The lower bit of value indicates wether to enable banked ram mode
				bankedRamMode = (value & 0x1) != 0;

				// Update the bank indices depending on the new ram mode
				if (bankedRamMode) // Extra bits are a ram bank index if banked ram mode is active
				{
					RomBank &= 0x1F;
					RamBank = extraBits;
				}
				else // Otherwise they are the bits 5-6 of the rom bank index
				{
					RomBank = (byte)(RomBank & 0x1F | (extraBits << 5));
					RamBank = 0;
				}
			}
		}
	}
}
