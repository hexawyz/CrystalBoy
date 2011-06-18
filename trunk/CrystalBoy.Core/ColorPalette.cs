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

namespace CrystalBoy.Core
{
	public struct ColorPalette
	{
		private short[] data;
		private int offset;

		/// <summary>Initializes a new instance of the <see cref="ColorPalette"/> struct with the specified data and offset.</summary>
		/// <remarks>A Game Boy palette is composed of 4 words (15 bits, not 16 bits), any parameter set specifying less than 4 values will not be considered as valid.</remarks>
		/// <param name="data">The palette data.</param>
		/// <param name="offset">The offset at which this palette starts in the data array.</param>
		/// <exception cref="ArgumentNullException">The data array is <c>null</c>.</exception>
		/// <exception cref="IndexOutOfRangeException">The specified offset is out of range.</exception>
		public ColorPalette(short[] data, int offset)
		{
			if (data == null) throw new ArgumentNullException("data");
			if (offset < 0 || offset + 3 >= data.Length) throw new IndexOutOfRangeException();

			this.data = data;
			this.offset = offset;
		}

		/// <summary>Creates a <see cref="ColorPalette"/> using the specified data and offset.</summary>
		/// <remarks>
		/// This implementation is used internally by <see cref="FixedCOlorPalette"/> and assumes that the provided data is always valid.
		/// This method does not throw any exception.
		/// </remarks>
		/// <param name="data">The palette data.</param>
		/// <param name="offset">The offset at which this palette starts in the data array.</param>
		/// <returns>ColorPalette referring to the specified data.</returns>
		internal static ColorPalette Create(short[] data, int offset) { return new ColorPalette() { data = data, offset = offset }; }

		/// <summary>Gets the palette value at the specified index.</summary>
		/// <value>15 bit color value.</value>
		/// <exception cref="IndexOutOfRangeException">The specified index is out of range.</exception>
		public short this[int index]
		{
			get
			{
				if (index < 0 || index > 3) throw new IndexOutOfRangeException();

				return data[offset + index];
			}
		}
	}
}
