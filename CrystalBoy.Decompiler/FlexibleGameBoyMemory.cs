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
using CrystalBoy.Core;

namespace CrystalBoy.Decompiler
{
	class FlexibleGameBoyMemory : IMemory
	{
		MemoryBlock externalRom;
		byte externalRomBank;

		public FlexibleGameBoyMemory(MemoryBlock externalRom)
		{
			this.externalRom = externalRom;
			this.externalRomBank = 1;
		}

		public byte this[ushort offset]
		{
			get
			{
				return ReadByte(offset);
			}
			set
			{
				WriteByte(offset, value);
			}
		}

		public byte ReadByte(ushort offset)
		{
			if (offset < 0x4000)
				return externalRom[offset];
			else if (offset < 0x8000)
				return externalRom[(externalRomBank << 14) | offset & 0x3FFF];
			else
				return 0;
		}

		public void WriteByte(ushort offset, byte value)
		{
		}


		public byte this[byte offsetHigh, byte offsetLow]
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public byte ReadByte(byte offsetHigh, byte offsetLow)
		{
			throw new NotImplementedException();
		}

		public void WriteByte(byte offsetHigh, byte offsetLow, byte value)
		{
			throw new NotImplementedException();
		}
	}
}
