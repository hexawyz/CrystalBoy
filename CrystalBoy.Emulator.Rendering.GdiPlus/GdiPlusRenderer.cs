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

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CrystalBoy.Emulation;
using System.Threading.Tasks;

namespace CrystalBoy.Emulator.Rendering.GdiPlus
{
	[DisplayName("GDI+")]
	public sealed class GdiPlusRenderer : VideoRenderer<Control>
	{
		private static readonly Task CompletedTask = Task.FromResult(true);

		Bitmap borderBitmap;
		Bitmap screenBitmap;
		BitmapData bitmapData;
		InterpolationMode interpolationMode;

		public GdiPlusRenderer(Control renderObject)
			: base(renderObject)
		{
			borderBitmap = new Bitmap(256, 224, PixelFormat.Format32bppPArgb);
			screenBitmap = new Bitmap(160, 144, PixelFormat.Format32bppRgb);
			bitmapData = new BitmapData();
			ResetInterpolationMode();
		}

		public override void Dispose()
		{
			screenBitmap.Dispose();
		}

		public override unsafe VideoBufferReference LockBorderBuffer()
		{
			// Calls to GDI+ should be thread-safe for the most part, so we only have to take a little bit of care for ensuring correctness.
			borderBitmap.LockBits(new Rectangle(0, 0, 256, 224), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb, bitmapData);
			return new VideoBufferReference(bitmapData.Scan0, bitmapData.Stride);
		}

		public override unsafe Task<VideoBufferReference> LockBorderBufferAsync() { return Task.FromResult(LockBorderBuffer()); }

		public override void UnlockBorderBuffer() { borderBitmap.UnlockBits(bitmapData); }

		public override Task UnlockBorderBufferAsync()
		{
			UnlockBorderBuffer();
			return CompletedTask;
		}

		public override unsafe VideoBufferReference LockScreenBuffer()
		{
			// Calls to GDI+ should be thread-safe for the most part, so we only have to take a little bit of care for ensuring correctness.
			screenBitmap.LockBits(new Rectangle(0, 0, 160, 144), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb, bitmapData);
			return new VideoBufferReference(bitmapData.Scan0, bitmapData.Stride);
		}

		public override unsafe Task<VideoBufferReference> LockScreenBufferAsync() { return Task.FromResult(LockScreenBuffer()); }

		public override void UnlockScreenBuffer() { screenBitmap.UnlockBits(bitmapData); }

		public override Task UnlockScreenBufferAsync()
		{
			UnlockScreenBuffer();
			return CompletedTask;
		}

		public override bool SupportsInterpolation { get { return true; } }

		private void ResetInterpolationMode() { interpolationMode = Interpolation ? InterpolationMode.Default : InterpolationMode.NearestNeighbor; }

		protected override void OnInterpolationChanged(System.EventArgs e)
		{
			ResetInterpolationMode();
			base.OnInterpolationChanged(e);
		}

		public override void Render()
		{
			// Calls to GDI+ should be thread-safe for the most part, so we only have to take a little bit of care for ensuring correctness.
			// CreateGraphics is one of the few thread-safe members of Control. :)
			Graphics g = RenderObject.CreateGraphics();

			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.HighSpeed;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.SmoothingMode = SmoothingMode.None;
			g.InterpolationMode = interpolationMode;
			g.DrawImage(screenBitmap, RenderObject.ClientRectangle);

			g.Dispose();
		}

		public override Task RenderAsync()
		{
			Render();
			return CompletedTask;
		}
	}
}
