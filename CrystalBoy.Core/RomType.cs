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
using System.ComponentModel;

namespace CrystalBoy.Core
{
	public enum RomType : byte
	{
		[Description("ROM ONLY")]
		RomOnly = 0x00,

		[Description("ROM+MBC1")]
		RomMbc1 = 0x01,
		[Description("ROM+MBC1+RAM")]
		RomMbc1Ram = 0x02,
		[Description("ROM+MBC1+RAM+BATTERY")]
		RomMbc1RamBattery = 0x03,

		[Description("ROM+MBC2")]
		RomMbc2 = 0x05,
		[Description("ROM+MBC2+BATTERY")]
		RomMbc2Battery = 0x06,

		[Description("ROM+RAM")]
		RomRam = 0x08,
		[Description("ROM+RAM+BATTERY")]
		RomRamBattery = 0x09,

		[Description("ROM+MM01")]
		RomMmm01 = 0x0B,
		[Description("ROM+MM01+RAM")]
		RomMmm01Ram = 0x0C,
		[Description("ROM+MM01+RAM+BATTERY")]
		RomMmm01RamBattery = 0x0D,

		[Description("ROM+MBC3+TIMER+BATTERY")]
		RomMbc3TimerBattery = 0x0F,
		[Description("ROM+MBC3+TIMER+RAM+BATTERY")]
		RomMbc3TimerRamBattery = 0x10,
		[Description("ROM+MBC3")]
		RomMbc3 = 0x11,
		[Description("ROM+MBC3+RAM")]
		RomMbc3Ram = 0x12,
		[Description("ROM+MBC3+RAM+BATTERY")]
		RomMbc3RamBattery = 0x13,

		[Description("ROM+MBC4")]
		RomMbc4 = 0x15,
		[Description("ROM+MBC4+RAM")]
		RomMbc4Ram = 0x16,
		[Description("ROM+MBC4+RAM+BATTERY")]
		RomMbc4RamBattery = 0x17,

		[Description("ROM+MBC5")]
		RomMbc5 = 0x19,
		[Description("ROM+MBC5+RAM")]
		RomMbc5Ram = 0x1A,
		[Description("ROM+MBC5+RAM+BATTERY")]
		RomMbc5RamBattery = 0x1B,
		[Description("ROM+MBC5+RUMBLE")]
		RomMbc5Rumble = 0x1C,
		[Description("ROM+MBC5+RUMBLE+RAM")]
		RomMbc5RumbleRam = 0x1D,
		[Description("ROM+MBC5+RUMBLE+RAM+BATTERY")]
		RomMbc5RumbleRamBattery = 0x1E,

		[Description("ROM+MBC6+RAM")]
		RomMbc6Ram = 0x20,

		[Description("ROM+MBC7+RAM+BATTERY")]
		RomMbc7RamBattery = 0x22,

		[Description("POCKET CAMERA")]
		PocketCamera = 0xFC,

		[Description("ROM+TAMA5")]
		Tama5 = 0xFD,

		[Description("ROM+HuC-3")]
		RomHuC3 = 0xFE,

		[Description("ROM+HuC-1+RAM+BATTERY")]
		RomHuC1RamBattery = 0xFF
	}
}
