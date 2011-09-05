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

namespace CrystalBoy.Emulation
{
	public abstract class AudioBuffer
	{
		internal AudioBuffer() { }

		public abstract int ChannelCount { get; }

		public abstract int Position { get; }

		public abstract int Length { get; }

		public abstract Type ElementType { get; }
	}

	public sealed class AudioBuffer<T> : AudioBuffer where T : struct
	{
		private T[] rawBuffer;
		private int position;

		public AudioBuffer(T[] buffer)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (rawBuffer.Length == 0 || (rawBuffer.Length & 1) != 0) throw new IndexOutOfRangeException();

			rawBuffer = buffer;
		}

		public void PutSample(T left, T right)
		{
			rawBuffer[position++] = left;
			rawBuffer[position++] = right;

			if (position == rawBuffer.Length) position = 0;
		}

		public override int Position { get { return position; } }

		public override int Length { get { return rawBuffer.Length; } }

		public T[] RawBuffer { get { return rawBuffer; } }

		public override Type ElementType { get { return typeof(T); } }

		internal void SetRawBuffer(T[] buffer) // This method allows to reuse the same object over and over again. That's all there is to it.
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (rawBuffer.Length == 0 || (rawBuffer.Length & 1) != 0) throw new IndexOutOfRangeException();

			position = 0;
			rawBuffer = buffer;
		}
	}
}
