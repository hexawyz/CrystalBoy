using System;

namespace CrystalBoy.Emulation
{
    public static class LookupTables
	{
		public static readonly uint[] StandardColorLookupTable32 = InitializeStandardColorLookupTable32();
		public static readonly ushort[] StandardColorLookupTable16 = InitializeStandardColorLookupTable16();

		public static readonly uint[] GrayPalette = BuildPalette();
		public static readonly ushort[] PaletteLookupTable, FlippedPaletteLookupTable;

		static LookupTables()
		{
			BuildPaletteLookupTables(out PaletteLookupTable, out FlippedPaletteLookupTable);
		}

		private static uint[] BuildPalette()
		{
			return new uint[] { 0xFFFFFFFF, 0xFFAAAAAA, 0xFF555555, 0xFF000000 };
		}

		private static void BuildPaletteLookupTables(out ushort[] paletteLookupTable, out ushort[] flippedPaletteLookupTable)
		{
			paletteLookupTable = new ushort[65536];
			flippedPaletteLookupTable = new ushort[65536];

			for (int i = 0; i < 65536; i++)
			{
				int src = i,
					dst1 = 0,
					dst2 = 0;

				for (int j = 0; j < 8; j++)
				{
					dst1 <<= 2;
					dst2 >>= 2;

					if ((src & 0x0001) != 0)
					{
						dst1 |= 0x0001;
						dst2 |= 0x4000;
					}
					if ((src & 0x0100) != 0)
					{
						dst1 |= 0x0002;
						dst2 |= 0x8000;
					}
					src >>= 1;
				}

				paletteLookupTable[i] = (ushort)dst1;
				flippedPaletteLookupTable[i] = (ushort)dst2;
			}
		}

		#region Color Conversion Tables Management

		#region Initialization

		// Static constructors should be avoided for performance, we use these method for table initialization instead

		private static ushort[] InitializeStandardColorLookupTable16()
		{
			ushort[] lookupTable = new ushort[65536];

			GetStandardColors16(lookupTable);

			return lookupTable;
		}

		private static uint[] InitializeStandardColorLookupTable32()
		{
			uint[] lookupTable = new uint[65536];

			GetStandardColors32(lookupTable);

			return lookupTable;
		}

		#endregion

		#region Standard Colors

		public static void GetStandardColors16(ushort[] lookupTable)
		{
			// This should give “High quality” color interpolation.
			// The missing green bit is set according to the green most significant bit, so that 0x00 is perfectly black and 0x1F is perfectly white.
			for (uint i = 0; i < lookupTable.Length; i++)
				lookupTable[i] = (ushort)((i << 1) & 0xFFC0 | ((i & 0x200) != 0 ? (ushort)0x20 : (ushort)0x00) | i & 0x1F);
		}

		public static unsafe void GetStandardColors32(uint[] lookupTable)
		{
			byte* values = stackalloc byte[0x20];

			for (int i = 0; i < 0x20; i++) values[i] = (byte)Math.Round(0xFF * i / (double)0x1F);

			for (uint i = 0; i < lookupTable.Length; i++)
			{
				uint r = values[i & 0x1F];
				uint g = values[(i >> 5) & 0x1F];
				uint b = values[(i >> 10) & 0x1F];

				lookupTable[i] = b | (g << 8) | (r << 16) | 0xFF000000U;
			}
		}

		#endregion

		#endregion
	}
}
