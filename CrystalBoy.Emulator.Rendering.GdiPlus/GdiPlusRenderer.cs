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

namespace CrystalBoy.Emulator.Rendering.GdiPlus
{
	[DisplayName("GDI+")]
	public sealed class GdiPlusRenderer : RenderMethod<Control>
	{
		Bitmap bitmap;
		BitmapData bitmapData;
		InterpolationMode interpolationMode;

		public GdiPlusRenderer(Control renderObject)
			: base(renderObject)
		{
			bitmap = new Bitmap(160, 144, PixelFormat.Format32bppArgb);
			bitmapData = new BitmapData();
			ResetInterpolationMode();
		}

		public override void Dispose()
		{
			bitmap.Dispose();
		}

		public override unsafe void* LockBuffer(out int stride)
		{
			bitmap.LockBits(new Rectangle(0, 0, 160, 144), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb, bitmapData);
			stride = bitmapData.Stride;
			return (void*)bitmapData.Scan0;
		}

		public override void UnlockBuffer()
		{
			bitmap.UnlockBits(bitmapData);
		}

		private void ResetInterpolationMode()
		{
			if (Interpolation)
				interpolationMode = InterpolationMode.Default;
			else
				interpolationMode = InterpolationMode.NearestNeighbor;
		}

		protected override void OnInterpolationChanged(System.EventArgs e)
		{
			ResetInterpolationMode();
			base.OnInterpolationChanged(e);
		}

		public override void Render()
		{
			Graphics g = RenderObject.CreateGraphics();

			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.HighSpeed;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.SmoothingMode = SmoothingMode.None;
			g.InterpolationMode = interpolationMode;
			g.DrawImage(bitmap, RenderObject.ClientRectangle);

			g.Dispose();
		}
	}
}
