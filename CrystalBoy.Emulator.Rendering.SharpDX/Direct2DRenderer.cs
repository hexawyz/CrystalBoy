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

namespace CrystalBoy.Emulator.Rendering.SharpDX
{
	[DisplayName("Direct2D")]
	[Description("Renders video using Direct2D / SharpDX.")]
	public sealed class Direct2DRenderer : ControlVideoRenderer
	{
		private static readonly Task CompletedTask = Task.FromResult(true);
		
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
		private volatile bool borderVisible;

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

			if (borderVisible)
			{
				if ((s = (float)h * 256 / 224) <= w) drawRectangle = new RawRectangleF(0.5f * (w - s), 0, s, h);
				else if ((s = (float)w * 224 / 256) <= h) drawRectangle = new RawRectangleF(0, 0.5f * (h - s), w, s);
				else drawRectangle = new RawRectangleF(0, 0, w, h);
			}
			else
			{
				if ((s = (float)h * 160 / 144) <= w) drawRectangle = new RawRectangleF(0.5f * (w - s), 0, s, h);
				else if ((s = (float)w * 144 / 160) <= h) drawRectangle = new RawRectangleF(0, 0.5f * (h - s), w, s);
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
			);
			windowRenderTarget.DotsPerInch = new Size2F(96.0f, 96.0f);
			if (borderVisible) CreateCompositeRenderTarget();
			screenRenderTarget = new BitmapRenderTarget(windowRenderTarget, CompatibleRenderTargetOptions.None, new Size2F(160, 144), new Size2(160, 144), null);
			windowRenderTarget.AntialiasMode = AntialiasMode.Aliased;
			RecalculateDrawRectangle();
		}

		private void CreateCompositeRenderTarget()
		{
			if (compositeRenderTarget == null)
				compositeRenderTarget = new BitmapRenderTarget(windowRenderTarget, CompatibleRenderTargetOptions.None, new Size2F(256, 224), new Size2(256, 224), null);
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
		
		private void Render() { Render(true); }

		private void Render(bool retry)
		{
			var interpolationMode = false ? BitmapInterpolationMode.Linear : BitmapInterpolationMode.NearestNeighbor;

			windowRenderTarget.BeginDraw();

			if (borderVisible)
			{
				compositeRenderTarget.BeginDraw();
				compositeRenderTarget.Clear(clearColor);
				compositeRenderTarget.DrawBitmap(screenRenderTarget.Bitmap, new RawRectangleF(48, 40, 160, 144), 1.0f, BitmapInterpolationMode.NearestNeighbor);
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
				if (retry) Render(false);
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

			Render();

			var tcs = state as TaskCompletionSource<bool>;

			if (tcs != null)
			{
				if (RenderControl.IsDisposed) tcs.TrySetCanceled();
				else tcs.TrySetResult(true);
			}
		}

		public override unsafe Task RenderFrameAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			fixed (void* p = screenBuffer)
			{
				renderer.RenderVideoFrame32(frame, (IntPtr)p, 160 * 4);
			}
			cancellationToken.ThrowIfCancellationRequested();

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
				return CompletedTask;
			}
		}

		public override Task RenderBorderAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken)
		{
			return CompletedTask;
		}
	}
}