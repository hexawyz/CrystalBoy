using System;
using System.Collections.Generic;
using CrystalBoy.Core;
using System.Threading.Tasks;
using System.Threading;

namespace CrystalBoy.Emulation
{
	internal sealed class VideoRenderingEngine : IDisposable
	{
		private readonly VideoFrameRenderer videoFrameRenderer = new VideoFrameRenderer();
		private readonly AsyncTripleBufferingSystem<VideoFrameData>.ConsumerSideBufferProvider bufferProvider;
		private readonly ThreadSynchronizationContext renderingContext;

		private IVideoRenderer videoRenderer;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		public event EventHandler BeforeRendering;
		public event EventHandler AfterRendering;

		public VideoRenderingEngine(AsyncTripleBufferingSystem<VideoFrameData>.ConsumerSideBufferProvider bufferProvider)
		{
			this.bufferProvider = bufferProvider;
			this.renderingContext = new ThreadSynchronizationContext(LoopRender, "Rendering Engine");
        }

		public void ResetRendering(bool colorHardware, bool useBootRom)
		{
            videoFrameRenderer.Reset(colorHardware, useBootRom);
		}

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public void Dispose()
		{
            cancellationTokenSource.Cancel();
            videoFrameRenderer.Dispose();
			renderingContext.Dispose();
		}
		
		public IVideoRenderer VideoRenderer
		{
			get { return videoRenderer; }
			set
			{
				videoRenderer = value;
				//ClearBuffer();
			}
		}

		private async Task LoopRender()
		{
			while (!cancellationTokenSource.IsCancellationRequested)
			{
				VideoFrameData frame;

                try
				{
					frame = await bufferProvider.SwapBuffersAsync(cancellationTokenSource.Token);
				}
				catch (OperationCanceledException)
				{
					break;
				}

				if (videoRenderer != null)
				{
					try
					{
						await videoRenderer.RenderFrameAsync(videoFrameRenderer, frame, cancellationTokenSource.Token);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
				}
			}
        }

		#region Event Handling

		private void OnBeforeRendering(EventArgs e)
		{
			BeforeRendering?.Invoke(this, e);
		}

		private void OnAfterRendering(EventArgs e)
		{
			AfterRendering?.Invoke(this, e);
		}

		#endregion
	}
}
