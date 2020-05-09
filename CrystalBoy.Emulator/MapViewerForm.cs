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
using CrystalBoy.Core;
using CrystalBoy.Emulation;

namespace CrystalBoy.Emulator
{
	partial class MapViewerForm : EmulatorForm
	{
        private readonly Bitmap _backgroundMapBitmap;
        private readonly Bitmap _windowMapBitmap;
        private readonly Bitmap _customMapBitmap;
        private readonly unsafe uint[] _colors;

		public MapViewerForm(EmulatedGameBoy emulatedGameBoy)
			: base(emulatedGameBoy)
		{
			InitializeComponent();
			_backgroundMapBitmap = new Bitmap(256, 256);
			_windowMapBitmap = new Bitmap(256, 256);
			_customMapBitmap = new Bitmap(256, 256);

			backgroundMapPanel.Bitmap = _backgroundMapBitmap;
			windowMapPanel.Bitmap = _windowMapBitmap;
			customMapPanel.Bitmap = _customMapBitmap;

			_colors = new uint[32]; // 8 palettes of 4 colors each
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if (Visible)
				UpdateMapBitmaps();
			base.OnVisibleChanged(e);
		}

		protected override void OnNewFrame(EventArgs e)
		{
			if (Visible && frameUpdateCheckBox.Checked)
			{
				UpdatePalettes();
				UpdateCustomMapBitmap();
			}
		}

		protected override void OnPaused(EventArgs e)
		{
			if (Visible && pauseUpdateCheckBox.Checked)
			{
				UpdatePalettes();
				UpdateCustomMapBitmap();
			}
		}

		private void UpdateMapBitmaps()
		{
			UpdatePalettes();
			UpdateAutomaticMapBitmaps();
			UpdateCustomMapBitmap();
		}

		private unsafe void UpdatePalettes()
		{
			if (EmulatedGameBoy.Bus.ColorMode)
				UpdateColorPalettes((ushort*)EmulatedGameBoy.Bus.PaletteMemory.Pointer);
			else
				UpdateGreyPalette(EmulatedGameBoy.Bus.ReadPort(Port.BGP));
		}

		private unsafe void UpdateColorPalettes(ushort *paletteMemory)
		{
			for (int i = 0; i < _colors.Length; i++)
				_colors[i] = LookupTables.StandardColorLookupTable32[*paletteMemory++];
		}

		private unsafe void UpdateGreyPalette(byte bgp)
		{
			for (int i = 0; i < 4; i++)
			{
				_colors[i] = LookupTables.GrayPalette[bgp & 3];
				bgp >>= 2;
			}
		}

		private void UpdateAutomaticMapBitmaps()
		{
			if (EmulatedGameBoy.Bus != null)
				unsafe
				{
					byte* videoRam = (byte*)EmulatedGameBoy.Bus.VideoRam.Pointer;
					byte lcdc = EmulatedGameBoy.Bus.ReadPort(Port.LCDC);
					int bgOffset, winOffset, tileSetOffset;
					bool signedIndex;

					bgOffset = (lcdc & 0x08) != 0 ? 0x1C00 : 0x1800;
					winOffset = (lcdc & 0x40) != 0 ? 0x1C00 : 0x1800;
					signedIndex = (lcdc & 0x10) == 0;
					tileSetOffset = signedIndex ? 0x1000 : 0x0000;

					fixed (uint* colors = this._colors)
					{
						if (EmulatedGameBoy.Bus.ColorMode)
						{
							UpdateColorMapBitmap(_backgroundMapBitmap, videoRam + bgOffset, videoRam + bgOffset + 0x2000, (ushort*)(videoRam + tileSetOffset), (ushort*)(videoRam + tileSetOffset + 0x2000), signedIndex, colors);
							UpdateColorMapBitmap(_windowMapBitmap, videoRam + winOffset, videoRam + winOffset + 0x2000, (ushort*)(videoRam + tileSetOffset), (ushort*)(videoRam + tileSetOffset + 0x2000), signedIndex, colors);
						}
						else
						{
							UpdateMapBitmap(_backgroundMapBitmap, videoRam + bgOffset, (ushort*)(videoRam + tileSetOffset), signedIndex, colors);
							UpdateMapBitmap(_windowMapBitmap, videoRam + winOffset, (ushort*)(videoRam + tileSetOffset), signedIndex, colors);
						}
					}
				}
			else
			{
				Common.ClearBitmap(_backgroundMapBitmap);
				Common.ClearBitmap(_windowMapBitmap);
			}
			backgroundMapPanel.Invalidate();
			windowMapPanel.Invalidate();
		}

		private unsafe void UpdateCustomMapBitmap()
		{
			byte* videoRam = (byte*)EmulatedGameBoy.Bus.VideoRam.Pointer;

			if (EmulatedGameBoy.Bus != null)
			{
				int mapOffset = mapData0RadioButton.Checked ? 0x1800 : 0x1C00;
				int tileSetOffset = tileData0RadioButton.Checked ? 0 : 0x1000;

				fixed (uint* colors = this._colors)
				{
					if (EmulatedGameBoy.Bus.ColorMode)
						UpdateColorMapBitmap(_customMapBitmap, videoRam + mapOffset, videoRam + mapOffset + 0x2000, (ushort*)(videoRam + tileSetOffset), (ushort*)(videoRam + tileSetOffset + 0x2000), tileData1RadioButton.Checked, colors);
					else
						UpdateMapBitmap(_customMapBitmap, videoRam + mapOffset, (ushort*)(videoRam + tileSetOffset), tileData1RadioButton.Checked, colors);
				}
			}
			else
				Common.ClearBitmap(_customMapBitmap);
			customMapPanel.Invalidate();
		}

		public static unsafe void UpdateMapBitmap(Bitmap bitmap, byte* mapData, ushort* tileSet, bool signedIndex, uint* palette)
		{
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			int lineOffset;
			byte* pLine = (byte*)bitmapData.Scan0,
				pMapLine = mapData,
				pTileIndex;
			uint* pPixel;

			for (int i = 0; i < 32; i++) // Loop on rows
			{
				lineOffset = 0;

				for (int j = 0; j < 8; j++) // Loop on lines
				{
					pPixel = (uint*)pLine;
					pTileIndex = pMapLine;

					for (int k = 0; k < 32; k++) // Loop on columns
					{
						int data;
						int tileOffset;

						if (signedIndex)
							tileOffset = lineOffset + ((sbyte)*pTileIndex++ << 3);
						else
							tileOffset = lineOffset + (*pTileIndex++ << 3);
						data = LookupTables.PaletteLookupTable[tileSet[tileOffset]];

						for (int l = 0; l < 8; l++) // Loop on pixels
						{
							*pPixel++ = palette[data & 0x3];
							data >>= 2;
						}
					}

					pLine += bitmapData.Stride;
					lineOffset++;
				}

				pMapLine += 32;
			}

			bitmap.UnlockBits(bitmapData);
		}

		private static unsafe void UpdateColorMapBitmap(Bitmap bitmap, byte* mapTileData, byte* mapExtraData, ushort* tileSet0, ushort* tileSet1, bool signedIndex, uint* colors)
		{
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			int lineOffset;
			byte* pLine = (byte*)bitmapData.Scan0,
				pMapLine0 = mapTileData,
				pMapLine1 = mapExtraData,
				pTileIndex, pTileAttributes;
			uint* pPixel;
			byte tileAttributes;
			uint* palette;

			for (int i = 0; i < 32; i++) // Loop on rows
			{
				lineOffset = 0;

				for (int j = 0; j < 8; j++) // Loop on lines
				{
					pPixel = (uint*)pLine;
					pTileIndex = pMapLine0;
					pTileAttributes = pMapLine1;

					for (int k = 0; k < 32; k++) // Loop on columns
					{
						int data;
						int tileOffset;
						ushort* tileSet;

						tileAttributes = *pTileAttributes++;

						palette = colors + ((tileAttributes & 0x7) << 2);

						if ((tileAttributes & 0x08) != 0) // VRAM Bank
							tileSet = tileSet1;
						else
							tileSet = tileSet0;

						if ((tileAttributes & 0x40) != 0) // Vertical flip
							tileOffset = lineOffset ^ 7;
						else
							tileOffset = lineOffset;

						if (signedIndex)
							tileOffset += (sbyte)*pTileIndex++ << 3;
						else
							tileOffset += *pTileIndex++ << 3;

						if ((tileAttributes & 0x20) != 0) // Horizontal Flip
							data = LookupTables.FlippedPaletteLookupTable[tileSet[tileOffset]];
						else
							data = LookupTables.PaletteLookupTable[tileSet[tileOffset]];

						for (int l = 0; l < 8; l++) // Loop on pixels
						{
							*pPixel++ = palette[data & 0x3];
							data >>= 2;
						}
					}

					pLine += bitmapData.Stride;
					lineOffset++;
				}

				pMapLine0 += 32;
				pMapLine1 += 32;
			}

			bitmap.UnlockBits(bitmapData);
		}

		private void refreshButton_Click(object sender, EventArgs e)
		{
			UpdateMapBitmaps();
		}

		private void closeButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void dataRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			UpdatePalettes();
			UpdateCustomMapBitmap();
		}
	}
}
