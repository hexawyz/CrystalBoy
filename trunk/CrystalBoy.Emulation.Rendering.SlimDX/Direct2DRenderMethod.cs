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
using RectangleF = System.Drawing.RectangleF;
using System.Runtime.InteropServices;

namespace CrystalBoy.Emulation.Rendering.SlimDX
{
	[DisplayName("Direct2D")]
	public sealed class Direct2DRenderMethod : RenderMethod<Control>
	{
		Factory factory;
		WindowRenderTarget renderTarget;
		Bitmap bitmap1, bitmap2;
		RectangleF drawRectangle;
		byte[] buffer;
		GCHandle bufferHandle;
		bool bufferLocked;

		public Direct2DRenderMethod(Control renderObject)
			: base(renderObject)
		{
			buffer = new byte[160 * 144 * 4];
			factory = new Factory(FactoryType.SingleThreaded, DebugLevel.None);
			Reset();
			renderObject.FindForm().SizeChanged += OnSizeChanged;
		}

		public override void Dispose()
		{
			DisposeBitmaps();
			DisposeRenderTarget();

			if (factory != null) factory.Dispose();
			factory = null;

			RenderObject.FindForm().SizeChanged -= OnSizeChanged;
		}

		private void Reset()
		{
			DisposeBitmaps();
			DisposeRenderTarget();

			CreateRenderTarget();
			CreateBitmaps();
		}

		private void OnSizeChanged(object sender, EventArgs e)
		{
			if (renderTarget != null) renderTarget.Resize(RenderObject.ClientSize);
			RecalculateDrawRectangle();
		}

		private void RecalculateDrawRectangle()
		{
			int w = RenderObject.ClientSize.Width;
			int h = RenderObject.ClientSize.Height;
			float s;

			if ((s = (float)h * 160 / 144) <= w) drawRectangle = new RectangleF(0.5f * (w - s), 0, s, h);
			else if ((s = (float)w * 144 / 160) <= h) drawRectangle = new RectangleF(0, 0.5f * (h - s), w, s);
			else drawRectangle = new RectangleF(0, 0, w, h);
		}

		private void CreateRenderTarget()
		{
			renderTarget = new WindowRenderTarget
			(
				factory,
				new RenderTargetProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore), Type = RenderTargetType.Default, MinimumFeatureLevel = FeatureLevel.Direct3D9 },
				new WindowRenderTargetProperties() { Handle = RenderObject.Handle, PixelSize = RenderObject.ClientSize, PresentOptions = PresentOptions.Immediately }
			);
			RecalculateDrawRectangle();
		}

		private void DisposeRenderTarget()
		{
			if (renderTarget != null) renderTarget.Dispose();
			renderTarget = null;
		}

		private void CreateBitmaps()
		{
			bitmap1 = new Bitmap(renderTarget, new Size(160, 144), new BitmapProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore) });
			bitmap1.FromMemory(buffer, 160 * 4);
			bitmap2 = new Bitmap(renderTarget, new Size(160, 144), new BitmapProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore) });
			bitmap2.FromBitmap(bitmap1);
		}

		private void DisposeBitmaps()
		{
			if (bitmap1 != null) bitmap1.Dispose();
			bitmap1 = null;
			if (bitmap2 != null) bitmap2.Dispose();
			bitmap2 = null;
		}

		public unsafe override void* LockBuffer(out int stride)
		{
			if (!bufferLocked)
			{
				bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
				bufferLocked = true;
			}

			stride = 160 * 4;

			return bufferHandle.AddrOfPinnedObject().ToPointer();
		}

		public override void UnlockBuffer()
		{
			if (!bufferLocked) throw new InvalidOperationException();

			bufferHandle.Free();
			bufferLocked = false;

			SwapBitmaps();

			bitmap1.FromMemory(buffer, 160 * 4);
		}

		private void SwapBitmaps()
		{
			var temp = bitmap1;
			bitmap1 = bitmap2;
			bitmap2 = temp;
		}

		public override void Render() { Render(true); }

		private void Render(bool retry)
		{
			renderTarget.BeginDraw();

			renderTarget.DrawBitmap(bitmap2, drawRectangle, 1.0f, Interpolation ? InterpolationMode.Linear : InterpolationMode.NearestNeighbor);
			renderTarget.DrawBitmap(bitmap1, drawRectangle, 0.5f, Interpolation ? InterpolationMode.Linear : InterpolationMode.NearestNeighbor);

			// If needed, try to recreate the target.
			if (renderTarget.EndDraw().IsFailure)
			{
				Reset();
				// Try to render again, but only once. (We don't want to enter an infinite recursion… AKA Stack Overflow)
				if (retry) Render(false);
			}
		}
	}
}
