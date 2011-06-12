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

namespace CrystalBoy.Emulation
{
	public enum Port : byte
	{
		JOYP = 0x00,
		SB = 0x01,
		SC = 0x02,
		DIV = 0x04,
		TIMA = 0x05,
		TMA = 0x06,
		TAC = 0x07,
		IF = 0x0F,
		NR10 = 0x10,
		NR11 = 0x11,
		NR12 = 0x12,
		NR13 = 0x13,
		NR14 = 0x14,
		NR21 = 0x16,
		NR22 = 0x17,
		NR23 = 0x18,
		NR24 = 0x19,
		NR30 = 0x1A,
		NR31 = 0x1B,
		NR32 = 0x1C,
		NR33 = 0x1D,
		NR34 = 0x1E,
		NR41 = 0x20,
		NR42 = 0x21,
		NR43 = 0x22,
		NR44 = 0x23,
		NR50 = 0x24,
		NR51 = 0x25,
		NR52 = 0x26,
		LCDC = 0x40,
		STAT = 0x41,
		SCY = 0x42,
		SCX = 0x43,
		LY = 0x44,
		LYC = 0x45,
		DMA = 0x46,
		BGP = 0x47,
		OBP0 = 0x48,
		OBP1 = 0x49,
		WY = 0x4A,
		WX = 0x4B,
		KEY1 = 0x4D,
		VBK = 0x4F,
		BLCK = 0x50, // Boot ROM Lock register (Also WRAM banking register ?)
		HDMA1 = 0x51,
		HDMA2 = 0x52,
		HDMA3 = 0x53,
		HDMA4 = 0x54,
		HDMA5 = 0x55,
		RP = 0x56,
		BCPS = 0x68,
		BCPD = 0x69,
		OCPS = 0x6A,
		OCPD = 0x6B,
		PMAP = 0x6C, // Palette Mapping register
		SVBK = 0x70,
		U72 = 0x72,
		U73 = 0x73,
		U74 = 0x74,
		U75 = 0x75,
		U76 = 0x76,
		U77 = 0x77,
		IE = 0xFF
	}
}
