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
using System.Collections.Generic;
using System.Text;
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	public class VideoStatusSnapshot
	{
		private GameBoyMemoryBus bus;
		private MemoryBlock paletteMemoryBlock;

		public unsafe VideoStatusSnapshot(GameBoyMemoryBus bus)
		{
			this.bus = bus;
			this.paletteMemoryBlock = new MemoryBlock(bus.PaletteMemory.Length);
			this.paletteMemory = (byte*)this.paletteMemoryBlock.Pointer;
		}

		public void Capture()
		{
			LCDC = bus.ReadPort(Port.LCDC);
			SCX = bus.ReadPort(Port.SCX);
			SCY = bus.ReadPort(Port.SCY);
			WX = bus.ReadPort(Port.WX);
			WY = bus.ReadPort(Port.WY);
			if (!bus.ColorMode)
			{
				BGP = bus.ReadPort(Port.BGP);
				OBP0 = bus.ReadPort(Port.OBP0);
				OBP1 = bus.ReadPort(Port.OBP1);
			}
			else unsafe
			{
				Memory.Copy((void*)paletteMemory, bus.PaletteMemory.Pointer, (uint)bus.PaletteMemory.Length);
			}
		}

		public byte LCDC;
		public byte SCX;
		public byte SCY;
		public byte WX;
		public byte WY;
		public byte BGP;
		public byte OBP0;
		public byte OBP1;
		public unsafe readonly byte* paletteMemory;
	}
}
