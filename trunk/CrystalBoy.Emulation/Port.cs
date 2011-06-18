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
		/// <summary>Joypad</summary>
		JOYP = 0x00,
		/// <summary>Serial Transfer Byte</summary>
		SB = 0x01,
		/// <summary>Serial Transfer Control</summary>
		SC = 0x02,
		/// <summary>Divider Register</summary>
		DIV = 0x04,
		/// <summary>Timer Counter</summary>
		TIMA = 0x05,
		/// <summary>Timer Modulo</summary>
		TMA = 0x06,
		/// <summary>Timer Control</summary>
		TAC = 0x07,
		/// <summary>Interrupt Flag</summary>
		IF = 0x0F,
		/// <summary></summary>
		NR10 = 0x10,
		/// <summary></summary>
		NR11 = 0x11,
		/// <summary></summary>
		NR12 = 0x12,
		/// <summary></summary>
		NR13 = 0x13,
		/// <summary></summary>
		NR14 = 0x14,
		/// <summary></summary>
		NR21 = 0x16,
		/// <summary></summary>
		NR22 = 0x17,
		/// <summary></summary>
		NR23 = 0x18,
		/// <summary></summary>
		NR24 = 0x19,
		/// <summary></summary>
		NR30 = 0x1A,
		/// <summary></summary>
		NR31 = 0x1B,
		/// <summary></summary>
		NR32 = 0x1C,
		/// <summary></summary>
		NR33 = 0x1D,
		/// <summary></summary>
		NR34 = 0x1E,
		/// <summary></summary>
		NR41 = 0x20,
		/// <summary></summary>
		NR42 = 0x21,
		/// <summary></summary>
		NR43 = 0x22,
		/// <summary></summary>
		NR44 = 0x23,
		/// <summary></summary>
		NR50 = 0x24,
		/// <summary></summary>
		NR51 = 0x25,
		/// <summary></summary>
		NR52 = 0x26,
		/// <summary>LCD Control</summary>
		LCDC = 0x40,
		/// <summary>LCD Status</summary>
		STAT = 0x41,
		/// <summary>Scroll Y</summary>
		SCY = 0x42,
		/// <summary>Scroll X</summary>
		SCX = 0x43,
		/// <summary>LCD Y Coordinate</summary>
		LY = 0x44,
		/// <summary>LY Compare</summary>
		LYC = 0x45,
		/// <summary>DMA Transfer and Start Address</summary>
		DMA = 0x46,
		/// <summary>BG Palette Data</summary>
		BGP = 0x47,
		/// <summary>OBJ Palette 0 Data</summary>
		OBP0 = 0x48,
		/// <summary>OBJ Palette 1 Data</summary>
		OBP1 = 0x49,
		/// <summary>Window Y Position</summary>
		WY = 0x4A,
		/// <summary>Window X Position minus 7</summary>
		WX = 0x4B,
		/// <summary>Prepare Speed Switch</summary>
		KEY1 = 0x4D,
		/// <summary>Video RAM Bank</summary>
		VBK = 0x4F,
		/// <summary>Bootstrap ROM Locking</summary>
		BLCK = 0x50, // Boot ROM Lock register (Also WRAM banking register ?)
		/// <summary>HBlank DMA Source, High Byte</summary>
		HDMA1 = 0x51,
		/// <summary>HBlank DMA Source, Low Byte</summary>
		HDMA2 = 0x52,
		/// <summary>HBlank DMA Destination, High Byte</summary>
		HDMA3 = 0x53,
		/// <summary>HBlank DMA Destination, Low Byte</summary>
		HDMA4 = 0x54,
		/// <summary>HBlank DMA Control</summary>
		HDMA5 = 0x55,
		/// <summary>Infrared Communications Port</summary>
		RP = 0x56,
		/// <summary>Background Palette Index</summary>
		BCPS = 0x68,
		/// <summary>Background Palette Data</summary>
		BCPD = 0x69,
		/// <summary>Object Palette Index</summary>
		OCPS = 0x6A,
		/// <summary>Object Palette Data</summary>
		OCPD = 0x6B,
		/// <summary></summary>
		PMAP = 0x6C, // Palette Mapping register
		/// <summary>WRAM  Bank</summary>
		SVBK = 0x70,
		/// <summary></summary>
		U72 = 0x72,
		/// <summary></summary>
		U73 = 0x73,
		/// <summary></summary>
		U74 = 0x74,
		/// <summary></summary>
		U75 = 0x75,
		/// <summary></summary>
		U76 = 0x76,
		/// <summary></summary>
		U77 = 0x77,
		/// <summary>Interrupt Enable</summary>
		IE = 0xFF
	}
}
