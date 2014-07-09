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
using System.Threading;
using System.Threading.Tasks;

namespace CrystalBoy.Emulation
{
	/// <summary>Base class for a video renderer.</summary>
	/// <remarks>
	/// All video renderers must inherit from this class.
	/// Generally, video renderers will not inherit directly from this class,
	/// but will instead inherit from the generic class <see cref="VideoRenderer{TRenderObject}"/>,
	/// which provides strong coupling with a particular host.
	/// All members of this class should be callable from another thread.
	/// The implementor must take care of the synchrnonization where needed for its specific case.
	/// For the time being, implementors can safely assume that drawing operations will only be called sequentially, even while coming from non UI threads.
	/// I.e. While required to be thread-safe, the renderers do not need to be multithread-safe.
	/// </remarks>
	[CLSCompliant(false)]
	public unsafe abstract class VideoRenderer : IDisposable
	{
		private volatile short clearColor = 0x7FFF; // Set this value to its default in order not to trigger the ClearColorChanged event inside the constructor.
		private volatile bool interpolation;
		private volatile bool borderVisible;

		/// <summary>Occurs when the <see cref="ClearColor"/> property is changed.</summary>
		/// <remarks>This event may be raised on any thread. Handlers should handle the synchrnonization as needed.</remarks>
		public event EventHandler ClearColorChanged;
		/// <summary>Occurs when the <see cref="BorderVisible"/> property is changed.</summary>
		/// <remarks>This event may be raised on any thread. Handlers should handle the synchrnonization as needed.</remarks>
		public event EventHandler BorderVisibleChanged;
		/// <summary>Occurs when the <see cref="Interpolation"/> property is changed.</summary>
		/// <remarks>This event may be raised on any thread. Handlers should handle the synchrnonization as needed.</remarks>
		public event EventHandler InterpolationChanged;

		/// <summary>Initializes a new instance of the class <see cref="VideoRenderer"/>.</summary>
		public VideoRenderer() { Reset(); }

		/// <summary>Resets the video renderer.</summary>
		public virtual void Reset() { ClearColor = 0x7FFF; }

		/// <summary>Disposes the managed resources allocated by this instance.</summary>
		public abstract void Dispose();

		/// <summary>Locks the image buffer for the border, and returns a pointer to the updatable video memory.</summary>
		/// <remarks>
		/// The buffer returned should to be big enough to hold a 256x224 pixels image in the correct format. (Currently 32bpp)
		/// Failure to return a valid buffer will lead to random crashes of the application.
		/// </remarks>
		/// <returns>A reference to the locked buffer.</returns>
		public virtual VideoBufferReference LockBorderBuffer() { return LockBorderBufferAsync().Result; }
		/// <summary>Asynchronously lock the image buffer for the border, and returns a pointer to the updatable video memory.</summary>
		/// <remarks>
		/// The buffer returned should to be big enough to hold a 256x224 pixels image in the correct format. (Currently 32bpp)
		/// Failure to return a valid buffer will lead to random crashes of the application.
		/// </remarks>
		/// <returns>A task representing the asynchronous operation. The <see cref="Task{T}.Result"/> property will contain a reference to the locked buffer.</returns>
		public abstract Task<VideoBufferReference> LockBorderBufferAsync();
		/// <summary>Unlocks the image buffer for the border.</summary>
		public virtual void UnlockBorderBuffer() { UnlockBorderBufferAsync().Wait(); }
		/// <summary>Asynchronously unlock the image buffer for the border.</summary>
		public abstract Task UnlockBorderBufferAsync();

		/// <summary>Locks the image buffer for the screen, and returns a pointer to the updatable video memory.</summary>
		/// <remarks>
		/// The buffer returned should to be big enough to hold a 160x144 pixels image in the correct format. (Currently 32bpp)
		/// Failure to return a valid buffer will lead to random crashes of the application.
		/// </remarks>
		/// <returns>A reference to the locked buffer.</returns>
		public virtual VideoBufferReference LockScreenBuffer() { return LockScreenBufferAsync().Result; }
		/// <summary>Asynchronously lock the image buffer for the screen, and returns a pointer to the updatable video memory.</summary>
		/// <remarks>
		/// The buffer returned should be big enough to hold a 160x144 pixels image in the correct format. (Currently 32bpp)
		/// Failure to return a valid buffer will lead to random crashes of the application.
		/// </remarks>
		/// <returns>A task representing the asynchronous operation. The <see cref="Task{T}.Result"/> property will contain a reference to the locked buffer.</returns>
		public abstract Task<VideoBufferReference> LockScreenBufferAsync();
		/// <summary>Unlocks the image buffer for the screen.</summary>
		public virtual void UnlockScreenBuffer() { UnlockScreenBufferAsync().Wait(); }
		/// <summary>Asynchronously unlock the image buffer for the screen.</summary>
		public abstract Task UnlockScreenBufferAsync();

		/// <summary>Presents the image to the screen.</summary>
		/// <remarks>
		/// The buffers should have been released before calling this method.
		/// If not the case, the video renderer may legitimately crash, depending on the implementation.
		/// </remarks>
		public virtual void Render() { RenderAsync().Wait(); }
		/// <summary>Asynchronously present the image to the rendering device.</summary>
		/// <remarks>
		/// The buffers should have been released before calling this method.
		/// If not the case, the video renderer may legitimately crash, depending on the implementation.
		/// </remarks>
		public abstract Task RenderAsync();

		/// <summary>Gets a value indicating whether the video renderer supports interpolation.</summary>
		/// <remarks>
		/// The value returned by this property must be constant.
		/// The default implementation always returns <c>false</c>.
		/// Subclasses supporting interpolated rendering should override this property and always return <c>true</c>.
		/// </remarks>
		/// <value><c>true</c> if the video renderer supports interpolation; otherwise, <c>false</c>.</value>
		public virtual bool SupportsInterpolation { get { return false; } }

		/// <summary>Gets or sets the color used for clearing the render surface.</summary>
		public short ClearColor
		{
			get { return clearColor; }
			set
			{
				var c = this.clearColor;
				// The value of clearColor may change between those two lines, but in should not cause too much harm
				if (c != (this.clearColor = value))
					OnInterpolationChanged(EventArgs.Empty);
			}
		}

		/// <summary>Handles a change in the value of the property <see cref="ClearColor"/>.</summary>
		/// <param name="e">Data associated with the event.</param>
		protected virtual void OnClearColorChanged(EventArgs e)
		{
			var handle = ClearColorChanged;

			if (handle != null)
				handle(this, e);
		}

		/// <summary>Gets or sets a flag indicating whether the SGB border is visible.</summary>
		public bool BorderVisible
		{
			get { return borderVisible; }
			set
			{
				var b = this.borderVisible;
				// The value of borderVisible may change between those two lines, but in should not cause too much harm
				if (b != (this.borderVisible = value))
					OnBorderVisibleChanged(EventArgs.Empty);
			}
		}

		/// <summary>Handles a change in the value of the property <see cref="BorderVisible"/>.</summary>
		/// <param name="e">Data associated with the event.</param>
		protected virtual void OnBorderVisibleChanged(EventArgs e)
		{
			var handle = BorderVisibleChanged;

			if (handle != null)
				handle(this, e);
		}

		/// <summary>Gets or sets a flag indicating whether interpolation is enabled.</summary>
		public bool Interpolation
		{
			get { return interpolation; }
			set
			{
				var b = this.interpolation;
				// The value of interpolation may change between those two lines, but in should not cause too much harm
				if (b != (this.interpolation = value))
					OnInterpolationChanged(EventArgs.Empty);
			}
		}

		/// <summary>Handles a change in the value of the property <see cref="Interpolation"/>.</summary>
		/// <param name="e">Data associated with the event.</param>
		protected virtual void OnInterpolationChanged(EventArgs e)
		{
			var handle = InterpolationChanged;

			if (handle != null)
				handle(this, e);
		}

		/// <summary>Gets the render object associated with this instance.</summary>
		public object RenderObject { get { return GetRenderObject(); } }

		/// <summary>Gets the render object associated with this instance.</summary>
		/// <remarks>
		/// To allow for mor flexibility in design, the management of the render object is delegated to the derived class,
		/// and this method is used to access the render object in an universal manner.
		/// This method should be implemented by simply returning the private render object stored in the derived class.
		/// </remarks>
		/// <returns>The render object associated with this instance.</returns>
		protected virtual object GetRenderObject() { return null; }
	}

	/// <summary>This class is the base class for a video renderer supporting a specific type of render object.</summary>
	/// <remarks>
	/// The render object is a host specific object. Where host specific means that the type of object is related to the specific implementation of the emulator.
	/// Usually, this will be a graphic object specific to the graphics framework used.
	/// An emulator implemented with Windows Forms for the interface would use different plugins than an emulator implemented using DirectFB on linux, and this difference
	/// would be enforced by the specific type of <see cref="VideoRenderer"/> that the implementation looks up.
	/// </remarks>
	/// <typeparam name="TRenderObject">The type of render objects supported by the class.</typeparam>
	[CLSCompliant(false)]
	public abstract class VideoRenderer<TRenderObject> : VideoRenderer
		where TRenderObject : class
	{
		TRenderObject renderObject;

		/// <summary>Initializes a new instance of the class <see cref="VideoRenderer{TRenderObject}"/>.</summary>
		/// <param name="renderObject">The render object to which the new instance will be bound.</param>
		public VideoRenderer(TRenderObject renderObject)
		{
			if (renderObject == null)
				throw new ArgumentNullException("renderObject");
			this.renderObject = renderObject;
		}

		/// <summary>Gets the render object associated with this instance.</summary>
		/// <remarks>
		/// This property masks <see cref="VideoRenderer.RenderObject"/> from the base class while returning the exact same value.
		/// The only difference is that this property returns a value typed as <typeparamref name="TRenderObject"/>.
		/// </remarks>
		public new TRenderObject RenderObject { get { return renderObject; } }

		/// <summary>Gets the render object associated with this instance.</summary>
		/// <remarks>This method simply returns the render object stored in this instance.</remarks>
		/// <returns>The render object associated with this instance.</returns>
		protected sealed override object GetRenderObject() { return renderObject; }
	}
}
