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
		private readonly GameBoyMemoryBus _bus;
		private readonly MemoryBlock _videoMemoryBlock;
		private readonly MemoryBlock _objectAttributeMemoryBlock;
		private readonly MemoryBlock _paletteMemoryBlock;

		public unsafe VideoMemorySnapshot(GameBoyMemoryBus bus)
		{
			_bus = bus;
			_videoMemoryBlock = new MemoryBlock(16384);
			VideoMemory = (byte*)this._videoMemoryBlock.Pointer;
			_objectAttributeMemoryBlock = new MemoryBlock(0xA0); // Do not allocate more bytes than needed… GameBoyMemoryBus allocates 0x100 because of the segmented memory.
			ObjectAttributeMemory = (byte*)_objectAttributeMemoryBlock.Pointer;
			_paletteMemoryBlock = new MemoryBlock(16 * 4 * sizeof(ushort));
			PaletteMemory = (byte*)this._paletteMemoryBlock.Pointer;
		}

		public void Dispose()
		{
			_videoMemoryBlock.Dispose();
			_objectAttributeMemoryBlock.Dispose();
			_paletteMemoryBlock.Dispose();
		}

		public void Capture(bool afterVideoEnable)
		{
			LCDC = _bus.ReadPort(Port.LCDC);
			SCX = _bus.ReadPort(Port.SCX);
			SCY = _bus.ReadPort(Port.SCY);
			WX = _bus.ReadPort(Port.WX);
			WY = _bus.ReadPort(Port.WY);
			unsafe
			{
				if (!_bus.ColorMode)
				{
					BGP = _bus.ReadPort(Port.BGP);
					OBP0 = _bus.ReadPort(Port.OBP0);
					OBP1 = _bus.ReadPort(Port.OBP1);
					MemoryBlock.Copy((void*)VideoMemory, _bus.VideoRam.Pointer, _bus.VideoRam.Length >> 1);
					if (afterVideoEnable)
					{
						MemoryBlock.Copy((void*)PaletteMemory, _bus.PaletteMemory.Pointer, _bus.PaletteMemory.Length);
					}
				}
				else
				{
					MemoryBlock.Copy((void*)VideoMemory, _bus.VideoRam.Pointer, _bus.VideoRam.Length);
					MemoryBlock.Copy((void*)PaletteMemory, _bus.PaletteMemory.Pointer, _bus.PaletteMemory.Length);
				}
				MemoryBlock.Copy((void*)ObjectAttributeMemory, _bus.ObjectAttributeMemory.Pointer, _objectAttributeMemoryBlock.Length);
			}
			SuperGameBoyScreenStatus = _bus.SuperGameBoyScreenStatus;
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
