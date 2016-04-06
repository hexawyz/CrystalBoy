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
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	internal sealed class VideoMemorySnapshot : IDisposable
	{
		private readonly GameBoyMemoryBus bus;
		private readonly MemoryBlock videoMemoryBlock;
		private readonly MemoryBlock objectAttributeMemoryBlock;
		private readonly MemoryBlock paletteMemoryBlock;

		public unsafe VideoMemorySnapshot(GameBoyMemoryBus bus)
		{
			this.bus = bus;
			this.videoMemoryBlock = new MemoryBlock(16384);
			this.VideoMemory = (byte*)this.videoMemoryBlock.Pointer;
			this.objectAttributeMemoryBlock = new MemoryBlock(0xA0); // Do not allocate more bytes than needed… GameBoyMemoryBus allocates 0x100 because of the segmented memory.
			this.ObjectAttributeMemory = (byte*)this.objectAttributeMemoryBlock.Pointer;
			this.paletteMemoryBlock = new MemoryBlock(16 * 4 * sizeof(ushort));
			this.PaletteMemory = (byte*)this.paletteMemoryBlock.Pointer;
		}

		public void Dispose()
		{
			videoMemoryBlock.Dispose();
			objectAttributeMemoryBlock.Dispose();
			paletteMemoryBlock.Dispose();
		}

		public void Capture(bool afterVideoEnable)
		{
			LCDC = bus.ReadPort(Port.LCDC);
			SCX = bus.ReadPort(Port.SCX);
			SCY = bus.ReadPort(Port.SCY);
			WX = bus.ReadPort(Port.WX);
			WY = bus.ReadPort(Port.WY);
			unsafe
			{
				if (!bus.ColorMode)
				{
					BGP = bus.ReadPort(Port.BGP);
					OBP0 = bus.ReadPort(Port.OBP0);
					OBP1 = bus.ReadPort(Port.OBP1);
					MemoryBlock.Copy((void*)VideoMemory, bus.VideoRam.Pointer, bus.VideoRam.Length >> 1);
					if (afterVideoEnable)
					{
						MemoryBlock.Copy((void*)PaletteMemory, bus.PaletteMemory.Pointer, bus.PaletteMemory.Length);
					}
				}
				else
				{
					MemoryBlock.Copy((void*)VideoMemory, bus.VideoRam.Pointer, bus.VideoRam.Length);
					MemoryBlock.Copy((void*)PaletteMemory, bus.PaletteMemory.Pointer, bus.PaletteMemory.Length);
				}
				MemoryBlock.Copy((void*)ObjectAttributeMemory, bus.ObjectAttributeMemory.Pointer, objectAttributeMemoryBlock.Length);
			}
			SuperGameBoyScreenStatus = bus.SuperGameBoyScreenStatus;
		}

		public byte LCDC;
		public byte SCX;
		public byte SCY;
		public byte WX;
		public byte WY;
		public byte BGP;
		public byte OBP0;
		public byte OBP1;
		public byte SuperGameBoyScreenStatus;

		public unsafe readonly byte* PaletteMemory;
		public unsafe readonly byte* VideoMemory;
		public unsafe readonly byte* ObjectAttributeMemory;
	}
}
