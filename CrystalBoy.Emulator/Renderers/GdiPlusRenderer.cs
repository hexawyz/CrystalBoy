using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CrystalBoy.Emulation;
using System.Threading.Tasks;
using System;
using System.Threading;
using CrystalBoy.Core;
using System.Diagnostics;

namespace CrystalBoy.Emulator.Renderers
{
	[DisplayName("GDI+")]
	public sealed class GdiPlusRenderer : IVideoRenderer
	{
		private static readonly Task CompletedTask = Task.FromResult(true);

		// We will limit the display of frames to 120FPS. Non displayed frames will still be computed, though.
		private static readonly long FrameTicks = Stopwatch.Frequency / 120;

		private readonly Control _renderTarget;
		private Bitmap _borderBitmap;
		private readonly TripleBufferingSystem<Bitmap> _screenBitmapTripleBufferingSystem;
		private readonly BitmapData _lockedScreenBitmapData;
		private readonly InterpolationMode _interpolationMode;
		private readonly SynchronizationContext _synchronizationContext;
		private readonly Stopwatch _stopwatch;

		public GdiPlusRenderer(Control renderTarget)
		{
			_renderTarget = renderTarget;
			_screenBitmapTripleBufferingSystem = new TripleBufferingSystem<Bitmap>(() => new Bitmap(160, 144, PixelFormat.Format32bppRgb));
			_borderBitmap = new Bitmap(256, 224, PixelFormat.Format32bppPArgb);
			_lockedScreenBitmapData = new BitmapData();
			_stopwatch = Stopwatch.StartNew();

			_synchronizationContext = renderTarget.InvokeRequired ?
				(SynchronizationContext)renderTarget.Invoke((Func<SynchronizationContext>)(() => SynchronizationContext.Current)) :
				SynchronizationContext.Current;
		}

		public void Dispose()
		{
			_borderBitmap.Dispose();
			_screenBitmapTripleBufferingSystem.Dispose();
		}

		private void Render(object state)
		{
			if (!_renderTarget.IsDisposed && _stopwatch.ElapsedTicks >= FrameTicks)
			{
				_stopwatch.Restart();

				Bitmap bitmap;

				_screenBitmapTripleBufferingSystem.TryGetNextConsumerBuffer(out bitmap);

				// Calls to GDI+ should be thread-safe for the most part, so we only have to take a little bit of care for ensuring correctness.
				// CreateGraphics is one of the few thread-safe members of Control. :)
				Graphics g = _renderTarget.CreateGraphics();

				g.CompositingMode = CompositingMode.SourceCopy;
				g.CompositingQuality = CompositingQuality.HighSpeed;
				g.PixelOffsetMode = PixelOffsetMode.Half;
				g.SmoothingMode = SmoothingMode.None;
				g.InterpolationMode = InterpolationMode.NearestNeighbor;
				g.DrawImage(bitmap, _renderTarget.ClientRectangle);

				g.Dispose();
			}

			var tcs = state as TaskCompletionSource<bool>;

			if (tcs != null)
			{
				if (_renderTarget.IsDisposed) tcs.TrySetCanceled();
				else tcs.TrySetResult(true);
			}
		}

		public Task RenderFrameAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var bitmap = _screenBitmapTripleBufferingSystem.GetCurrentProducerBuffer();

			// Calls to GDI+ should be thread-safe for the most part, so we only have to take a little bit of care for ensuring correctness.
			bitmap.LockBits(new Rectangle(0, 0, 160, 144), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb, _lockedScreenBitmapData);

			renderer.RenderVideoFrame32(frame, _lockedScreenBitmapData.Scan0, _lockedScreenBitmapData.Stride);

			bitmap.UnlockBits(_lockedScreenBitmapData);

			_screenBitmapTripleBufferingSystem.GetNextProducerBuffer();

			cancellationToken.ThrowIfCancellationRequested();

			// Setting up a task completion source may not be needed here, but I don't know if there's something to gain by not doing so.
			if (_synchronizationContext != null)
			{
				var tcs = new TaskCompletionSource<bool>();
				_synchronizationContext.Post(Render, tcs);
				return tcs.Task;
			}
			else
			{
				Render(null);
				return CompletedTask;
			}
		}

		public Task RenderBorderAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}
	}
}
