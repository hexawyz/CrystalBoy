using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalBoy.Emulation
{
	/// <summary>Represents a video renderer.</summary>
	/// <remarks>
	/// Methods from this type can be called from any thread.
	/// They will, however, not be called more than once at a time.
	/// The implementor is responsible for implementing necessary marshalling of the call to a specific thread, if required.
	/// </remarks>
	public interface IVideoRenderer : IDisposable
	{
		/// <summary>Renders and present the specified video frame.</summary>
		/// <param name="renderer">The renderer to use for rendering the data.</param>
		/// <param name="frame">The frame to render.</param>
		/// <param name="cancellationToken">A token used to indicate cancellation of the operation.</param>
		/// <returns>A task representing the status of the operation.</returns>
		Task RenderFrameAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken);

		/// <summary>Renders and present the specified video frame.</summary>
		/// <param name="renderer">The renderer to use for rendering the data.</param>
		/// <param name="frame">The frame to render.</param>
		/// <param name="cancellationToken">A token used to indicate cancellation of the operation.</param>
		/// <returns>A task representing the status of the operation.</returns>
		Task RenderBorderAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken);
	}
}
