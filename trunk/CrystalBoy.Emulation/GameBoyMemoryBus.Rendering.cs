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
using System.Collections.Generic;
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	public sealed partial class GameBoyMemoryBus
	{
		#region Variables

		private RenderMethod renderMethod;
		private MemoryBlock renderPaletteMemoryBlock;

		private unsafe uint** backgroundPalettes32, spritePalettes32;
		private unsafe ushort** backgroundPalettes16, spritePalettes16;

		private uint[] backgroundPalette;
		private uint[] objectPalette1;
		private uint[] objectPalette2;

		private bool greyPaletteUpdated;

		#endregion

		#region Initialize

		partial void InitializeRendering()
		{
			unsafe
			{
				uint** pointerTable;
				uint* paletteTable;

				// We will allocate memory for 16 palettes of 4 colors each, and for a palette pointer table of 16 pointers
				renderPaletteMemoryBlock = new MemoryBlock(2 * 8 * sizeof(uint*) + 2 * 8 * 4 * sizeof(uint));

				pointerTable = (uint**)renderPaletteMemoryBlock.Pointer; // Take 16 uint* at the beginning for pointer table
				paletteTable = (uint*)(pointerTable + 16); // Take the rest for palette array

				// Fill the pointer table with palette
				for (int i = 0; i < 16; i++)
					pointerTable[i] = paletteTable + 4 * i; // Each palette is 4 uint wide

				backgroundPalettes32 = pointerTable; // First 8 pointers are for the 8 background palettes
				spritePalettes32 = backgroundPalettes32 + 8; // Other 8 pointers are for the 8 sprite palettes

				// We'll use the same memory for 16 and 32 bit palettes, because only one will be used at once
				backgroundPalettes16 = (ushort**)backgroundPalettes32;
				spritePalettes16 = backgroundPalettes16 + 8;
			}

			backgroundPalette = new uint[4];
			objectPalette1 = new uint[4];
			objectPalette2 = new uint[4];
		}

		#endregion

		#region Reset

		partial void ResetRendering()
		{
			greyPaletteUpdated = false;
			Buffer.BlockCopy(LookupTables.GrayPalette, 0, backgroundPalette, 0, 4 * sizeof(uint));
			Buffer.BlockCopy(LookupTables.GrayPalette, 0, objectPalette1, 0, 4 * sizeof(uint));
			Buffer.BlockCopy(LookupTables.GrayPalette, 0, objectPalette2, 0, 4 * sizeof(uint));
		}

		#endregion

		#region Dispose

		partial void DisposeRendering()
		{
			renderPaletteMemoryBlock.Dispose();
		}

		#endregion

		#region Properties

		[CLSCompliant(false)]
		public RenderMethod RenderMethod
		{
			get
			{
				return renderMethod;
			}
			set
			{
				renderMethod = value;
				ClearBuffer();
			}
		}

		#endregion

		#region Rendering

		private unsafe void Render()
		{
			byte* buffer;
			int stride;

			if (renderMethod == null)
				return;

			buffer = (byte*)renderMethod.LockBuffer(out stride);

			if ((videoStatusSnapshot.LCDC & 0x80) != 0)
				if (colorMode)
				{
					FillPalettes32((ushort*)videoStatusSnapshot.paletteMemory);
					DrawColorFrame32(buffer, stride);
				}
				else
				{
					if (greyPaletteUpdated)
					{
						FillPalettes32((ushort*)paletteMemory);
						for (int i = 0; i < backgroundPalette.Length; i++)
							backgroundPalette[i] = backgroundPalettes32[0][i];
						for (int i = 0; i < objectPalette1.Length; i++)
							objectPalette1[i] = spritePalettes32[0][i];
						for (int i = 0; i < objectPalette2.Length; i++)
							objectPalette2[i] = spritePalettes32[1][i];
						greyPaletteUpdated = false;
					}
					DrawFrame32(buffer, stride);
				}
			else ClearBuffer32(buffer, stride, 0xFFFFFFFF);

			renderMethod.UnlockBuffer();

			renderMethod.Render();
		}

		#region Palette Initialization

		private unsafe void FillPalettes16(ushort* paletteData)
		{
			ushort* dest = backgroundPalettes16[0];

			for (int i = 0; i < 64; i++)
				*dest++ = LookupTables.ColorLookupTable16[*paletteData++];
		}

		private unsafe void FillPalettes32(ushort* paletteData)
		{
			uint* dest = backgroundPalettes32[0];

			for (int i = 0; i < 64; i++)
				*dest++ = LookupTables.ColorLookupTable32[*paletteData++];
		}

		#endregion

		#region Buffer Clearing

		public unsafe void ClearBuffer()
		{
			byte* buffer;
			int stride;

			if (renderMethod == null)
				return;

			buffer = (byte*)renderMethod.LockBuffer(out stride);

			ClearBuffer32(buffer, stride, 0xFF000000);

			renderMethod.UnlockBuffer();

			renderMethod.Render();
		}

		#region 16 BPP

		private unsafe void ClearBuffer16(byte* buffer, int stride, ushort color)
		{
			ushort* bufferPixel;

			for (int i = 0; i < 144; i++)
			{
				bufferPixel = (ushort*)buffer;

				for (int j = 0; j < 160; j++)
					*bufferPixel++ = color;

				buffer += stride;
			}
		}

		#endregion

		#region 32 BPP

		private unsafe void ClearBuffer32(byte* buffer, int stride, uint color)
		{
			uint* bufferPixel;

			for (int i = 0; i < 144; i++)
			{
				bufferPixel = (uint*)buffer;

				for (int j = 0; j < 160; j++)
					*bufferPixel++ = color;

				buffer += stride;
			}
		}

		#endregion

		#endregion

		#region ObjectData Structure

		struct ObjectData
		{
			public int Left;
			public int Right;
			public int PixelData;
			public int Palette;
			public bool Priority;
		}

		ObjectData[] objectData = new ObjectData[10];

		#endregion

		#region Color Rendering

		#region 32 BPP

		/// <summary>
		/// Draws the current frame into a pixel buffer
		/// </summary>
		/// <param name="buffer">Destination pixel buffer</param>
		/// <param name="stride">Buffer line stride</param>
		/// <param name="videoRam">Source video data</param>
		/// <param name="videoPortSnapshot">Initial snapshot of the video ports</param>
		/// <param name="portAccesses"></param>
		private unsafe void DrawColorFrame32(byte* buffer, int stride)
		{
			// WARNING: Very looooooooooong code :D
			// I have to keep track of a lot of variables for this one-pass rendering
			// Since on GBC the priorities between BG, WIN and OBJ can sometimes be weird, I don't think there is a better way of handling this.
			// The code may lack some optimizations tough, but i try my best to keep the variable count the lower possible (taking in account the fact that MS JIT is designed to handle no more than 64 variables...)
			// If you see some possible optimization, feel free to contribute.
			// The code might be very long but it is still very well structured, so with a bit of knowledge on (C)GB hardware you should understand it easily
			// In fact I think the function works pretty much like the real lcd controller on (C)GB... ;)
			byte* bufferLine = buffer;
			uint* bufferPixel;
			int scx, scy, wx, wy;
			int pi, ppi, data1, data2;
			bool bgPriority, winDraw, winDraw2, objDraw, signedIndex;
			byte objDrawn, objPriority; 
			uint** bgPalettes, objPalettes;
			uint* tilePalette;
			byte* bgMap, winMap,
				bgTile, winTile;
			int bgLineOffset, winLineOffset;
			int bgTileIndex, pixelIndex;
			ushort* bgTiles;
			int i, j;
			int objHeight, objCount;
			uint objColor = 0;

			bgPalettes = this.backgroundPalettes32;
			objPalettes = this.spritePalettes32;

			fixed (ObjectData* objectData = this.objectData)
			fixed (ushort* paletteIndexTable = LookupTables.PaletteLookupTable,
				flippedPaletteIndexTable = LookupTables.FlippedPaletteLookupTable)
			{
				tilePalette = bgPalettes[0];

				data1 = videoStatusSnapshot.LCDC;
				bgPriority = (data1 & 0x01) != 0;
				bgMap = videoMemory + ((data1 & 0x08) != 0 ? 0x1C00 : 0x1800);
				winDraw = (data1 & 0x20) != 0;
				winMap = videoMemory + ((data1 & 0x40) != 0 ? 0x1C00 : 0x1800);
				objDraw = (data1 & 0x02) != 0;
				objHeight = (data1 & 0x04) != 0 ? 16 : 8;
				signedIndex = (data1 & 0x10) == 0;
				bgTiles = (ushort*)(signedIndex ? videoMemory + 0x1000 : videoMemory);

				scx = videoStatusSnapshot.SCX;
				scy = videoStatusSnapshot.SCY;
				wx = videoStatusSnapshot.WX - 7;
				wy = videoStatusSnapshot.WY;

				// Initialize objPriority to 1 as it's the default value, but it's only to please the C# compiler.
				// This is an internal status flag used for CGB rendering which is more complex than DMG.
				// This flag is the combination of the VRAM Bank Tile Attribute Bit 7 (BG Priority) and the master priority flag in LCDC Bit 0
				// Values for objPriority flag: (Internal status flag)
				//  0: BG priority (BG drawn over the sprite)
				//  1: OBJ priority controlled by OAM flag (Sprite appears over BG, or only if BG color index is 0)
				//  2: OBJ priority (Sprite draw over BG & WIN)
				objPriority = 1;

				pi = 0; // Port access list index
				ppi = 0; // Palette access list index

				for (i = 0; i < 144; i++) // Loop on frame lines
				{
					#region Video Port Updates

					data2 = i * 456; // Line clock

					// Update ports before drawing the line
					while (pi < videoPortAccessList.Count && videoPortAccessList[pi].Clock < data2)
					{
						switch (videoPortAccessList[pi].Port)
						{
							case Port.LCDC:
								data1 = videoPortAccessList[pi].Value;
								bgPriority = (data1 & 0x01) != 0;
								bgMap = videoMemory + ((data1 & 0x08) != 0 ? 0x1C00 : 0x1800);
								winDraw = (data1 & 0x20) != 0;
								winMap = videoMemory + ((data1 & 0x40) != 0 ? 0x1C00 : 0x1800);
								objDraw = (data1 & 0x02) != 0;
								objHeight = (data1 & 0x04) != 0 ? 16 : 8;
								signedIndex = (data1 & 0x10) == 0;
								bgTiles = (ushort*)(signedIndex ? videoMemory + 0x1000 : videoMemory);
								break;
							case Port.SCX: scx = videoPortAccessList[pi].Value; break;
							case Port.SCY: scy = videoPortAccessList[pi].Value; break;
							case Port.WX: wx = videoPortAccessList[pi].Value - 7; break;
						}

						pi++;
					}
					// Update palettes before drawing the line (This is necessary for a lot of demos with dynamic palettes)
					while (ppi < paletteAccessList.Count && paletteAccessList[ppi].Clock < data2)
					{
						// By doing this, we trash the palette memory snapshot… But at least it works. (Might be necessary to allocate another temporary palette buffer in the future)
						videoStatusSnapshot.paletteMemory[paletteAccessList[ppi].Offset] = paletteAccessList[ppi].Value;
						bgPalettes[0][paletteAccessList[ppi].Offset / 2] = LookupTables.ColorLookupTable32[((ushort*)videoStatusSnapshot.paletteMemory)[paletteAccessList[ppi].Offset / 2]];

						ppi++;
					}

					#endregion

					#region Object Attribute Memory Search

					// Find valid sprites for the line, limited to 10 like on real GB
					for (j = 0, objCount = 0; j < 40 && objCount < 10; j++) // Loop on OAM data
					{
						bgTile = objectAttributeMemory + (j << 2); // Obtain a pointer to the object data

						// First byte is vertical position and that's exactly what we want to compare :)
						data1 = *bgTile - 16;
						if (data1 <= i && data1 + objHeight > i) // Check that the sprite is drawn on the current line
						{
							// Initialize the object data according to what we want
							data2 = bgTile[1]; // Second byte is the horizontal position, we store it somewhere
							objectData[objCount].Left = data2 - 8;
							objectData[objCount].Right = data2;
							data2 = bgTile[3]; // Fourth byte contain flags that we'll examine
							objectData[objCount].Palette = data2 & 0x7; // Use the palette index stored in flags
							objectData[objCount].Priority = (data2 & 0x80) == 0; // Store the priority information
							// Now we check the Y flip flag, as we'll use it to calculate the tile line offset
							if ((data2 & 0x40) != 0)
								data1 = (objHeight + data1 - i - 1) << 1;
							else
								data1 = (i - data1) << 1;
							// Now that we have the line offset, we add to it the tile offset
							if (objHeight == 16) // Depending on the sprite size we'll have to mask bit 0 of the tile index
								data1 += (bgTile[2] & 0xFE) << 4; // Third byte is the tile index
							else
								data1 += bgTile[2] << 4; // A tile is 16 bytes wide
							// Now all that is left is to fetch the tile data :)
							if ((data2 & 0x8) != 0)
								bgTile = videoMemory + data1 + 0x2000; // Calculate the full tile line address for VRAM Bank 1
							else
								bgTile = videoMemory + data1; // Calculate the full tile line address for VRAM Bank 0
							// Depending on the X flip flag, we will load the flipped pixel data or the regular one
							if ((data2 & 0x20) != 0)
								objectData[objCount].PixelData = flippedPaletteIndexTable[*(ushort*)bgTile];
							else
								objectData[objCount].PixelData = paletteIndexTable[*(ushort*)bgTile];
							objCount++; // Increment the object counter
						}
					}

					#endregion

					#region Background and Window Fetch Initialization

					// Initialize the background and window with new parameters
					bgTileIndex = scx >> 3;
					pixelIndex = scx & 7;
					data1 = (scy + i) >> 3; // Background Line Index
					bgLineOffset = (scy + i) & 7;
					if (data1 >= 32) // Tile the background vertically
						data1 -= 32;
					bgTile = bgMap + (data1 << 5) + bgTileIndex;
					winTile = winMap + (((i - wy) << 2) & ~0x1F); // Optimisation for 32 * x / 8 => >> 3 << 5
					winLineOffset = (i - wy) & 7;

					winDraw2 = winDraw && i >= wy;

					#endregion

					// Adjust the current pixel to the current line
					bufferPixel = (uint*)bufferLine;

					// Do the actual drawing
					for (j = 0; j < 160; j++) // Loop on line pixels
					{
						objDrawn = 0; // Draw no object by default

						if (objDraw && objCount > 0)
						{
							for (data2 = 0; data2 < objCount; data2++)
							{
								if (objectData[data2].Left <= j && objectData[data2].Right > j)
								{
									objColor = (uint)(objectData[data2].PixelData >> ((j - objectData[data2].Left) << 1)) & 3;
									if ((objDrawn = (byte)(objColor != 0 ? objectData[data2].Priority ? 2 : 1 : 0)) != 0)
									{
										objColor = objPalettes[objectData[data2].Palette][objColor];
										break;
									}
								}
							}
						}
						if (winDraw2 && j >= wx)
						{
							if (pixelIndex >= 8 || j == 0 || j == wx)
							{
								data2 = *(winTile + 0x2000);
								tilePalette = bgPalettes[data2 & 0x7];
								data1 = ((data2 & 0x40) != 0 ? 7 - winLineOffset : winLineOffset) + (signedIndex ? (sbyte)*winTile++ << 3 : *winTile++ << 3);
								if ((data2 & 0x8) != 0) data1 += 0x1000;
								data1 = (data2 & 0x20) != 0 ? flippedPaletteIndexTable[bgTiles[data1]] : paletteIndexTable[bgTiles[data1]];

								objPriority = bgPriority ? (data2 & 0x80) != 0 ? (byte)0 : (byte)1 : (byte)2;

								if (j == 0 && wx < 0)
								{
									pixelIndex = -wx;
									data1 >>= pixelIndex << 1;
								}
								else pixelIndex = 0;
							}

							*bufferPixel++ = objDrawn != 0 && objPriority != 0 && (objPriority == 2 || objDrawn == 2 || (data1 & 0x3) == 0) ? objColor : tilePalette[data1 & 0x3];

							data1 >>= 2;
							pixelIndex++;
						}
						else
						{
							if (pixelIndex >= 8 || j == 0)
							{
								if (bgTileIndex++ >= 32) // Tile the background horizontally
								{
									bgTile -= 32;
									bgTileIndex = 0;
								}

								data2 = *(bgTile + 0x2000);
								tilePalette = bgPalettes[data2 & 0x7];
								data1 = ((data2 & 0x40) != 0 ? 7 - bgLineOffset : bgLineOffset) + (signedIndex ? (sbyte)*bgTile++ << 3 : *bgTile++ << 3);
								if ((data2 & 0x8) != 0) data1 += 0x1000;
								data1 = (data2 & 0x20) != 0 ? flippedPaletteIndexTable[bgTiles[data1]] : paletteIndexTable[bgTiles[data1]];

								objPriority = bgPriority ? (data2 & 0x80) != 0 ? (byte)0 : (byte)1 : (byte)2;

								if (j == 0 && pixelIndex > 0) data1 >>= pixelIndex << 1;
								else pixelIndex = 0;
							}

							*bufferPixel++ = objDrawn != 0 && objPriority != 0 && (objPriority == 2 || objDrawn == 2 || (data1 & 0x3) == 0) ? objColor : tilePalette[data1 & 0x3];
							data1 >>= 2;
							pixelIndex++;
						}
					}

					bufferLine += stride;
				}
			}
		}

		#endregion

		#endregion

		#region Grayscale Rendering

		#region 32 BPP

		/// <summary>
		/// Draws the current frame into a pixel buffer
		/// </summary>
		/// <param name="buffer">Destination pixel buffer</param>
		/// <param name="stride">Buffer line stride</param>
		/// <param name="videoRam">Source video data</param>
		/// <param name="videoPortSnapshot">Initial snapshot of the video ports</param>
		/// <param name="portAccesses"></param>
		private unsafe void DrawFrame32(byte* buffer, int stride)
		{
			// WARNING: Very looooooooooong code :D
			// I have to keep track of a lot of variables for this one-pass rendering
			// Since on GBC the priorities between BG, WIN and OBJ can sometimes be weird, I don't think there is a better way of handling this.
			// The code may lack some optimizations tough, but i try my best to keep the variable count the lower possible (taking in account the fact that MS JIT is designed to handle no more than 64 variables...)
			// If you see some possible optimization, feel free to contribute.
			// The code might be very long but it is still very well structured, so with a bit of knowledge on (C)GB hardware you should understand it easily
			// In fact I think the function works pretty much like the real lcd controller on (C)GB... ;)
			byte* bufferLine = buffer;
			uint* bufferPixel;
			int scx, scy, wx, wy;
			int pi, data1, data2;
			bool bgDraw, winDraw, winDraw2, objDraw, signedIndex;
			byte objDrawn;
			uint** bgPalettes, objPalettes;
			uint* tilePalette;
			byte* bgMap, winMap,
				bgTile, winTile;
			int bgLineOffset, winLineOffset;
			int bgTileIndex, pixelIndex;
			ushort* bgTiles;
			int i, j;
			int objHeight, objCount;
			uint objColor = 0;

			bgPalettes = this.backgroundPalettes32;
			objPalettes = this.spritePalettes32;

			fixed (ObjectData* objectData = this.objectData)
			fixed (ushort* paletteIndexTable = LookupTables.PaletteLookupTable,
				flippedPaletteIndexTable = LookupTables.FlippedPaletteLookupTable)
			{
				tilePalette = bgPalettes[0];

				data1 = videoStatusSnapshot.LCDC;
				bgDraw = (data1 & 0x01) != 0;
				bgMap = videoMemory + ((data1 & 0x08) != 0 ? 0x1C00 : 0x1800);
				winDraw = (data1 & 0x20) != 0;
				winMap = videoMemory + ((data1 & 0x40) != 0 ? 0x1C00 : 0x1800);
				objDraw = (data1 & 0x02) != 0;
				objHeight = (data1 & 0x04) != 0 ? 16 : 8;
				signedIndex = (data1 & 0x10) == 0;
				bgTiles = (ushort*)(signedIndex ? videoMemory + 0x1000 : videoMemory);

				scx = videoStatusSnapshot.SCX;
				scy = videoStatusSnapshot.SCY;
				wx = videoStatusSnapshot.WX - 7;
				wy = videoStatusSnapshot.WY;
				data1 = videoStatusSnapshot.BGP;
				for (i = 0; i < 4; i++)
				{
					tilePalette[i] = backgroundPalette[data1 & 3];
					data1 >>= 2;
				}
				data1 = videoStatusSnapshot.OBP0;
				for (j = 0; j < 4; j++)
				{
					objPalettes[0][j] = objectPalette1[data1 & 3];
					data1 >>= 2;
				}
				data1 = videoStatusSnapshot.OBP1;
				for (j = 0; j < 4; j++)
				{
					objPalettes[1][j] = objectPalette2[data1 & 3];
					data1 >>= 2;
				}

				pi = 0;

				for (i = 0; i < 144; i++) // Loop on frame lines
				{
					#region Video Port Updates

					data2 = i * 456; // Line clock

					// Update ports before drawing the line
					while (pi < videoPortAccessList.Count && videoPortAccessList[pi].Clock < data2)
					{
						switch (videoPortAccessList[pi].Port)
						{
							case Port.LCDC:
								data1 = videoPortAccessList[pi].Value;
								bgDraw = (data1 & 0x01) != 0;
								bgMap = videoMemory + ((data1 & 0x08) != 0 ? 0x1C00 : 0x1800);
								winDraw = (data1 & 0x20) != 0;
								winMap = videoMemory + ((data1 & 0x40) != 0 ? 0x1C00 : 0x1800);
								objDraw = (data1 & 0x02) != 0;
								objHeight = (data1 & 0x04) != 0 ? 16 : 8;
								signedIndex = (data1 & 0x10) == 0;
								bgTiles = (ushort*)(signedIndex ? videoMemory + 0x1000 : videoMemory);
								break;
							case Port.SCX: scx = videoPortAccessList[pi].Value; break;
							case Port.SCY: scy = videoPortAccessList[pi].Value; break;
							case Port.WX: wx = videoPortAccessList[pi].Value - 7; break;
							case Port.BGP:
								data1 = videoPortAccessList[pi].Value;
								for (j = 0; j < 4; j++)
								{
									tilePalette[j] = backgroundPalette[data1 & 3];
									data1 >>= 2;
								}
								break;
							case Port.OBP0:
								data1 = videoPortAccessList[pi].Value;
								for (j = 0; j < 4; j++)
								{
									objPalettes[0][j] = objectPalette1[data1 & 3];
									data1 >>= 2;
								}
								break;
							case Port.OBP1:
								data1 = videoPortAccessList[pi].Value;
								for (j = 0; j < 4; j++)
								{
									objPalettes[1][j] = objectPalette2[data1 & 3];
									data1 >>= 2;
								}
								break;
						}

						pi++;
					}

					#endregion

					#region Object Attribute Memory Search

					// Find valid sprites for the line, limited to 10 like on real GB
					for (j = 0, objCount = 0; j < 40 && objCount < 10; j++) // Loop on OAM data
					{
						bgTile = objectAttributeMemory + (j << 2); // Obtain a pointer to the object data

						// First byte is vertical position and that's exactly what we want to compare :)
						data1 = *bgTile - 16;
						if (data1 <= i && data1 + objHeight > i) // Check that the sprite is drawn on the current line
						{
							// Initialize the object data according to what we want
							data2 = bgTile[1]; // Second byte is the horizontal position, we store it somewhere
							objectData[objCount].Left = data2 - 8;
							objectData[objCount].Right = data2;
							data2 = bgTile[3]; // Fourth byte contain flags that we'll examine
							objectData[objCount].Palette = (data2 & 0x10) != 0 ? 1 : 0; // Set the palette index according to the flags
							objectData[objCount].Priority = (data2 & 0x80) == 0; // Store the priority information
							// Now we check the Y flip flag, as we'll use it to calculate the tile line offset
							if ((data2 & 0x40) != 0)
								data1 = (objHeight + data1 - i - 1) << 1;
							else
								data1 = (i - data1) << 1;
							// Now that we have the line offset, we add to it the tile offset
							if (objHeight == 16) // Depending on the sprite size we'll have to mask bit 0 of the tile index
								data1 += (bgTile[2] & 0xFE) << 4; // Third byte is the tile index
							else
								data1 += bgTile[2] << 4; // A tile is 16 bytes wide
							// No all that is left is to fetch the tile data :)
							bgTile = videoMemory + data1; // Calculate the full tile line address for VRAM Bank 0
							// Depending on the X flip flag, we will load the flipped pixel data or the regular one
							if ((data2 & 0x20) != 0)
								objectData[objCount].PixelData = flippedPaletteIndexTable[*(ushort*)bgTile];
							else
								objectData[objCount].PixelData = paletteIndexTable[*(ushort*)bgTile];
							objCount++; // Increment the object counter
						}
					}

					#endregion

					#region Background and Window Fetch Initialization

					// Initialize the background and window with new parameters
					bgTileIndex = scx >> 3;
					pixelIndex = scx & 7;
					data1 = (scy + i) >> 3; // Background Line Index
					bgLineOffset = (scy + i) & 7;
					if (data1 >= 32) // Tile the background vertically
						data1 -= 32;
					bgTile = bgMap + (data1 << 5) + bgTileIndex;
					winTile = winMap + (((i - wy) << 2) & ~0x1F);
					winLineOffset = (i - wy) & 7;

					winDraw2 = winDraw && i >= wy;

					#endregion

					// Adjust the current pixel to the current line
					bufferPixel = (uint*)bufferLine;

					// Do the actual drawing
					for (j = 0; j < 160; j++) // Loop on line pixels
					{
						objDrawn = 0; // Draw no object by default

						if (objDraw && objCount > 0)
						{
							for (data2 = 0; data2 < objCount; data2++)
							{
								if (objectData[data2].Left <= j && objectData[data2].Right > j)
								{
									objColor = (uint)(objectData[data2].PixelData >> ((j - objectData[data2].Left) << 1)) & 3;
									if ((objDrawn = (byte)(objColor != 0 ? objectData[data2].Priority ? 2 : 1 : 0)) != 0)
									{
										objColor = objPalettes[objectData[data2].Palette][objColor];
										break;
									}
								}
							}
						}
						if (winDraw2 && j >= wx)
						{
							if (pixelIndex >= 8 || j == 0 || j == wx)
							{
								data1 = winLineOffset + (signedIndex ? (sbyte)*winTile++ << 3 : *winTile++ << 3);

								data1 = paletteIndexTable[bgTiles[data1]];

								if (j == 0 && wx < 0)
								{
									pixelIndex = -wx;
									data1 >>= pixelIndex << 1;
								}
								else pixelIndex = 0;
							}

							*bufferPixel++ = objDrawn != 0 && (objDrawn == 2 || (data1 & 0x3) == 0) ? objColor : tilePalette[data1 & 0x3];

							data1 >>= 2;
							pixelIndex++;
						}
						else if (bgDraw)
						{
							if (pixelIndex >= 8 || j == 0)
							{
								if (bgTileIndex++ >= 32) // Tile the background horizontally
								{
									bgTile -= 32;
									bgTileIndex = 0;
								}

								data1 = bgLineOffset + (signedIndex ? (sbyte)*bgTile++ << 3 : *bgTile++ << 3);

								data1 = paletteIndexTable[bgTiles[data1]];

								if (j == 0 && pixelIndex > 0) data1 >>= pixelIndex << 1;
								else pixelIndex = 0;
							}

							*bufferPixel++ = objDrawn != 0 && (objDrawn == 2 || (data1 & 0x3) == 0) ? objColor : tilePalette[data1 & 0x3];
							data1 >>= 2;
							pixelIndex++;
						}
						else *bufferPixel++ = objDrawn != 0 ? objColor : LookupTables.GrayPalette[0];
					}

					bufferLine += stride;
				}
			}
		}

		#endregion

		#endregion

		#endregion
	}
}
