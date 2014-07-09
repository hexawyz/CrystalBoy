using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrystalBoy.Emulation
{
	/// <summary>Represents a reference to a video buffer.</summary>
	public struct VideoBufferReference
	{
		/// <summary>Pointer to the image data.</summary>
		public readonly IntPtr DataPointer;
		/// <summary>The offset in bytes between two rows of pixels in the buffer.</summary>
		/// <remarks>This value may be negative.</remarks>
		public int Stride;

		public VideoBufferReference(IntPtr dataPointer, int stride)
		{
			this.DataPointer = dataPointer;
			this.Stride = stride;
		}
	}
}
