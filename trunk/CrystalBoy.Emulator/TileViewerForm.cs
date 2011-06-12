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
using System.Drawing;
using System.Drawing.Imaging;
using CrystalBoy.Emulation;

namespace CrystalBoy.Emulator
{
	partial class TileViewerForm : EmulatorForm
	{
		Bitmap tileSet0Bitmap, tileSet1Bitmap;

		public TileViewerForm(EmulatedGameBoy emulatedGameBoy)
			: base(emulatedGameBoy)
		{
			InitializeComponent();
			tileSet0Bitmap = new Bitmap(128, 192);
			tileSet1Bitmap = new Bitmap(128, 192);
			tileSet0Panel.Bitmap = tileSet0Bitmap;
			tileSet1Panel.Bitmap = tileSet1Bitmap;
		}

		private void UpdateTileSetBitmaps()
		{
			if (EmulatedGameBoy.Bus != null)
			{
				unsafe
				{
					UpdateTileSetBitmap(tileSet0Bitmap, (ushort*)EmulatedGameBoy.Bus.VideoRam.Pointer);
					UpdateTileSetBitmap(tileSet1Bitmap, (ushort*)((byte*)EmulatedGameBoy.Bus.VideoRam.Pointer + 0x2000));
				}
			}
			else
			{
				Common.ClearBitmap(tileSet0Bitmap);
				Common.ClearBitmap(tileSet1Bitmap);
			}
			tileSet0Panel.Invalidate();
			tileSet1Panel.Invalidate();
		}

		private unsafe void UpdateTileSetBitmap(Bitmap tileSetBitmap, ushort* tileSet)
		{
			BitmapData bitmapData = tileSetBitmap.LockBits(new Rectangle(0, 0, 128, 192), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			byte* pLine = (byte*)bitmapData.Scan0;
			ushort* pTileLine = tileSet,
				pTile;
			uint* pPixel;

			for (int i = 0; i < 24; i++) // Loop on tile rows
			{
				for (int j = 0; j < 8; j++) // Loop on tile lines
				{
					pPixel = (uint*)pLine;
					pTile = pTileLine;

					for (int k = 0; k < 16; k++) // Loop on tiles
					{
						int data = LookupTables.PaletteLookupTable[*pTile];

						for (int l = 0; l < 8; l++) // Loop on tile line pixels
						{
							*pPixel++ = LookupTables.GrayPalette[data & 0x3];
							data >>= 2;
						}

						pTile += 8; // Jump to next tile
					}

					pLine += bitmapData.Stride; // Jump to next line
					pTileLine += 1; // Increment the tile line
				}
				pTileLine += 15 * 8; // Jump to next tile row
			}

			tileSetBitmap.UnlockBits(bitmapData);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if (Visible)
				UpdateTileSetBitmaps();
			base.OnVisibleChanged(e);
		}

		private void refreshButton_Click(object sender, EventArgs e)
		{
			UpdateTileSetBitmaps();
			tileSet0Panel.Invalidate();
		}

		private void closeButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
