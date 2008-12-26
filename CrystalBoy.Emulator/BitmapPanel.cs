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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CrystalBoy.Emulator
{
	sealed class BitmapPanel : Control
	{
		Bitmap bitmap;
		InterpolationMode interpolationMode;

		public BitmapPanel()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
			interpolationMode = InterpolationMode.NearestNeighbor;
		}

		[Category("Appearance")]
		[DefaultValue(null)]
		public Bitmap Bitmap
		{
			get
			{
				return bitmap;
			}
			set
			{
				if (value != bitmap)
				{
					bitmap = value;
					Invalidate();
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(InterpolationMode.NearestNeighbor)]
		public InterpolationMode InterpolationMode
		{
			get
			{
				return interpolationMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InterpolationMode), value))
					throw new ArgumentOutOfRangeException("value");

				if (value != interpolationMode)
				{
					interpolationMode = value;
					Invalidate();
				}
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (bitmap == null)
				e.Graphics.FillRectangle(new SolidBrush(BackColor), e.ClipRectangle);
			else
			{
				e.Graphics.InterpolationMode = interpolationMode;
				e.Graphics.DrawImage(bitmap, ClientRectangle);
			}
			base.OnPaint(e);
		}
	}
}
