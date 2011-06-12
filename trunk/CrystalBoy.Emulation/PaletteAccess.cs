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

namespace CrystalBoy.Emulation
{
	internal struct PaletteAccess
	{
		public PaletteAccess(int clock, byte offset, byte value)
		{
			this.Clock = clock;
			this.Offset = offset;
			this.Value = value;
		}

		/// <summary>Time in Clock Cycles (Since the last VBlank).</summary>
		public int Clock;
		/// <summary>Palette data offset.</summary>
		public byte Offset;
		/// <summary>Value written to the register.</summary>
		public byte Value;

		public override string ToString()
		{
			return "{PaletteAccess: Clock " + Clock + ", Offset 0x" + Offset.ToString("X2") + ", Value 0x" + Value.ToString("X2") + "}";
		}
	}
}
