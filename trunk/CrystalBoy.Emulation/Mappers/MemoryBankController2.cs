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

namespace CrystalBoy.Emulation.Mappers
{
	public sealed class MemoryBankController2 : MemoryBankController
	{
		public MemoryBankController2(GameBoyMemoryBus bus)
			: base(bus) { }

		public override void HandleRomWrite(byte offsetLow, byte offsetHigh, byte value)
		{
			// The value of bit 8 must be 0 for enabling or disabling RAM, or 1 for changing ROM bank
			if (offsetHigh < 0x20)
			{
				if ((offsetHigh & 0x01) == 0)
				{
					value &= 0x0F; // Take only the 4 lower bits of the value

					RamEnabled = value == 0x0A; // Enable RAM only if value is 0x0A
				}
			}
			else if (offsetHigh < 0x40 && (offsetHigh & 0x01) != 0)
			{
				value &= 0x0F; // Take only the 4 lower bits of the value

				// Prevents the mapping of bank 0 in the 4000-7FFF area
				if (value == 0) value = 1;

				RomBank = value; // Switch the ROM bank
			}
		}
	}
}
