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

		private static readonly Rectangle _borderRectangle = new Rectangle(0, 0, 256, 224);
		private static readonly Rectangle _screenRectangle = new Rectangle(0, 0, 160, 144);
		private static readonly Rectangle _borderedScreenRectangle = new Rectangle(48, 40, 160, 144);
		
		private readonly TripleBufferingSystem<Bitmap> _borderBitmapTripleBufferingSystem;
		private readonly TripleBufferingSystem<Bitmap> _screenBitmapTripleBufferingSystem;
		private readonly Bitmap _compositeBitmap;
		private readonly BitmapData _lockedBorderBitmapData;
		private readonly BitmapData _lockedScreenBitmapData;
		private readonly InterpolationMode _interpolationMode;
		private readonly Stopwatch _stopwatch;

		public GdiPlusRenderer(Control renderControl)
			: base(renderControl)
		{
			_borderBitmapTripleBufferingSystem = new TripleBufferingSystem<Bitmap>(() => new Bitmap(256, 224, PixelFormat.Format32bppPArgb));
			_screenBitmapTripleBufferingSystem = new TripleBufferingSystem<Bitmap>(() => new Bitmap(160, 144, PixelFormat.Format32bppRgb));
			_compositeBitmap = new Bitmap(256, 224, PixelFormat.Format32bppPArgb);
			_lockedBorderBitmapData = new BitmapData();
			_lockedScreenBitmapData = new BitmapData();
			_stopwatch = Stopwatch.StartNew();
		}

		public override void Dispose()
		{
			_compositeBitmap.Dispose();
			_borderBitmapTripleBufferingSystem.Dispose();
			_screenBitmapTripleBufferingSystem.Dispose();
		}

		private void Render(object state)
		{
			if (!RenderControl.IsDisposed && _stopwatch.ElapsedTicks >= FrameTicks)
			{
				_stopwatch.Restart();

				Bitmap finalBitmpap;

				Bitmap screenBitmap;
				Bitmap borderBitmap;

				_screenBitmapTripleBufferingSystem.TryGetNextConsumerBuffer(out screenBitmap);

				if (BorderVisible)
				{
					_borderBitmapTripleBufferingSystem.TryGetNextConsumerBuffer(out borderBitmap);

					using (var g = Graphics.FromImage(_compositeBitmap))
					{
						g.DrawImage(screenBitmap, _borderedScreenRectangle, _screenRectangle, GraphicsUnit.Pixel);
						g.DrawImage(borderBitmap, _borderRectangle, _borderRectangle, GraphicsUnit.Pixel);
					}

					finalBitmpap = _compositeBitmap;
				}
				else
				{
					finalBitmpap = screenBitmap;
				}

				// Calls to GDI+ should be thread-safe for the most part, so we only have to take a little bit of care for ensuring correctness.
				// CreateGraphics is one of the few thread-safe members of Control. :)
				using (var g = RenderControl.CreateGraphics())
				{
					g.CompositingMode = CompositingMode.SourceCopy;
					g.CompositingQuality = CompositingQuality.HighSpeed;
					g.PixelOffsetMode = PixelOffsetMode.Half;
					g.SmoothingMode = SmoothingMode.None;
					g.InterpolationMode = InterpolationMode.NearestNeighbor;
					g.DrawImage(finalBitmpap, RenderControl.ClientRectangle);
				}
			}

			var tcs = state as TaskCompletionSource<bool>;

			if (tcs != null)
			{
				if (RenderControl.IsDisposed) tcs.TrySetCanceled();
				else tcs.TrySetResult(true);
			}
		}

		public override void Refresh()
		{
			Render(null);
		}

		public override Task RenderFrameAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested) return TaskHelper.CanceledTask;

			var bitmap = _screenBitmapTripleBufferingSystem.GetCurrentProducerBuffer();

			// Calls to GDI+ should be thread-safe for the most part, so we only have to take a little bit of care for ensuring correctness.
			bitmap.LockBits(_screenRectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb, _lockedScreenBitmapData);

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
			if (cancellationToken.IsCancellationRequested) return TaskHelper.CanceledTask;

			var bitmap = _borderBitmapTripleBufferingSystem.GetCurrentProducerBuffer();

			// Calls to GDI+ should be thread-safe for the most part, so we only have to take a little bit of care for ensuring correctness.
			bitmap.LockBits(_borderRectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb, _lockedBorderBitmapData);

			renderer.RenderVideoBorder32(frame, _lockedBorderBitmapData.Scan0, _lockedBorderBitmapData.Stride);

			bitmap.UnlockBits(_lockedBorderBitmapData);

			_borderBitmapTripleBufferingSystem.GetNextProducerBuffer();

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
	}
}
