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
		bool interpolation;

		public EventHandler InterpolationChanged;

		public abstract void Dispose();
		public abstract void* LockBuffer(out int stride);
		public abstract void UnlockBuffer();
		public abstract void Render();

		public virtual bool SupportsInterpolation { get { return true; } }

		protected virtual void OnInterpolationChanged(EventArgs e)
		{
			if (InterpolationChanged != null)
				InterpolationChanged(this, e);
		}

		public bool Interpolation
		{
			get { return interpolation; }
			set
			{
				if (SupportsInterpolation && value != interpolation) // Can have a weird behavior if SupportsInterpolation is dynamic, but I assume you know what you do if you make it dynamic...
				{
					interpolation = value;
					OnInterpolationChanged(EventArgs.Empty);
				}
			}
		}
	}

	[CLSCompliant(false)]
	public abstract class RenderMethod<T> : VideoRenderer
		where T: class
	{
		T renderObject;

		public RenderMethod(T renderObject)
		{
			if (renderObject == null)
				throw new ArgumentNullException("renderObject");
			this.renderObject = renderObject;
		}

		public T RenderObject { get { return renderObject; } }
	}
}
