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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CrystalBoy.Emulation
{
	public unsafe abstract class AudioRenderer : IDisposable
	{
		internal AudioRenderer() { Reset(); }

		public virtual void Reset() { }

		public abstract void Dispose();

		public abstract Type SampleType { get; }

		public int BitsPerSample { get { return 8 * Marshal.SizeOf(SampleType); } }

		public int Frequency { get { return 44100; } }

		public Array RawBuffer { get { return GetRawBuffer(); } set { SetRawBuffer(value); } }

		public abstract bool CanSetRawBuffer { get; }

		public object RenderObject { get { return GetRenderObject(); } }

		protected virtual object GetRenderObject() { return null; }

		// We'd want these to be protected AND internal but it is not possible…
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal abstract Array GetRawBuffer();
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal abstract void SetRawBuffer(Array value);
	}

	public unsafe abstract class AudioRenderer<TSample> : AudioRenderer
		where TSample : struct
	{
		public AudioRenderer() : base() { }

		public sealed override Type SampleType { get { return typeof(TSample); } }

		public new abstract TSample[] RawBuffer { get; set; }

		internal override Array GetRawBuffer() { return RawBuffer; }

		internal override void SetRawBuffer(Array value) { RawBuffer = (TSample[])value; } // Hint: We *WANT* this to throw an InvalidCastException
	}

	public abstract class AudioRenderer<TSample, TRenderObject> : AudioRenderer<TSample>
		where TSample : struct
		where TRenderObject : class
	{
		TRenderObject renderObject;

		public AudioRenderer(TRenderObject renderObject)
		{
			if (renderObject == null)
				throw new ArgumentNullException("renderObject");
			this.renderObject = renderObject;
		}

		public new TRenderObject RenderObject { get { return renderObject; } }

		protected sealed override object GetRenderObject() { return renderObject; }
	}
}
