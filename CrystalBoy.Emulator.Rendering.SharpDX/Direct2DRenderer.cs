using System;
using System.ComponentModel;
using System.IO;
using SharpDX;
using SharpDX.Direct2D1;
using Format = SharpDX.DXGI.Format;
using Control = System.Windows.Forms.Control;
using System.Runtime.InteropServices;
using CrystalBoy.Emulation;
using System.Threading.Tasks;
using System.Threading;
using SharpDX.Mathematics.Interop;
using CrystalBoy.Emulation.Windows.Forms;
using CrystalBoy.Core;

namespace CrystalBoy.Emulator.Rendering.SharpDX
{
	[DisplayName("Direct2D")]
	[Description("Renders video using Direct2D.")]
	public sealed class Direct2DRenderer : ControlVideoRenderer
	{
		private Factory factory;
		private WindowRenderTarget windowRenderTarget;
		private BitmapRenderTarget compositeRenderTarget;
		private BitmapRenderTarget screenRenderTarget;
		private Bitmap borderBitmap;
		private Bitmap bitmap1;
		private Bitmap bitmap2;
		private RawColor4 clearColor;
		private RawRectangleF drawRectangle;
		private readonly byte[] borderBuffer;
		private readonly byte[] screenBuffer;
		private volatile bool borderVisible = true;

		public Direct2DRenderer(Control renderControl)
			: base(renderControl)
		{
			borderBuffer = new byte[256 * 224 * 4];
			screenBuffer = new byte[160 * 144 * 4];
			//clearColor = new RawColor4(unchecked((int)LookupTables.StandardColorLookupTable32[ClearColor]));
			clearColor = new RawColor4(1, 1, 1, 1);
			factory = new Factory(FactoryType.SingleThreaded, DebugLevel.None);
			ResetRendering();
			RenderControl.FindForm().SizeChanged += OnSizeChanged;
		}

		public void Dispose()
		{
			DisposeBitmaps();
			DisposeRenderTargets();

			if (factory != null) factory.Dispose();
			factory = null;

			RenderControl.FindForm().SizeChanged -= OnSizeChanged;
		}

		private void ResetRendering()
		{
			DisposeBitmaps();
			DisposeRenderTargets();

			CreateRenderTargets();
			CreateBitmaps();
		}

		private void OnSizeChanged(object sender, EventArgs e)
		{
			if (windowRenderTarget != null) windowRenderTarget.Resize(new Size2(RenderControl.ClientSize.Width, RenderControl.ClientSize.Height));
			RecalculateDrawRectangle();
		}

		private void RecalculateDrawRectangle()
		{
			int w = RenderControl.ClientSize.Width;
			int h = RenderControl.ClientSize.Height;
			float s;
			float t;

			if (borderVisible)
			{
				if ((s = (float)h * 256 / 224) <= w) drawRectangle = new RawRectangleF(t = 0.5f * (w - s), 0, t + s, h);
				else if ((s = (float)w * 224 / 256) <= h) drawRectangle = new RawRectangleF(0, t = 0.5f * (h - s), w, t + s);
				else drawRectangle = new RawRectangleF(0, 0, w, h);
			}
			else
			{
				if ((s = (float)h * 160 / 144) <= w) drawRectangle = new RawRectangleF(t = 0.5f * (w - s), 0, t + s, h);
				else if ((s = (float)w * 144 / 160) <= h) drawRectangle = new RawRectangleF(0, t = 0.5f * (h - s), w, t + s);
				else drawRectangle = new RawRectangleF(0, 0, w, h);
			}
		}

		private void CreateRenderTargets()
		{
			windowRenderTarget = new WindowRenderTarget
			(
				factory,
				new RenderTargetProperties()
				{
					PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore),
					Type = RenderTargetType.Default,
					MinLevel = FeatureLevel.Level_DEFAULT
				},
				new HwndRenderTargetProperties()
				{
					Hwnd = RenderControl.Handle,
					PixelSize = new Size2(RenderControl.ClientSize.Width, RenderControl.ClientSize.Height),
					PresentOptions = PresentOptions.Immediately
				}
			)
			{
				DotsPerInch = new Size2F(96.0f, 96.0f),
				AntialiasMode = AntialiasMode.Aliased,
			};
			if (borderVisible) CreateCompositeRenderTarget();
			screenRenderTarget = new BitmapRenderTarget(windowRenderTarget, CompatibleRenderTargetOptions.None, new Size2F(160, 144), new Size2(160, 144), null);
			RecalculateDrawRectangle();
		}

		private void CreateCompositeRenderTarget()
		{
			if (compositeRenderTarget == null)
			{
				compositeRenderTarget = new BitmapRenderTarget(windowRenderTarget, CompatibleRenderTargetOptions.None, new Size2F(256, 224), new Size2(256, 224), new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore))
				{
					DotsPerInch = new Size2F(96.0f, 96.0f),
					AntialiasMode = AntialiasMode.Aliased,
				};
			}
		}

		private void DisposeRenderTargets()
		{
			if (compositeRenderTarget != null) compositeRenderTarget.Dispose();
			compositeRenderTarget = null;
			if (windowRenderTarget != null) windowRenderTarget.Dispose();
			windowRenderTarget = null;
		}

		private void CreateBitmaps()
		{
			borderBitmap = new Bitmap(windowRenderTarget, new Size2(256, 224), new BitmapProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied) });
			borderBitmap.CopyFromMemory(borderBuffer, 256 * 4);
			bitmap1 = new Bitmap(windowRenderTarget, new Size2(160, 144), new BitmapProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore) });
			bitmap1.CopyFromMemory(screenBuffer, 160 * 4);
			bitmap2 = new Bitmap(windowRenderTarget, new Size2(160, 144), new BitmapProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore) });
			bitmap2.CopyFromBitmap(bitmap1);
		}

		private void DisposeBitmaps()
		{
			if (borderBitmap != null) borderBitmap.Dispose();
			borderBitmap = null;
			if (bitmap1 != null) bitmap1.Dispose();
			bitmap1 = null;
			if (bitmap2 != null) bitmap2.Dispose();
			bitmap2 = null;
		}

		private void SwapBitmaps()
		{
			var temp = bitmap1;
			bitmap1 = bitmap2;
			bitmap2 = temp;
		}
		
		private void Render(bool shouldRecompose) { Render(shouldRecompose, true); }

		private void Render(bool shouldRecompose, bool shouldRetry)
		{
			var interpolationMode = false ? BitmapInterpolationMode.Linear : BitmapInterpolationMode.NearestNeighbor;

			windowRenderTarget.BeginDraw();

			if (borderVisible)
			{
				compositeRenderTarget.BeginDraw();
				compositeRenderTarget.Clear(clearColor);
				compositeRenderTarget.DrawBitmap(screenRenderTarget.Bitmap, new RawRectangleF(48, 40, 48 + 160, 40 + 144), 1.0f, BitmapInterpolationMode.NearestNeighbor);
				compositeRenderTarget.DrawBitmap(borderBitmap, 1.0f, BitmapInterpolationMode.NearestNeighbor);
				compositeRenderTarget.EndDraw();
				windowRenderTarget.DrawBitmap(compositeRenderTarget.Bitmap, drawRectangle, 1.0f, interpolationMode);
			}
			else windowRenderTarget.DrawBitmap(screenRenderTarget.Bitmap, drawRectangle, 1.0f, interpolationMode);

			try { windowRenderTarget.EndDraw();}
			catch (COMException)
			{
				// If needed, try to recreate the target.
				ResetRendering();
				// Try to render again, but only once. (We don't want to enter an infinite recursion… AKA Stack Overflow)
				if (shouldRetry) Render(true, false);
			}
		}

		private void UpdateScreenAndPresent(object state)
		{
			SwapBitmaps();

			bitmap1.CopyFromMemory(screenBuffer, 160 * 4);

			screenRenderTarget.BeginDraw();
			screenRenderTarget.DrawBitmap(bitmap2, new RawRectangleF(0, 0, 160, 144), 1.0f, BitmapInterpolationMode.NearestNeighbor);
			screenRenderTarget.DrawBitmap(bitmap1, new RawRectangleF(0, 0, 160, 144), 0.5f, BitmapInterpolationMode.NearestNeighbor);
			screenRenderTarget.EndDraw();

			Render(true);

			HandleCompletion(state);
		}

		private void UpdateBorderAndPresent(object state)
		{
			borderBitmap.CopyFromMemory(borderBuffer, 256 * 4);

			Render(true);

			HandleCompletion(state);
		}

		private void HandleCompletion(object state)
		{
			var tcs = state as TaskCompletionSource<bool>;

			if (tcs != null)
			{
				if (RenderControl.IsDisposed) tcs.TrySetCanceled();
				else tcs.TrySetResult(true);
			}
		}

		public override void Refresh()
		{
			Render(false);
		}

		public override unsafe Task RenderFrameAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested) return TaskHelper.CanceledTask;
			
			fixed (void* p = screenBuffer)
			{
				renderer.RenderVideoFrame32(frame, (IntPtr)p, 160 * 4);
			}

			if (cancellationToken.IsCancellationRequested) return TaskHelper.CanceledTask;

			// Setting up a task completion source may not be needed here, but I don't know if there's something to gain by not doing so.
			if (SynchronizationContext != null)
			{
				var tcs = new TaskCompletionSource<bool>();
				SynchronizationContext.Post(UpdateScreenAndPresent, tcs);
				return tcs.Task;
			}
			else
			{
				UpdateScreenAndPresent(null);
				return TaskHelper.TrueTask;
			}
		}

		public override unsafe Task RenderBorderAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			fixed (void* p = borderBuffer)
			{
				renderer.RenderVideoBorder32(frame, (IntPtr)p, 256 * 4);
			}
			cancellationToken.ThrowIfCancellationRequested();

			// Setting up a task completion source may not be needed here, but I don't know if there's something to gain by not doing so.
			if (SynchronizationContext != null)
			{
				var tcs = new TaskCompletionSource<bool>();
				SynchronizationContext.Post(UpdateBorderAndPresent, tcs);
				return tcs.Task;
			}
			else
			{
				UpdateBorderAndPresent(null);
				return TaskHelper.TrueTask;
			}
		}
	}
}