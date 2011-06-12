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
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	internal delegate void MemoryWriteHandler(byte offsetLow, byte offsetHigh, byte value);

	partial class GameBoyMemoryBus : IMemoryBus
	{
		#region Variables

		MemoryBlock externalRomBlock;
		MemoryBlock externalRamBlock;
		MemoryBlock videoMemoryBlock;
		MemoryBlock workMemoryBlock;
		MemoryBlock paletteMemoryBlock;
		MemoryBlock objectAttributeMemoryBlock;
		MemoryBlock portMemoryBlock;
		MemoryBlock segmentMemoryBlock;
		MemoryBlock generalMemoryBlock;
		MemoryBlock dmgBootMemoryBlock;
		MemoryBlock sgbBootMemoryBlock;
		MemoryBlock cgbBootMemoryBlock;

		MemoryWriteHandler[] segmentWriteHandlerArray;
		unsafe byte** segmentArray;
		unsafe byte* paletteMemory;
		unsafe byte* portMemory;
		unsafe byte* videoMemory;
		unsafe byte* workMemory;
		unsafe byte* objectAttributeMemory;
		unsafe byte* trashMemory;
		unsafe byte* externalPortMemory;
		unsafe byte* dmgBootMemory;
		unsafe byte* sgbBootMemory;
		unsafe byte* cgbBootMemory;

		bool internalRomMapped;

		int lowerRomBank, upperRombank, ramBank, videoRamBank, workRamBank;

		#endregion

		#region Initialize

		partial void InitializeMemory()
		{
			unsafe
			{
				segmentWriteHandlerArray = new MemoryWriteHandler[256];

				segmentMemoryBlock = new MemoryBlock(256 * sizeof(byte*)); // Allocate a memory segment table (256 segments)
				segmentArray = (byte**)segmentMemoryBlock.Pointer;

				externalRamBlock = new MemoryBlock(131072); // 128Kb maximum

				videoMemoryBlock = new MemoryBlock(16384); // 8Kb banks (only in CGB mode)
				videoMemory = (byte*)videoMemoryBlock.Pointer;

				workMemoryBlock = new MemoryBlock(32768); // 4Kb banks (switchable in CGB mode)
				workMemory = (byte*)workMemoryBlock.Pointer;

				objectAttributeMemoryBlock = new MemoryBlock(256); // 256 bytes of OAM
				objectAttributeMemory = (byte*)objectAttributeMemoryBlock.Pointer;

				portMemoryBlock = new MemoryBlock(256); // 256 bytes of High RAM
				portMemory = (byte*)portMemoryBlock.Pointer;

				paletteMemoryBlock = new MemoryBlock(16 * 4 * sizeof(ushort)); // 128 bytes of palette ram (only for CGB)
				paletteMemory = (byte*)paletteMemoryBlock.Pointer;

				generalMemoryBlock = new MemoryBlock(512); // 256 bytes of register memory and 256 bytes of 'trash' memory
				externalPortMemory = (byte*)generalMemoryBlock.Pointer;
				trashMemory = (byte*)generalMemoryBlock.Pointer + 256;

				dmgBootMemoryBlock = new MemoryBlock(0x100);
				dmgBootMemory = (byte*)dmgBootMemoryBlock.Pointer;
				sgbBootMemoryBlock = new MemoryBlock(0x100);
				sgbBootMemory = (byte*)sgbBootMemoryBlock.Pointer;
				cgbBootMemoryBlock = new MemoryBlock(0x800);
				cgbBootMemory = (byte*)cgbBootMemoryBlock.Pointer;
			}

			ResetSegments();
			ResetWriteHandlers();
		}

		#endregion

		#region Reset

		partial void ResetMemory()
		{
			// Reallocate the RAM block if the Mapper request more memory than currently allocated
			if (mapper != null && mapper.RamSize > externalRamBlock.Length)
			{
				externalRamBlock.Dispose();
				externalRamBlock = new MemoryBlock(mapper.RamSize);
			}

			unsafe { MemoryBlock.Set((void*)videoMemory, 0, (uint)videoMemoryBlock.Length); }

			videoRamBank = 0;
			workRamBank = 1;
			internalRomMapped = false;

			ResetSegments();
			ResetWriteHandlers();
		}

		private unsafe void ResetSegments()
		{
			UnmapExternalRom();
			UnmapExternalRam();
			MapVideoRamBank();
			MapStaticWorkRamBank();
			MapWorkRamBank();
			if (useBootRom) MapInternalRom();
			segmentArray[0xFE] = (byte*)objectAttributeMemoryBlock.Pointer;
			segmentArray[0xFF] = portMemory;
		}

		private void ResetWriteHandlers()
		{
			ResetRomWriteHandler();
			ResetRamWriteHandler();
			segmentWriteHandlerArray[0xFF] = WritePort;
		}

		private void ResetRomWriteHandler()
		{
			MemoryWriteHandler handler = mapper != null ? new MemoryWriteHandler(mapper.HandleRomWrite) : null;

			for (int i = 0x00; i < 0x80; i++)
				segmentWriteHandlerArray[i] = handler;
		}

		internal void ResetRamWriteHandler()
		{
			MemoryWriteHandler handler = mapper != null ? mapper.RamWriteHandler : null;

			for (int i = 0xA0; i < 0xC0; i++)
				segmentWriteHandlerArray[i] = handler;
		}

		#endregion

		#region Dispose

		partial void DisposeMemory()
		{
			paletteMemoryBlock.Dispose();
			externalRamBlock.Dispose();
			videoMemoryBlock.Dispose();
			workMemoryBlock.Dispose();
			segmentMemoryBlock.Dispose();
		}

		#endregion

		#region Memory Blocks

		public MemoryBlock ExternalRom { get { return externalRomBlock; } }

		public MemoryBlock ExternalRam { get { return externalRamBlock; } }

		public MemoryBlock VideoRam { get { return videoMemoryBlock; } }

		public MemoryBlock WorkRam { get { return workMemoryBlock; } }

		public MemoryBlock PaletteMemory { get { return paletteMemoryBlock; } }

		public MemoryBlock ObjectAttributeMemory { get { return objectAttributeMemoryBlock; } }

		#endregion 

		#region Memory Banks

		public int LowerExternalRomBank { get { return lowerRomBank; } }

		public int UpperExternalRomBank { get { return upperRombank; } }

		public int ExternalRamBank { get { return ramBank; } }

		public int VideoRamBank { get { return videoRamBank; } }

		public int WorkRamBank { get { return workRamBank; } }

		#endregion

		#region Memory Mapping Information

		[CLSCompliant(false)]
		public MemoryType GetMapping(ushort offset)
		{
			if (offset < 0x8000) return MemoryType.ExternalRom;
			else if (offset < 0xA000) return MemoryType.VideoRam;
			else if (offset < 0xC000) return MemoryType.ExternalRam;
			else if (offset < 0xFE00) return MemoryType.WorkRam;
			else if (offset < 0xFEA0) return MemoryType.Oam;
			else if (offset < 0xFF00) return MemoryType.Unknown;
			else if (offset < 0xFF80) return MemoryType.IoPorts;
			else if (offset < 0xFFFF) return MemoryType.HighRam;
			else return MemoryType.InterruptEnableRegister;
		}

		[CLSCompliant(false)]
		public MemoryType GetMapping(ushort offset, out int bank)
		{
			bank = 0;

			if (offset < 0x4000)
			{
				if (internalRomMapped && (offset < 0x100 || colorHardware && offset >= 0x200 && offset < 0x900))
					return MemoryType.InternalRom;
				if (lowerRomBank > 0)
				{
					bank = lowerRomBank;
					return MemoryType.ExternalRom;
				}
				else return MemoryType.Unknown;
			}
			else if (offset < 0x8000)
			{
				if (upperRombank > 0)
				{
					bank = upperRombank;
					return MemoryType.ExternalRom;
				}
				else return MemoryType.Unknown;
			}
			else if (offset < 0xA000)
			{
				bank = videoRamBank;
				return MemoryType.VideoRam;
			}
			else if (offset < 0xC000)
			{
				if (ramBank > 0)
				{
					bank = ramBank;
					return MemoryType.ExternalRam;
				}
				else return MemoryType.Unknown;
			}
			else if (offset < 0xD000)
				return MemoryType.WorkRam;
			else if (offset < 0xE000)
			{
				bank = workRamBank;
				return MemoryType.WorkRam;
			}
			else if (offset < 0xF000)
				return MemoryType.WorkRam;
			else if (offset < 0xFE00)
			{
				bank = workRamBank;
				return MemoryType.WorkRam;
			}
			else if (offset < 0xFEA0) return MemoryType.Oam;
			else if (offset < 0xFF00) return MemoryType.Unknown;
			else if (offset < 0xFF80) return MemoryType.IoPorts;
			else if (offset < 0xFFFF) return MemoryType.HighRam;
			else return MemoryType.InterruptEnableRegister;
		}

		#endregion

		#region Memory Accesses

		[CLSCompliant(false)]
		public byte this[ushort offset]
		{
			get { return ReadByte(offset); }
			set { WriteByte(offset, value); }
		}

		public byte this[byte offsetLow, byte offsetHigh]
		{
			get { return ReadByte(offsetLow, offsetHigh); }
			set { WriteByte(offsetLow, offsetHigh, value); }
		}

		[CLSCompliant(false)]
		public unsafe byte ReadByte(ushort offset)
		{
			return offset < 0xFF00 ? segmentArray[offset >> 8][offset & 0xFF] : ReadPort((byte)offset);
		}

		public unsafe byte ReadByte(byte offsetLow, byte offsetHigh)
		{
			return offsetHigh < 0xFF ? segmentArray[offsetHigh][offsetLow] : ReadPort(offsetLow);
		}

		[CLSCompliant(false)]
		public unsafe void WriteByte(ushort offset, byte value)
		{
			MemoryWriteHandler handler = segmentWriteHandlerArray[offset >> 8];

			if (handler != null) handler((byte)offset, (byte)(offset >> 8), value);
			else segmentArray[offset >> 8][offset & 0xFF] = value;
		}

		public unsafe void WriteByte(byte offsetLow, byte offsetHigh, byte value)
		{
			MemoryWriteHandler handler = segmentWriteHandlerArray[offsetHigh];

			if (handler != null) handler(offsetLow, offsetHigh, value);
			else segmentArray[offsetHigh][offsetLow] = value;
		}

		internal unsafe void RamWritePassthrough(byte offsetLow, byte offsetHigh, byte value)
		{
			// Ensures the write only goes to the external RAM area…
			if (offsetHigh >= 0xA0 && offsetHigh < 0xC0)
				segmentArray[offsetHigh][offsetLow] = value;
		}

		private void WritePort(byte offsetLow, byte offsetHigh, byte value) { WritePort(offsetLow, value); }

		#region DMA

		private void HandleDma(byte destinationHigh, byte destinationLow, byte sourceHigh, byte sourceLow, byte length)
		{
			int fullLength = (length + 1) << 4;
			int dh = destinationHigh,
				dl = destinationLow,
				sh = sourceHigh,
				sl = sourceLow;

			while (fullLength > 0)
			{
				int max = 256 - (dl > sl ? dl : sl);

				if (fullLength < max) max = fullLength;

				unsafe { MemoryBlock.Copy(segmentArray[dh] + dl, segmentArray[sh] + sl, max); }

				fullLength -= max;
				sl += max;
				dl += max;

				if (sl > 0xFF)
				{
					sl &= 0xFF;
					sh++;
				}

				if (dl > 0xFF)
				{
					dl &= 0xFF;
					dh++;
				}
			}

			cycleCount += (length + 1) << 3;
		}

		private unsafe void HandleHdma(bool addCycles)
		{
			int copyCount = (cycleCount - hdmaNextCycle) / HorizontalLineDuration;

			if (copyCount > hdmaCurrentLength)
				copyCount = hdmaCurrentLength;

			while (copyCount-- >= 0)
			{
				if (hdmaCurrentDestinationLow > 0xF0 || hdmaCurrentSourceLow > 0xF0)
				{
					uint length, remaining;

					length = 0x100 - Math.Max((uint)hdmaCurrentDestinationLow, (uint)hdmaCurrentSourceLow);

					MemoryBlock.Copy(segmentArray[hdmaCurrentDestinationHigh] + hdmaCurrentDestinationLow, segmentArray[hdmaCurrentSourceHigh] + hdmaCurrentSourceLow, length);

					if ((hdmaCurrentDestinationLow += (byte)length) == 0) hdmaCurrentDestinationHigh++;
					if ((hdmaCurrentSourceLow += (byte)length) == 0) hdmaCurrentSourceHigh++;
					remaining = 16 - length;

					length = Math.Min(16 - length, 0x100 - Math.Max((uint)hdmaCurrentDestinationLow, (uint)hdmaCurrentSourceLow));

					MemoryBlock.Copy(segmentArray[hdmaCurrentDestinationHigh] + hdmaCurrentDestinationLow, segmentArray[hdmaCurrentSourceHigh] + hdmaCurrentSourceLow, length);

					if ((hdmaCurrentDestinationLow += (byte)length) == 0) hdmaCurrentDestinationHigh++;
					if ((hdmaCurrentSourceLow += (byte)length) == 0) hdmaCurrentSourceHigh++;
					if ((remaining -= length) != 0)
					{
						MemoryBlock.Copy(segmentArray[hdmaCurrentDestinationHigh] + hdmaCurrentDestinationLow, segmentArray[hdmaCurrentSourceHigh] + hdmaCurrentSourceLow, remaining);
						hdmaCurrentDestinationLow += (byte)remaining;
						hdmaCurrentSourceLow += (byte)remaining;
					}
				}
				else
				{
					MemoryBlock.Copy(segmentArray[hdmaCurrentDestinationHigh] + hdmaCurrentDestinationLow, segmentArray[hdmaCurrentSourceHigh] + hdmaCurrentSourceLow, 16);

					if ((hdmaCurrentDestinationLow += 16 )== 0) hdmaCurrentDestinationHigh++;
					if ((hdmaCurrentSourceLow += 16) == 0) hdmaCurrentSourceHigh++;
				}
				if (addCycles)
					cycleCount += 8;
				hdmaNextCycle += HorizontalLineDuration;
				hdmaCurrentLength--;
			}

			hdmaActive = hdmaCurrentLength != 0xFF;

			// Adjusts the next cycle (skips VBlank)
			if (hdmaNextCycle >= FrameDuration - VerticalBlankDuration)
				hdmaNextCycle = FrameDuration + Mode2Duration + Mode3Duration;
		}

		#endregion

		#endregion

		#region Banking

		#region Internal ROM / Bootstrap ROM

		private unsafe void MapInternalRom()
		{
			internalRomMapped = true;
			switch (HardwareType)
			{
				case HardwareType.GameBoy:
				case HardwareType.GameBoyPocket:
					segmentArray[0] = dmgBootMemory;
					break;
				case HardwareType.SuperGameBoy:
					segmentArray[0] = sgbBootMemory;
					break;
				case HardwareType.GameBoyColor:
					segmentArray[0] = cgbBootMemory;
					for (int i = 0; i < 8; i++)
						segmentArray[i + 2] = cgbBootMemory + ((i + 1) << 8);
					break;
				default:
					this.useBootRom = false;
					break;
			}
		}

		private unsafe void UnmapInternalRom()
		{
			internalRomMapped = false;
			if (lowerRomBank >= 0) MapExternalRomBank(false, lowerRomBank);
			else UnmapExternalRom();
		}

		#endregion

		#region External ROM

		internal unsafe void UnmapExternalRom()
		{
			byte** segment = segmentArray;

			lowerRomBank = -1;
			upperRombank = -1;
			if (internalRomMapped)
				for (int i = 0; i < 0x80; i++)
				{
					if (i >= 0x1 && (!colorHardware || i < 0x2 || i >= 0x9))
						*segment = trashMemory;
					segment++;
				}
			else
				for (int i = 0x80; i != 0; i--)
					*segment++ = trashMemory;
		}

		internal unsafe void MapExternalRomBank(bool upper, int bankIndex)
		{
			int offset = bankIndex * 0x4000;

			if (ExternalRom.Length < offset + 0x4000) throw new InvalidOperationException("Unhandled ROM mapping case");

			byte* source = (byte*)externalRomBlock.Pointer + offset;
			byte** segment;

			if (!upper)
			{
				segment = segmentArray;
				lowerRomBank = bankIndex;
			}
			else
			{
				segment = segmentArray + 0x40;
				upperRombank = bankIndex;
			}

			if (!upper && internalRomMapped)
				for (int i = 0; i < 0x40; i++, source += 256)
				{
					if (i >= 0x1 && (!colorHardware || i < 0x2 || i >= 0x9))
						*segment = source;
					segment++;
				}
			else
				for (int i = 0x40; i != 0; i--, source += 256)
					*segment++ = source;
		}

		#endregion

		#region External RAM

		internal unsafe void UnmapExternalRam()
		{
			for (int i = 0xA0; i < 0xC0; i++)
				segmentArray[i] = trashMemory;

			ramBank = -1;
		}

		internal unsafe void MapExternalRamBank(int bankIndex)
		{
			byte* source = (byte*)externalRamBlock.Pointer + (bankIndex & 0x0F) * 0x2000;
			byte** segment = segmentArray + 0xA0;

			for (int i = 0x20; i != 0; i--, source += 256)
				*segment++ = source;

			ramBank = bankIndex;
		}

		#region Memory Mapped Port

		internal unsafe void MapExternalPort()
		{
			for (int i = 0xA0; i < 0xC0; i++)
				segmentArray[i] = externalPortMemory;

			ramBank = -1;
		}

		internal unsafe void SetPortValue(byte value)
		{
			MemoryBlock.Set(externalPortMemory, value, 256);
		}

		#endregion

		#endregion

		#region Video RAM

		private unsafe void MapVideoRamBank()
		{
			byte* offset = (byte*)videoMemoryBlock.Pointer + videoRamBank * 0x2000;

			for (int i = 0x80; i < 0xA0; i++, offset += 256)
				segmentArray[i] = offset;
		}

		#endregion

		#region Work RAM

		private unsafe void MapStaticWorkRamBank()
		{
			byte* source = (byte*)workMemoryBlock.Pointer;

			for (int i = 0xC0; i < 0xD0; i++, source += 256)
			{
				segmentArray[i] = source;
				segmentArray[i + 0x20] = source;
			}
		}

		private unsafe void MapWorkRamBank()
		{
			byte* source = (byte*)workMemoryBlock.Pointer + workRamBank * 0x1000;

			for (int i = 0xD0; i < 0xE0; i++, source += 256)
			{
				segmentArray[i] = source;
				if (i <= 0xDE) // Echo memory
					segmentArray[i + 0x20] = source;
			}
		}

		#endregion

		#endregion
	}
}
