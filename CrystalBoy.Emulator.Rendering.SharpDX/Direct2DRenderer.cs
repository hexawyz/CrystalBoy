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
using System.IO;
using SharpDX;
using SharpDX.Direct2D1;
using Format = SharpDX.DXGI.Format;
using Control = System.Windows.Forms.Control;
using System.Runtime.InteropServices;
using CrystalBoy.Emulation;
using System.Threading.Tasks;

namespace CrystalBoy.Emulator.Rendering.SharpDX
{
	[DisplayName("Direct2D")]
	[Description("Renders video using Direct2D / SharpDX.")]
	public sealed class Direct2DRenderer : VideoRenderer<Control>
	{
		private static readonly Task CompletedTask = Task.FromResult(true);

		private Factory factory;
		private WindowRenderTarget windowRenderTarget;
		private BitmapRenderTarget compositeRenderTarget;
		private BitmapRenderTarget screenRenderTarget;
		private Bitmap borderBitmap;
		private Bitmap bitmap1;
		private Bitmap bitmap2;
		private Color4 clearColor;
		private RectangleF drawRectangle;
		private volatile byte[] borderBuffer;
		private volatile byte[] screenBuffer;
		private GCHandle borderBufferHandle;
		private GCHandle screenBufferHandle;
		private volatile bool borderBufferLocked;
		private volatile bool screenBufferLocked;

		public Direct2DRenderer(Control renderObject)
			: base(renderObject)
		{
			borderBuffer = new byte[256 * 224 * 4];
			screenBuffer = new byte[160 * 144 * 4];
			clearColor = new Color4(unchecked((int)LookupTables.StandardColorLookupTable32[ClearColor]));
			factory = new Factory(FactoryType.SingleThreaded, DebugLevel.None);
			ResetRendering();
			renderObject.FindForm().SizeChanged += OnSizeChanged;
		}

		public override void Dispose()
		{
			DisposeBitmaps();
			DisposeRenderTargets();

			if (factory != null) factory.Dispose();
			factory = null;

			RenderObject.FindForm().SizeChanged -= OnSizeChanged;
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
			if (windowRenderTarget != null) windowRenderTarget.Resize(new Size2(RenderObject.ClientSize.Width, RenderObject.ClientSize.Height));
			RecalculateDrawRectangle();
		}

		private void RecalculateDrawRectangle()
		{
			int w = RenderObject.ClientSize.Width;
			int h = RenderObject.ClientSize.Height;
			float s;

			if (BorderVisible)
			{
				if ((s = (float)h * 256 / 224) <= w) drawRectangle = new RectangleF(0.5f * (w - s), 0, s, h);
				else if ((s = (float)w * 224 / 256) <= h) drawRectangle = new RectangleF(0, 0.5f * (h - s), w, s);
				else drawRectangle = new RectangleF(0, 0, w, h);
			}
			else
			{
				if ((s = (float)h * 160 / 144) <= w) drawRectangle = new RectangleF(0.5f * (w - s), 0, s, h);
				else if ((s = (float)w * 144 / 160) <= h) drawRectangle = new RectangleF(0, 0.5f * (h - s), w, s);
				else drawRectangle = new RectangleF(0, 0, w, h);
			}
		}

		private void CreateRenderTargets()
		{
			windowRenderTarget = new WindowRenderTarget
			(
				factory,
				new RenderTargetProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore), Type = RenderTargetType.Default, MinLevel = FeatureLevel.Level_DEFAULT },
				new HwndRenderTargetProperties() { Hwnd = RenderObject.Handle, PixelSize = new Size2(RenderObject.ClientSize.Width, RenderObject.ClientSize.Height), PresentOptions = PresentOptions.Immediately }
			);
			windowRenderTarget.DotsPerInch = new Size2F(96.0f, 96.0f);
			if (BorderVisible) CreateCompositeRenderTarget();
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

		public override unsafe VideoBufferReference LockBorderBuffer()
		{
			if (!borderBufferLocked)
			{
				borderBufferHandle = GCHandle.Alloc(borderBuffer, GCHandleType.Pinned);
				borderBufferLocked = true;
			}

			return new VideoBufferReference(borderBufferHandle.AddrOfPinnedObject(), 256 * 4);
		}

		public override unsafe Task<VideoBufferReference> LockBorderBufferAsync() { return Task.FromResult(LockBorderBuffer()); }

		public override void UnlockBorderBuffer()
		{
			if (!borderBufferLocked) throw new InvalidOperationException();

			borderBufferHandle.Free();
			borderBufferLocked = false;

			borderBitmap.CopyFromMemory(borderBuffer, 256 * 4);
		}

		public override Task UnlockBorderBufferAsync()
		{
			UnlockBorderBuffer();
			return CompletedTask;
		}

		public unsafe override VideoBufferReference LockScreenBuffer()
		{
			if (!screenBufferLocked)
			{
				screenBufferHandle = GCHandle.Alloc(screenBuffer, GCHandleType.Pinned);
				screenBufferLocked = true;
			}

			return new VideoBufferReference(screenBufferHandle.AddrOfPinnedObject(), 160 * 4);
		}

		public override unsafe Task<VideoBufferReference> LockScreenBufferAsync() { return Task.FromResult(LockScreenBuffer()); }

		public override void UnlockScreenBuffer()
		{
			if (!screenBufferLocked) throw new InvalidOperationException();

			screenBufferHandle.Free();
			screenBufferLocked = false;

			SwapBitmaps();

			bitmap1.CopyFromMemory(screenBuffer, 160 * 4);

			screenRenderTarget.BeginDraw();
			screenRenderTarget.DrawBitmap(bitmap2, new RectangleF(0, 0, 160, 144), 1.0f, BitmapInterpolationMode.NearestNeighbor);
			screenRenderTarget.DrawBitmap(bitmap1, new RectangleF(0, 0, 160, 144), 0.5f, BitmapInterpolationMode.NearestNeighbor);
			screenRenderTarget.EndDraw();
		}

		public override Task UnlockScreenBufferAsync()
		{
			UnlockScreenBuffer();
			return CompletedTask;
		}

		private void SwapBitmaps()
		{
			var temp = bitmap1;
			bitmap1 = bitmap2;
			bitmap2 = temp;
		}

		public override bool SupportsInterpolation { get { return true; } }

		protected override void OnClearColorChanged(EventArgs e)
		{
			clearColor = new Color4(unchecked((int)LookupTables.StandardColorLookupTable32[ClearColor]));
			base.OnClearColorChanged(e);
		}

		protected override void OnBorderVisibleChanged(EventArgs e)
		{
			if (BorderVisible) CreateCompositeRenderTarget();
			RecalculateDrawRectangle();
			base.OnBorderVisibleChanged(e);
		}

		public override void Render() { Render(true); }

		private void Render(bool retry)
		{
			var interpolationMode = Interpolation ? BitmapInterpolationMode.Linear : BitmapInterpolationMode.NearestNeighbor;

			windowRenderTarget.BeginDraw();

			if (BorderVisible)
			{
				compositeRenderTarget.BeginDraw();
				compositeRenderTarget.Clear(clearColor);
				compositeRenderTarget.DrawBitmap(screenRenderTarget.Bitmap, new RectangleF(48, 40, 160, 144), 1.0f, BitmapInterpolationMode.NearestNeighbor);
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

		public override Task RenderAsync()
		{
			Render();
			return CompletedTask;
		}
	}
}
