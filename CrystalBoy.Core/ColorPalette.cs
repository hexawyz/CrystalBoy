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
			if (data == null) throw new ArgumentNullException(nameof(data));
			if (offset < 0 || offset + 3 >= data.Length) throw new ArgumentOutOfRangeException(nameof(offset));

			this.data = data;
			this.offset = offset;
		}

		/// <summary>Creates a <see cref="ColorPalette"/> using the specified data and offset.</summary>
		/// <remarks>
		/// This implementation is used internally by <see cref="FixedColorPalette"/> and assumes that the provided data is always valid.
		/// This method does not throw any exception.
		/// </remarks>
		/// <param name="data">The palette data.</param>
		/// <param name="offset">The offset at which this palette starts in the data array.</param>
		/// <returns>ColorPalette referring to the specified data.</returns>
		internal static ColorPalette Create(short[] data, int offset) { return new ColorPalette() { data = data, offset = offset }; }

		/// <summary>Gets the palette value at the specified index.</summary>
		/// <value>15 bit color value.</value>
		/// <exception cref="IndexOutOfRangeException">The specified index is out of range.</exception>
		public short this[int index] => data[offset + index];
	}
}
