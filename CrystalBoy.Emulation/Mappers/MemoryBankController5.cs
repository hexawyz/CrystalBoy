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
	public sealed class MemoryBankController5 : MemoryBankController
	{
		bool rumble;

		public MemoryBankController5(GameBoyMemoryBus bus)
			: base(bus) { }

		public override void Reset()
		{
			base.Reset();
			rumble = false;
		}

		public bool Rumble
		{
			get { return rumble; }
			private set
			{
				rumble = value;

				if (RumbleChanged != null)
					RumbleChanged(this, new RumbleEventArgs(value));
			}
		}

		public event EventHandler<RumbleEventArgs> RumbleChanged;

		public override void HandleRomWrite(byte offsetLow, byte offsetHigh, byte value)
		{
			if (offsetHigh < 0x20)
			{
				value &= 0x0F; // Take only the 4 lower bits of the value

				RamEnabled = value == 0x0A; // Enable RAM only if value is 0x0A
			}
			else if (offsetHigh < 0x30)
			{
				// Update the 8 lower bits of rom bank
				RomBank = (byte)(RomBank & 0x100 | value);
			}
			else if (offsetHigh < 0x40)
			{
				// Update bit 8 of rom bank
				if ((value & 0x01) != 0) RomBank = (byte)(0x100 | RomBank & 0xFF);
				else RomBank &= 0xFF;
			}
			else if (offsetHigh < 0x60)
			{
				if (Bus.RomInformation.HasRumble)
				{
					RamBank = (byte)(value & 0x7);
					Rumble = (value & 0x8) != 0;
				}
				else RamBank = (byte)(value & 0xF);
			}
			else /* if (offsetHigh < 0x80) */
			{
			}
		}
	}
}
