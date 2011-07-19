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
	[CLSCompliant(false)]
	public unsafe abstract class VideoRenderer : IDisposable
	{
		private short clearColor = 0x7FFF; // Set this value to its default in order not to trigger the ClearColorChanged event inside the constructor.
		private bool interpolation;
		private bool borderVisible;

		public event EventHandler ClearColorChanged;
		public event EventHandler BorderVisibileChanged;
		public event EventHandler InterpolationChanged;

		public VideoRenderer() { Reset(); }

		public virtual void Reset() { ClearColor = 0x7FFF; }

		public abstract void Dispose();
		public abstract void* LockBorderBuffer(out int stride);
		public abstract void UnlockBorderBuffer();
		public abstract void* LockScreenBuffer(out int stride);
		public abstract void UnlockScreenBuffer();
		public abstract void Render();

		/// <summary>Gets a value indicating whether the video renderer supports interpolation.</summary>
		/// <remarks>
		/// The value returned by this property must be constant.
		/// The default implementation always returns <c>false</c>.
		/// Subclasses supporting interpolated rendering should override this property and always return <c>true</c>.
		/// </remarks>
		/// <value><c>true</c> if the video renderer supports interpolation; otherwise, <c>false</c>.</value>
		public virtual bool SupportsInterpolation { get { return false; } }

		public short ClearColor
		{
			get { return clearColor; }
			set { if (clearColor != (clearColor = (short)(value & 0x7FFF))) OnClearColorChanged(EventArgs.Empty); }
		}

		protected virtual void OnClearColorChanged(EventArgs e) { if (ClearColorChanged != null) ClearColorChanged(this, e); }

		public bool BorderVisible
		{
			get { return borderVisible; }
			set { if (borderVisible != (borderVisible = value)) OnBorderVisibleChanged(EventArgs.Empty); }
		}

		protected virtual void OnBorderVisibleChanged(EventArgs e) { if (BorderVisibileChanged != null) BorderVisibileChanged(this, e); }

		public bool Interpolation
		{
			get { return interpolation; }
			set { if (interpolation != (interpolation = value)) OnInterpolationChanged(EventArgs.Empty); }
		}

		protected virtual void OnInterpolationChanged(EventArgs e) { if (InterpolationChanged != null) InterpolationChanged(this, e); }
	}

	[CLSCompliant(false)]
	public abstract class VideoRenderer<T> : VideoRenderer
		where T: class
	{
		T renderObject;

		public VideoRenderer(T renderObject)
		{
			if (renderObject == null)
				throw new ArgumentNullException("renderObject");
			this.renderObject = renderObject;
		}

		public T RenderObject { get { return renderObject; } }
	}
}
