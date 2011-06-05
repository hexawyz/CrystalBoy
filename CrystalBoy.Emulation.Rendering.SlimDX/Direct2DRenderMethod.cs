#region Copyright Notice
// This file is part of CrystalBoy.
// Copyright (C) 2008 Fabien Barbier
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
using System.Runtime.InteropServices;

namespace CrystalBoy.Emulation.Rendering.SlimDX
{
	[DisplayName("Direct2D")]
	public sealed class Direct2DRenderMethod : RenderMethod<Control>
	{
		Factory factory;
		WindowRenderTarget renderTarget;
		Bitmap bitmap;
		byte[] buffer;
		GCHandle bufferHandle;
		bool bufferLocked;

		public Direct2DRenderMethod(Control renderObject)
			: base(renderObject)
		{
			buffer = new byte[160 * 144 * 4];
			factory = new Factory(FactoryType.SingleThreaded, DebugLevel.None);
			CreateRenderTarget();
			CreateBitmap();
		}

		public override void Dispose()
		{
			DisposeBitmap();
			DisposeRenderTarget();

			if (factory != null) factory.Dispose();
			factory = null;
		}

		private void CreateRenderTarget()
		{
			renderTarget = new WindowRenderTarget
			(
				factory,
				new RenderTargetProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore), Type = RenderTargetType.Hardware },
				new WindowRenderTargetProperties() { Handle = RenderObject.Handle, PixelSize = new Size(160, 144), PresentOptions = PresentOptions.Immediately }
			);
		}

		private void DisposeRenderTarget()
		{
			if (renderTarget != null) renderTarget.Dispose();
			renderTarget = null;
		}

		private void CreateBitmap()
		{
			bitmap = new Bitmap(renderTarget, new Size(160, 144), new BitmapProperties() { PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore) });
		}

		private void DisposeBitmap()
		{
			if (bitmap != null) bitmap.Dispose();
			bitmap = null;
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

			bitmap.FromMemory(buffer, 160 * 4);
		}

		public override void Render()
		{
			renderTarget.BeginDraw();

			renderTarget.DrawBitmap(bitmap);

			renderTarget.EndDraw();
		}
	}
}
