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
		private Factory _factory;
		private WindowRenderTarget _windowRenderTarget;
		private BitmapRenderTarget _compositeRenderTarget;
		private BitmapRenderTarget _screenRenderTarget;
		private Bitmap _borderBitmap;
		private Bitmap _screenBitmap1;
		private Bitmap _screenBitmap2;
		private RawColor4 _clearColor;
		private RawRectangleF _drawRectangle;
		private readonly byte[] _borderBuffer;
		private readonly byte[] _screenBuffer;

		public Direct2DRenderer(Control renderControl)
			: base(renderControl)
		{
			_borderBuffer = new byte[256 * 224 * 4];
			_screenBuffer = new byte[160 * 144 * 4];
			//clearColor = new RawColor4(unchecked((int)LookupTables.StandardColorLookupTable32[ClearColor]));
			_clearColor = new RawColor4(1, 1, 1, 1);
			_factory = new Factory(FactoryType.SingleThreaded, DebugLevel.None);
			ResetRendering();
			(RenderControl.TopLevelControl ?? RenderControl).SizeChanged += OnSizeChanged;
		}

		public override void Dispose()
		{
			DisposeBitmaps();
			DisposeRenderTargets();

			if (_factory != null) _factory.Dispose();
			_factory = null;

			(RenderControl.TopLevelControl ?? RenderControl).SizeChanged -= OnSizeChanged;
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
			if (_windowRenderTarget != null) _windowRenderTarget.Resize(new Size2(RenderControl.ClientSize.Width, RenderControl.ClientSize.Height));
			RecalculateDrawRectangle();
		}

		private void RecalculateDrawRectangle()
		{
			int w = RenderControl.ClientSize.Width;
			int h = RenderControl.ClientSize.Height;
			float s;
			float t;

			if (BorderVisible)
			{
				if ((s = (float)h * 256 / 224) <= w) _drawRectangle = new RawRectangleF(t = 0.5f * (w - s), 0, t + s, h);
				else if ((s = (float)w * 224 / 256) <= h) _drawRectangle = new RawRectangleF(0, t = 0.5f * (h - s), w, t + s);
				else _drawRectangle = new RawRectangleF(0, 0, w, h);
			}
			else
			{
				if ((s = (float)h * 160 / 144) <= w) _drawRectangle = new RawRectangleF(t = 0.5f * (w - s), 0, t + s, h);
				else if ((s = (float)w * 144 / 160) <= h) _drawRectangle = new RawRectangleF(0, t = 0.5f * (h - s), w, t + s);
				else _drawRectangle = new RawRectangleF(0, 0, w, h);
			}
		}

		private void CreateRenderTargets()
		{
			_windowRenderTarget = new WindowRenderTarget
			(
				_factory,
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
			if (BorderVisible) CreateCompositeRenderTarget();
			_screenRenderTarget = new BitmapRenderTarget(_windowRenderTarget, CompatibleRenderTargetOptions.None, new Size2F(160, 144), new Size2(160, 144), null);
			RecalculateDrawRectangle();
		}

		private void CreateCompositeRenderTarget()
		{
			if (_compositeRenderTarget == null)
			{
				_compositeRenderTarget = new BitmapRenderTarget(_windowRenderTarget, CompatibleRenderTargetOptions.None, new Size2F(256, 224), new Size2(256, 224), new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore))
				{
					DotsPerInch = new Size2F(96.0f, 96.0f),
					AntialiasMode = AntialiasMode.Aliased,
				};
			}
		}

		private void DisposeRenderTargets()
		{
			if (_compositeRenderTarget != null) _compositeRenderTarget.Dispose();
			_compositeRenderTarget = null;
			if (_windowRenderTarget != null) _windowRenderTarget.Dispose();
			_windowRenderTarget = null;
		}

		private void CreateBitmaps()
		{
			_borderBitmap = new Bitmap(_windowRenderTarget, new Size2(256, 224), new BitmapProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied) });
			_borderBitmap.CopyFromMemory(_borderBuffer, 256 * 4);
			_screenBitmap1 = new Bitmap(_windowRenderTarget, new Size2(160, 144), new BitmapProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore) });
			_screenBitmap1.CopyFromMemory(_screenBuffer, 160 * 4);
			_screenBitmap2 = new Bitmap(_windowRenderTarget, new Size2(160, 144), new BitmapProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore) });
			_screenBitmap2.CopyFromBitmap(_screenBitmap1);
		}

		private void DisposeBitmaps()
		{
			if (_borderBitmap != null) _borderBitmap.Dispose();
			_borderBitmap = null;
			if (_screenBitmap1 != null) _screenBitmap1.Dispose();
			_screenBitmap1 = null;
			if (_screenBitmap2 != null) _screenBitmap2.Dispose();
			_screenBitmap2 = null;
		}

		private void SwapBitmaps()
		{
			var temp = _screenBitmap1;
			_screenBitmap1 = _screenBitmap2;
			_screenBitmap2 = temp;
		}
		
		private void Render(bool shouldRecompose) { Render(shouldRecompose, true); }

		private void Render(bool shouldRecompose, bool shouldRetry)
		{
			var interpolationMode = false ? BitmapInterpolationMode.Linear : BitmapInterpolationMode.NearestNeighbor;

			_windowRenderTarget.BeginDraw();

			if (BorderVisible)
			{
				CreateCompositeRenderTarget();

				_compositeRenderTarget.BeginDraw();
				_compositeRenderTarget.Clear(_clearColor);
				_compositeRenderTarget.DrawBitmap(_screenRenderTarget.Bitmap, new RawRectangleF(48, 40, 48 + 160, 40 + 144), 1.0f, BitmapInterpolationMode.NearestNeighbor);
				_compositeRenderTarget.DrawBitmap(_borderBitmap, 1.0f, BitmapInterpolationMode.NearestNeighbor);
				_compositeRenderTarget.EndDraw();
				_windowRenderTarget.DrawBitmap(_compositeRenderTarget.Bitmap, _drawRectangle, 1.0f, interpolationMode);
			}
			else
			{
				_windowRenderTarget.DrawBitmap(_screenRenderTarget.Bitmap, _drawRectangle, 1.0f, interpolationMode);
			}

			try { _windowRenderTarget.EndDraw();}
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

			_screenBitmap1.CopyFromMemory(_screenBuffer, 160 * 4);

			_screenRenderTarget.BeginDraw();
			_screenRenderTarget.DrawBitmap(_screenBitmap2, new RawRectangleF(0, 0, 160, 144), 1.0f, BitmapInterpolationMode.NearestNeighbor);
			_screenRenderTarget.DrawBitmap(_screenBitmap1, new RawRectangleF(0, 0, 160, 144), 0.5f, BitmapInterpolationMode.NearestNeighbor);
			_screenRenderTarget.EndDraw();

			Render(true);

			HandleCompletion(state);
		}

		private void UpdateBorderAndPresent(object state)
		{
			_borderBitmap.CopyFromMemory(_borderBuffer, 256 * 4);

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
			
			fixed (void* p = _screenBuffer)
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

			fixed (void* p = _borderBuffer)
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