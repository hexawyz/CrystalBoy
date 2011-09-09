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
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace CrystalBoy.Emulation
{
	public abstract class AudioBuffer
	{
		internal AudioBuffer() { }

		/// <summary>Gets the current position in the raw buffer.</summary>
		/// <value>The position in the raw buffer.</value>
		public abstract int Position { get; }

		/// <summary>Gets the length of the raw buffer.</summary>
		/// <value>The length of the raw buffer.</value>
		public abstract int Length { get; }

		/// <summary>Gets the raw buffer currently in use.</summary>
		/// <value>The raw buffer.</value>
		public Array RawBuffer { get { return GetRawBuffer(); } }

		/// <summary>Gets the type of the samples.</summary>
		/// <value>The type of the samples.</value>
		public abstract Type SampleType { get; }

		/// <summary>Gets the number of bits per sample .</summary>
		/// <value>The number of bits per sample.</value>
		public int BitsPerSample { get { return 8 * Marshal.SizeOf(SampleType); } }

		/// <summary>Returns the current raw buffer.</summary>
		/// <remarks>
		/// Because we are working with generic buffers for greater flexibility,
		/// the non-generic <see cref="AudioBuffer"/> can only reference the base <see cref="Array"/> class.
		/// The raw buffer is stored and provided by the <see cref="AudioBuffer{TSample}" /> generic subclass.
		/// </remarks>
		/// <returns>An <see cref="Array"/> representing the buffer currently in use.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal abstract Array GetRawBuffer(); // We'd want this to be protected AND internal but it is not possible…
	}

	public sealed class AudioBuffer<TSample> : AudioBuffer
		where TSample : struct
	{
		private TSample[] rawBuffer;
		private int position;

		public AudioBuffer(TSample[] buffer)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (buffer.Length == 0 || (buffer.Length & 1) != 0) throw new IndexOutOfRangeException();

			rawBuffer = buffer;
		}

		public void PutSample(TSample left, TSample right)
		{
			rawBuffer[position++] = left;
			rawBuffer[position++] = right;

			if (position == rawBuffer.Length) position = 0;
		}

		/// <summary>
		/// Gets the current position in the raw buffer.
		/// </summary>
		/// <value>The position in the raw buffer.</value>
		public sealed override int Position { get { return position; } }

		/// <summary>Gets the length of the raw buffer.</summary>
		/// <value>The length of the raw buffer.</value>
		public sealed override int Length { get { return rawBuffer.Length; } }

		/// <summary>Gets the raw buffer currently in use.</summary>
		/// <value>The raw buffer.</value>
		public new TSample[] RawBuffer { get { return rawBuffer; } }

		/// <summary>Gets the type of the samples.</summary>
		/// <value>The type of the samples.</value>
		public sealed override Type SampleType { get { return typeof(TSample); } }

		/// <summary>Returns the current raw buffer.</summary>
		/// <returns>An <see cref="Array"/> representing the buffer currently in use.</returns>
		/// <remarks>
		/// Because we are working with generic buffers for greater flexibility,
		/// the non-generic <see cref="AudioBuffer"/> can only reference the base <see cref="Array"/> class.
		/// The raw buffer is stored and provided by the <see cref="AudioBuffer{TSample}"/> generic subclass.
		/// </remarks>
		internal sealed override Array GetRawBuffer() { return RawBuffer; }
	}
}
