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
using CrystalBoy.Emulation.Windows.Forms;

namespace CrystalBoy.Emulator.Renderers
{
	[DisplayName("GDI+")]
	public sealed class GdiPlusRenderer : ControlVideoRenderer
	{
		private static readonly Task CompletedTask = Task.FromResult(true);

		// We will limit the display of frames to 120FPS. Non displayed frames will still be computed, though.
		private static readonly long FrameTicks = Stopwatch.Frequency / 120;
		
		private Bitmap _borderBitmap;
		private readonly TripleBufferingSystem<Bitmap> _screenBitmapTripleBufferingSystem;
		private readonly BitmapData _lockedScreenBitmapData;
		private readonly InterpolationMode _interpolationMode;
		private readonly Stopwatch _stopwatch;

		public GdiPlusRenderer(Control renderControl)
			: base(renderControl)
		{
			_screenBitmapTripleBufferingSystem = new TripleBufferingSystem<Bitmap>(() => new Bitmap(160, 144, PixelFormat.Format32bppRgb));
			_borderBitmap = new Bitmap(256, 224, PixelFormat.Format32bppPArgb);
			_lockedScreenBitmapData = new BitmapData();
			_stopwatch = Stopwatch.StartNew();
		}

		public void Dispose()
		{
			_borderBitmap.Dispose();
			_screenBitmapTripleBufferingSystem.Dispose();
		}

		private void Render(object state)
		{
			if (!RenderControl.IsDisposed && _stopwatch.ElapsedTicks >= FrameTicks)
			{
				_stopwatch.Restart();

				Bitmap bitmap;

				_screenBitmapTripleBufferingSystem.TryGetNextConsumerBuffer(out bitmap);

				// Calls to GDI+ should be thread-safe for the most part, so we only have to take a little bit of care for ensuring correctness.
				// CreateGraphics is one of the few thread-safe members of Control. :)
				using (Graphics g = RenderControl.CreateGraphics())
				{
					g.CompositingMode = CompositingMode.SourceCopy;
					g.CompositingQuality = CompositingQuality.HighSpeed;
					g.PixelOffsetMode = PixelOffsetMode.Half;
					g.SmoothingMode = SmoothingMode.None;
					g.InterpolationMode = InterpolationMode.NearestNeighbor;
					g.DrawImage(bitmap, RenderControl.ClientRectangle);
				}
			}

			var tcs = state as TaskCompletionSource<bool>;

			if (tcs != null)
			{
				if (RenderControl.IsDisposed) tcs.TrySetCanceled();
				else tcs.TrySetResult(true);
			}
		}

		public override Task RenderFrameAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested) return TaskHelper.CanceledTask;

			var bitmap = _screenBitmapTripleBufferingSystem.GetCurrentProducerBuffer();

			// Calls to GDI+ should be thread-safe for the most part, so we only have to take a little bit of care for ensuring correctness.
			bitmap.LockBits(new Rectangle(0, 0, 160, 144), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb, _lockedScreenBitmapData);

			renderer.RenderVideoFrame32(frame, _lockedScreenBitmapData.Scan0, _lockedScreenBitmapData.Stride);

			bitmap.UnlockBits(_lockedScreenBitmapData);

			_screenBitmapTripleBufferingSystem.GetNextProducerBuffer();

			if (cancellationToken.IsCancellationRequested) return TaskHelper.CanceledTask;

			// Setting up a task completion source may not be needed here, but I don't know if there's something to gain by not doing so.
			if (SynchronizationContext != null)
			{
				var tcs = new TaskCompletionSource<bool>();
				SynchronizationContext.Post(Render, tcs);
				return tcs.Task;
			}
			else
			{
				Render(null);
				return CompletedTask;
			}
		}

		public override Task RenderBorderAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}
	}
}
