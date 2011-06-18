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
	public static class PaletteData
	{
		private static readonly byte[] hashData;
		private static readonly short[] dictionaryData;

		// Using a static constructor means that the initialization will not be lazy… :(
		// Fortunately enough, this will mostly only be used in the constructor of RomInformation, so it should be OK with performance.
		static PaletteData()
		{
			byte[] rawData = LoadPaletteData();

			dictionaryData = LoadPaletteDictionary(rawData);
			hashData = new byte[202];
			Buffer.BlockCopy(rawData, 0, hashData, 0, 202);
		}

		private static byte[] LoadPaletteData()
		{
			using (var stream = typeof(PaletteData).Assembly.GetManifestResourceStream(typeof(PaletteData), "PaletteData"))
			{
				byte[] data = new byte[stream.Length];

				stream.Read(data, 0, data.Length);

				return data;
			}
		}

		private static short[] LoadPaletteDictionary(byte[] rawData)
		{
			byte[] buffer = new byte[90]; // Should be 96 bytes long but with the last 6 bytes at 0...
			short[] paletteDictionary = new short[6 * 32 * 3 * 4]; // 6 buckets of 32 sets of 3 palettes of 4 colors… Or just 192 sets of 3 palettes of 4 colors.
			int n;

			// The dictionary has been farctioned in 6 parts, probably for reducing memory usage on GB.
			// It doen't matter to us, and we can just assume it is just a dictionay of 192 items.
			// The dictionary is filled with all the 192 palettes in 6 steps.
			for (int i = 0; i < 6; i++)
			{
				n = 202; // Serves as a base offset in the raw palette data array

				// Fill the index buffer that will be used to fill the dictionary.
				// The data will be taken from a special index table in the raw palette data, permuting various 4 color palettes according to the value of i…
				for (int j = 0; j < buffer.Length; )
				{
					buffer[j++] = (i & 0x1) != 0 ? rawData[n] : rawData[n + 2];

					if ((i & 0x4) != 0)
					{
						buffer[j++] = rawData[++n];
						n++;
					}
					else if ((i & 0x2) != 0)
					{
						buffer[j++] = rawData[n];
						n += 2;
					}
					else buffer[j++] = rawData[n += 2];

					buffer[j++] = rawData[n++];
				}

				n = i * (32 * 3 * 4); // Serves as a base offset in the palette dictionary array

				// Fill the real dictionary with palette data
				// Each byte in the buffer now represents the offset to a palette in the palette raw data.
				// A palette is 8 bytes long, or 4 words long. (One 15 bit word for each color)
				// Quite logically, the palette data is stored in Z80 byte order.
				for (int j = 0; j < 96; j++)
				{
					int offset = 289 + (j < 90 ? buffer[j] : 0);

					for (int k = 0; k < 4; k++) // Read the 4 words, incrementing the offset each time
						paletteDictionary[n++] = (short)(rawData[offset++] | (rawData[offset++] << 8));
				}
			}

			return paletteDictionary;
		}

		/// <summary>Gets the offset to the specified palette in the dictionary data.</summary>
		/// <remarks>
		/// This method does not check its input because it is internal.
		/// Callers must ensure that <c>index</c> is always less than 192, otherwise an invalid offset will be generated.
		/// </remarks>
		/// <param name="index">The index of the palette.</param>
		/// <returns>Offset to the specified palette in the dictionary data.</returns>
		private static int GetPaletteOffset(byte index) { return index * (3 * 4); }

		/// <summary>Finds the index of the palette corresponding to the specified nintendo game.</summary>
		/// <remarks>
		/// This methods finds the palette index the same way the GBC BIOS would have.
		/// In fact, only Nintendo games have an automatically affected palette.
		/// </remarks>
		/// <param name="makerCode">The official maker code of the game.</param>
		/// <param name="checksum">The checksum of the game's title's.</param>
		/// <param name="complement">The fourth byte of the game's title.</param>
		/// <returns>Index of the palette to use for the game.</returns>
		internal static byte FindPaletteIndex(string makerCode, byte checksum, byte complement)
		{
			int v = 0;

			if (makerCode == "01")
				for (int i = 0; i < 79; i++)
					if (hashData[i] == checksum)
					{
						if (i < 65) v = i;
						else for (int j = i - 65; j < 94; j += 14)
							if (hashData[79 + j] == complement) v = 65 + j;

						break;
					}

			return hashData[108 + v];
		}

		/// <summary>Gets the data for the palette with the specified index.</summary>
		/// <param name="index">The requested palette index.</param>
		/// <returns>Palette data.</returns>
		public static short[] GetPaletteData(byte index)
		{
			if (index > 192) throw new ArgumentOutOfRangeException("index");

			short[] palette = new short[12];
			int offset = GetPaletteOffset(index);

			for (int i = 0; i < palette.Length; i++)
				palette[i] = dictionaryData[offset + i];

			return palette;
		}

		/// <summary>Gets the palette with the specified index.</summary>
		/// <param name="index">The requested palette index.</param>
		/// <returns>A <see cref="FixedColorPalette"/> structure representing the palette.</returns>
		public static FixedColorPalette GetPalette(byte index)
		{
			if (index > 192) throw new ArgumentOutOfRangeException("index");

			return new FixedColorPalette(dictionaryData, GetPaletteOffset(index));
		}
	}
}
