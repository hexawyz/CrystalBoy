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
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using CrystalBoy.Emulator.Properties;

namespace CrystalBoy.Emulator
{
	static class Common
	{
		public static void ClearBitmap(Bitmap bitmap)
		{
			Graphics g = Graphics.FromImage(bitmap);

			g.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);

			g.Dispose();
		}

		public static string FormatSize(int size)
		{
			int temp = size;
			int unit = 0;
			int diviser = 1;
			string formatString;

			if (size < 0)
				throw new ArgumentOutOfRangeException("size");

			while (temp >= 1024)
			{
				temp >>= 10;
				unit++;
			}

			switch (unit)
			{
				case 0:
					if (size == 1)
						formatString = Resources.OneByteFormat;
					else
						formatString = Resources.ByteFormat;
					break;
				case 1:
					formatString = Resources.KiloByteFormat;
					diviser = 1024;
					break;
				case 2:
					formatString = Resources.MegaByteFormat;
					diviser = 1024 * 1024;
					break;
				default:
					formatString = Resources.GigaByteFormat;
					diviser = 1024 * 1024 * 1024;
					break;
			}

			if (unit == 0)
				return string.Format(Resources.Culture, formatString, size);
			else
				return string.Format(Resources.Culture, formatString, (double)size / diviser);
		}
	}
}
