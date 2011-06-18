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
	/// <summary>Represents a fixed color palette that can be used to color a monochrome Game Boy game.</summary>
	public struct FixedColorPalette
	{
		private short[] data;
		private int offset;

		/// <summary>Initializes a new instance of the <see cref="FixedColorPalette"/> struct.</summary>
		/// <remarks>
		/// The palette data is represented by 12 words, each representing one color.
		/// The data will be split into 3 palettes of 4 colors, used for object palette 0, object palette 1, and background.
		/// </remarks>
		/// <param name="data">An array of twelve <see cref="Int16"/> containing the palette data.</param>
		public FixedColorPalette(short[] data)
		{
			if (data == null) throw new ArgumentNullException("data");
			if (data.Length < 12) throw new IndexOutOfRangeException();

			this.data = data;
			this.offset = 0;
		}

		/// <summary>Initializes a new instance of the <see cref="FixedColorPalette"/> struct.</summary>
		/// <remarks>
		/// The palette data is represented by 12 words, each representing one color.
		/// The data will be split into 3 palettes of 4 colors, used for background, object palette 0, and object palette 1.
		/// </remarks>
		/// <param name="data">An array of at least twelve <see cref="Int16"/> containing the palette data.</param>
		/// <param name="offset">The offset of the first palette word.</param>
		public FixedColorPalette(short[] data, int offset)
		{
			if (data == null) throw new ArgumentNullException("data");
			if (offset < 0 || offset + 12 >= data.Length) throw new IndexOutOfRangeException();

			this.data = data;
			this.offset = offset;
		}

		/// <summary>Gets the background palette.</summary>
		/// <value>The background palette.</value>
		public ColorPalette BackgroundPalette { get { return new ColorPalette(data, offset + 8); } }
		/// <summary>Gets the object palette 0.</summary>
		/// <value>The object palette0.</value>
		public ColorPalette ObjectPalette0 { get { return new ColorPalette(data, offset); } }
		/// <summary>Gets the object palette 1.</summary>
		/// <value>The object palette1.</value>
		public ColorPalette ObjectPalette1 { get { return new ColorPalette(data, offset + 4); } }
	}
}
