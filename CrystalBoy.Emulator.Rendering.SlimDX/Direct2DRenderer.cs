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
using System.Windows.Forms;
using System.IO;
using SlimDX;
using SlimDX.Direct2D;
using Format = SlimDX.DXGI.Format;
using Size = System.Drawing.Size;
using SizeF = System.Drawing.SizeF;
using Rectangle = System.Drawing.Rectangle;
using RectangleF = System.Drawing.RectangleF;
using System.Runtime.InteropServices;
using CrystalBoy.Emulation;

namespace CrystalBoy.Emulator.Rendering.SlimDX
{
	[DisplayName("Direct2D")]
	public sealed class Direct2DRenderer : VideoRenderer<Control>
	{
		Factory factory;
		WindowRenderTarget windowRenderTarget;
		BitmapRenderTarget compositeRenderTarget;
		Bitmap borderBitmap;
		Bitmap bitmap1, bitmap2;
		Color4 clearColor;
		RectangleF drawRectangle;
		byte[] borderBuffer;
		byte[] screenBuffer;
		GCHandle borderBufferHandle;
		GCHandle screenBufferHandle;
		bool borderBufferLocked;
		bool screenBufferLocked;

		public Direct2DRenderer(Control renderObject)
			: base(renderObject)
		{
			borderBuffer = new byte[256 * 224 * 4];
			screenBuffer = new byte[160 * 144 * 4];
			clearColor = new Color4(unchecked((int)LookupTables.StandardColorLookupTable32[ClearColor]));
			factory = new Factory(FactoryType.SingleThreaded, DebugLevel.None);
			Reset();
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

		private void Reset()
		{
			DisposeBitmaps();
			DisposeRenderTargets();

			CreateRenderTargets();
			CreateBitmaps();
		}

		private void OnSizeChanged(object sender, EventArgs e)
		{
			if (windowRenderTarget != null) windowRenderTarget.Resize(RenderObject.ClientSize);
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
				new RenderTargetProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore), Type = RenderTargetType.Default, MinimumFeatureLevel = FeatureLevel.Direct3D9 },
				new WindowRenderTargetProperties() { Handle = RenderObject.Handle, PixelSize = RenderObject.ClientSize, PresentOptions = PresentOptions.Immediately }
			);
			if (BorderVisible) CreateCompositeRenderTarget();
			windowRenderTarget.AntialiasMode = AntialiasMode.Aliased;
			RecalculateDrawRectangle();
		}

		private void CreateCompositeRenderTarget()
		{
			if (compositeRenderTarget == null)
				compositeRenderTarget = windowRenderTarget.CreateCompatibleRenderTarget(new SizeF(256, 224), new Size(256, 224));
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
			borderBitmap = new Bitmap(windowRenderTarget, new Size(256, 224), new BitmapProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied) });
			borderBitmap.FromMemory(borderBuffer, 256 * 4);
			bitmap1 = new Bitmap(windowRenderTarget, new Size(160, 144), new BitmapProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore) });
			bitmap1.FromMemory(screenBuffer, 160 * 4);
			bitmap2 = new Bitmap(windowRenderTarget, new Size(160, 144), new BitmapProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore) });
			bitmap2.FromBitmap(bitmap1);
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

		public override unsafe void* LockBorderBuffer(out int stride)
		{
			if (!borderBufferLocked)
			{
				borderBufferHandle = GCHandle.Alloc(borderBuffer, GCHandleType.Pinned);
				borderBufferLocked = true;
			}
			stride = 256 * 4;

			return borderBufferHandle.AddrOfPinnedObject().ToPointer();
		}

		public override void UnlockBorderBuffer()
		{
			if (!borderBufferLocked) throw new InvalidOperationException();

			borderBufferHandle.Free();
			borderBufferLocked = false;

			borderBitmap.FromMemory(borderBuffer, 256 * 4);
		}

		public unsafe override void* LockScreenBuffer(out int stride)
		{
			if (!screenBufferLocked)
			{
				screenBufferHandle = GCHandle.Alloc(screenBuffer, GCHandleType.Pinned);
				screenBufferLocked = true;
			}

			stride = 160 * 4;

			return screenBufferHandle.AddrOfPinnedObject().ToPointer();
		}

		public override void UnlockScreenBuffer()
		{
			if (!screenBufferLocked) throw new InvalidOperationException();

			screenBufferHandle.Free();
			screenBufferLocked = false;

			SwapBitmaps();

			bitmap1.FromMemory(screenBuffer, 160 * 4);
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
			var interpolationMode = Interpolation ? InterpolationMode.Linear : InterpolationMode.NearestNeighbor;

			windowRenderTarget.BeginDraw();

			if (BorderVisible)
			{
				compositeRenderTarget.BeginDraw();
				compositeRenderTarget.Clear(clearColor);
				compositeRenderTarget.DrawBitmap(bitmap2, new Rectangle(48, 40, 160, 144), 1.0f, InterpolationMode.NearestNeighbor);
				compositeRenderTarget.DrawBitmap(bitmap1, new Rectangle(48, 40, 160, 144), 0.5f, InterpolationMode.NearestNeighbor);
				compositeRenderTarget.DrawBitmap(borderBitmap);
				compositeRenderTarget.EndDraw();
				windowRenderTarget.DrawBitmap(compositeRenderTarget.Bitmap, drawRectangle, 1.0f, interpolationMode);
			}
			else
			{
				windowRenderTarget.DrawBitmap(bitmap2, drawRectangle, 1.0f, interpolationMode);
				windowRenderTarget.DrawBitmap(bitmap1, drawRectangle, 0.5f, interpolationMode);
			}

			// If needed, try to recreate the target.
			if (windowRenderTarget.EndDraw().IsFailure)
			{
				Reset();
				// Try to render again, but only once. (We don't want to enter an infinite recursion… AKA Stack Overflow)
				if (retry) Render(false);
			}
		}
	}
}
