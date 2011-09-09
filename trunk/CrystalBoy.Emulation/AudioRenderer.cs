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
		private AudioBuffer audioBuffer;

		/// <summary>Initializes a new instance of the <see cref="AudioRenderer"/> class.</summary>
		public AudioRenderer() { Reset(); }

		~AudioRenderer() { Dispose(false); }

		public virtual void Reset() { }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual void Dispose(bool disposing) { }

		public Type SampleType { get { return audioBuffer != null ? audioBuffer.SampleType : null; } }

		public int BitsPerSample { get { return audioBuffer != null ? 8 * Marshal.SizeOf(SampleType) : 0; } }

		public int Frequency { get { return 44100; } }

		/// <summary>Gets or sets the audio buffer.</summary>
		/// <remarks>
		/// Changing the audio buffer is done in four steps:
		/// <list type="number">
		/// <item><description>Validate the proposed buffer by calling <see cref="IsAcceptableBuffer"/>.</description></item>
		/// <item><description>Prepare the change by calling <see cref="BeginBufferChange"/>.</description></item>
		/// <item><description>Actually change the buffer.</description></item>
		/// <item><description>Terminate the change by calling <see cref="EndBufferChange"/>.</description></item>
		/// </list>
		/// </remarks>
		/// <value>The audio buffer.</value>
		public AudioBuffer AudioBuffer
		{
			get { return audioBuffer; }
			set
			{
				if (value != audioBuffer)
				{
					if (!IsAcceptableBuffer(value))
						throw new ArgumentOutOfRangeException("value");

					BeginBufferChange();

					audioBuffer = value;

					EndBufferChange();
				}
			}
		}

		/// <summary>Gets the object associated with this renderer.</summary>
		/// <remarks>
		/// This object will typically be a window or a control.
		/// The implementation in <see cref="AudioRenderer"/> always returns <c>null</c>.
		/// </remarks>
		/// <value>The object associated with this renderer.</value>
		public object RenderObject { get { return GetRenderObject(); } }

		internal virtual object GetRenderObject() { return null; }

		/// <summary>This method is called before changing the buffer.</summary>
		/// <remarks>
		/// The default implementation does nothing.
		/// Override this method if you need to implement a specific behavior before changing the buffer.
		/// </remarks>
		protected virtual void BeginBufferChange() { }

		/// <summary>This method is called after changing the buffer.</summary>
		/// <remarks>
		/// The default implementation does nothing.
		/// Override this method if you need to implement a specific behavior after changing the buffer.
		/// </remarks>
		protected virtual void EndBufferChange() { }

		/// <summary>Determines whether the specified audio buffer is an acceptable buffer.</summary>
		/// <param name="audioBuffer">The proposed audio buffer.</param>
		/// <remarks>
		/// This method will consider <c>null</c> as a valid buffer, even though it is not a buffer.
		/// The following methods will be called to help determine if the buffer is acceptable:
		/// <list type="bullet">
		/// <item><description><see cref="IsSampleTypeSupported"/></description></item>
		/// </list>
		/// </remarks>
		/// <returns><c>true</c> if the specified audio buffer is acceptable; otherwise, <c>false</c>.</returns>
		public bool IsAcceptableBuffer(AudioBuffer audioBuffer)
		{
			if (audioBuffer == null) return true;

			return IsSampleTypeSupported(audioBuffer.SampleType) && IsFrequencySupported(44100);
		}

		public bool IsSampleTypeSupported(Type type)
		{
			// The tests here should be enough to eliminate most of the reference types and weird types.
			// The only remaining test to make is to try creating a pointer out of the type…
			return !(type.IsClass || type.IsInterface || type == typeof(void) || type.HasElementType || type.IsEnum || type.IsGenericParameter || type.ContainsGenericParameters)
				&& IsSampleTypeSupportedInternal(type);
		}

		protected virtual bool IsSampleTypeSupportedInternal(Type sampleType) { return true; }

		public bool IsFrequencySupported(int frequency) { return frequency % 60 == 0 && IsFrequencySupportedInternal(frequency); }

		protected virtual bool IsFrequencySupportedInternal(int frequency) { return true; }
	}

	public abstract class AudioRenderer<TRenderObject> : AudioRenderer
		where TRenderObject : class
	{
		TRenderObject renderObject;

		/// <summary>Initializes a new instance of the <see cref="AudioRenderer{TRenderObject}"/> class.</summary>
		/// <param name="renderObject">The object associated with this new renderer instance.</param>
		public AudioRenderer(TRenderObject renderObject)
		{
			if (renderObject == null)
				throw new ArgumentNullException("renderObject");
			this.renderObject = renderObject;
		}

		/// <summary>Gets the object associated with this renderer.</summary>
		/// <remarks>This object will typically be a window or a control, whose value will have been passed as a parameter in the consructor to <see cref="AudioRenderer{TRenderObject}"/></remarks>
		/// <value>The object associated with this renderer.</value>
		public new TRenderObject RenderObject { get { return renderObject; } }

		internal sealed override object GetRenderObject() { return renderObject; }
	}
}
