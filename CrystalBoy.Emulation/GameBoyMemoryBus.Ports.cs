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
	partial class GameBoyMemoryBus : IPortMap
	{
		#region Variables

		// Snapshot of the audio ports, used for frame rendering
		AudioStatusSnapshot audioStatusSnapshot;
		AudioStatusSnapshot savedAudioStatusSnapshot;
		// Snapshot of the video ports, used for frame rendering
		VideoStatusSnapshot videoStatusSnapshot;
		VideoStatusSnapshot savedVideoStatusSnapshot;
		// Recorded video port accesses, used for video frame rendering
		List<PortAccess> videoPortAccessList;
		List<PortAccess> savedVideoPortAccessList;
		// Recorded color palette accesses for CGB mode only. (No need to simulate BGPI/BGPD and OPBI/OBPD during rendering)
		List<PaletteAccess> paletteAccessList;
		List<PaletteAccess> savedPaletteAccessList;
		// Recorded audio port accesses, used for audio frame rendering
		List<PortAccess> audioPortAccessList;
		List<PortAccess> savedAudioPortAccessList;

		int bgpIndex, obpIndex;
		bool bgpInc, obpInc;
		bool usePaletteMapping;

		byte hdmaSourceHigh, hdmaSourceLow, hdmaDestinationHigh, hdmaDestinationLow;
		// For Horizontal Blank DMA
		bool hdmaActive;
		byte hdmaCurrentSourceHigh, hdmaCurrentSourceLow, hdmaCurrentDestinationHigh, hdmaCurrentDestinationLow, hdmaCurrentLength;

		#endregion

		#region Initialize

		partial void InitializePorts()
		{
			this.videoStatusSnapshot = new VideoStatusSnapshot(this);
			this.videoPortAccessList = new List<PortAccess>(1000);
			this.paletteAccessList = new List<PaletteAccess>(1000);
			this.audioPortAccessList = new List<PortAccess>(1000);
#if WITH_THREADING
			this.savedVideoStatusSnapshot = new VideoStatusSnapshot(this);
			this.savedVideoPortAccessList = new List<PortAccess>(1000);
			this.savedPaletteAccessList = new List<PaletteAccess>(1000);
			this.savedAudioPortAccessList = new List<PortAccess>(1000);
#else
			this.savedVideoStatusSnapshot = videoStatusSnapshot;
			this.savedVideoPortAccessList = videoPortAccessList;
			this.savedPaletteAccessList = paletteAccessList;
#endif
		}

		#endregion

		#region Reset

		partial void ResetPorts()
		{
			hdmaActive = false;
			usePaletteMapping = colorHardware && !useBootRom;

			unsafe { portMemory[0x00] = 0x30; }

			WritePort(Port.TIMA, 0);
			WritePort(Port.TMA, 0);
			WritePort(Port.TAC, 0);

			WritePort(Port.IF, 0x00);

			// Those probably start at 0 on boot too…
			WritePort(Port.NR10, 0x80);
			WritePort(Port.NR11, 0xBF);
			WritePort(Port.NR12, 0xF3);
			WritePort(Port.NR14, 0xBF);
			WritePort(Port.NR21, 0x3F);
			WritePort(Port.NR22, 0x00);
			WritePort(Port.NR24, 0xBF);
			WritePort(Port.NR30, 0x7F);
			WritePort(Port.NR31, 0xFF);
			WritePort(Port.NR32, 0x9F);
			WritePort(Port.NR34, 0xBF);
			WritePort(Port.NR41, 0xFF);
			WritePort(Port.NR42, 0x00);
			WritePort(Port.NR43, 0x00);
			WritePort(Port.NR44, 0xBF);
			WritePort(Port.NR50, 0x77);
			WritePort(Port.NR51, 0xF3);
			WritePort(Port.NR52, 0xF1);

			WritePort(Port.LCDC, useBootRom ? (byte)0x00 : (byte)0x91);
			WritePort(Port.STAT, 0x00);
			//WritePort(Port.LY, 0x00);
			WritePort(Port.SCY, 0x00);
			WritePort(Port.SCX, 0x00);
			WritePort(Port.LYC, 0x00);
			WritePort(Port.BGP, useBootRom ? (byte)0xFF : (byte)0xFC);
			WritePort(Port.OBP0, 0xFF);
			WritePort(Port.OBP1, 0xFF);
			WritePort(Port.WY, 0x00);
			WritePort(Port.WX, 0x00);
			WritePort(Port.IE, 0x00);

			if (colorHardware && !useBootRom) ApplyAutomaticPalette();

			if (colorHardware) WritePort(Port.PMAP, colorMode ? (byte)0xFE : (byte)0xFF);
			WritePort(Port.U72, 0x00);
			WritePort(Port.U73, 0x00);
			WritePort(Port.U74, 0x00);
			WritePort(Port.U75, 0x8F);

			videoPortAccessList.Clear();
			paletteAccessList.Clear();
			audioPortAccessList.Clear();

			videoStatusSnapshot.Capture();
		}

		private void ApplyAutomaticPalette()
		{
			if (romInformation == null || romInformation.AutomaticColorPalette == null) return;

			var palette = RomInformation.AutomaticColorPalette.Value;

			WritePort(Port.BCPS, 0x80);
			WritePort(Port.OCPS, 0x80);

			WritePalette(Port.BCPD, palette.BackgroundPalette);
			WritePalette(Port.OCPD, palette.ObjectPalette0);
			WritePalette(Port.OCPD, palette.ObjectPalette1);
		}

		private void WritePalette(Port port, ColorPalette palette)
		{
			for (int i = 0; i < 4; i++)
			{
				short value = palette[i];

				WritePort(port, (byte)value);
				WritePort(port, (byte)(value >> 8));
			}
		}

		#endregion

		#region Port I/O

		public byte ReadPort(Port port) { return ReadPort((byte)port); }

		public void WritePort(Port port, byte value) { WritePort((byte)port, value); }

		public unsafe void WritePort(byte port, byte value)
		{
			switch (port)
			{
				// Joypad
				case 0x00: // JOYP
					WriteJoypadRegister(value);
					break;
				// Timer Ports
				case 0x04: // DIV
					ResetDivider();
					break;
				case 0x05: // TIMA
					SetTimerValue(value);
					break;
				case 0x07: // TAC
					// Check the timer enable bit
					if ((value & 0x4) != 0)
					{
						switch (value & 0x3)
						{
							case 0: EnableTimer(1024); break;
							case 1: EnableTimer(16); break;
							case 2: EnableTimer(64); break;
							case 3: EnableTimer(256); break;
						}
					}
					else DisableTimer();
					portMemory[0x07] = value;
					break;
				case 0x0F: // IF
					requestedInterrupts = (byte)(value & 0x1F);
					break;
				case 0x4D: // KEY1
					if (colorMode) prepareSpeedSwitch = (value & 0x1) != 0;
					break;
				case 0x50: // BLCK
					// Disables the boot ROM when 0x01 or 0x11 is written, and never re-enable it again.
					if (internalRomMapped && (value & 0x1) != 0)
						UnmapInternalRom();
					break;
				case 0x70: // SVBK
					value &= 0x7;
					if (value == 0)
						value = 1;
					if (colorMode && workRamBank != value && (!hdmaActive || hdmaCurrentSourceHigh < 0xA0 || hdmaCurrentSourceHigh >= 0xE0))
					{
						workRamBank = value;
						MapWorkRamBank();
					}
					break;
				case 0x75: // Undocumented port 0x75
					portMemory[0x75] = (byte)(0x8F | value);
					break;
				case 0xFF: // IE
					portMemory[0xFF] = (byte)(value & 0x1F);
					break;
				// Video Ports
				case 0x41: // STAT
					notifyCoincidence = (value & 0x40) != 0;
					notifyMode2 = (value & 0x20) != 0;
					notifyMode1 = (value & 0x10) != 0;
					notifyMode0 = (value & 0x08) != 0;
					break;
				case 0x44: // LY
					// We use a negative offset here...
					// -4 allows the interrupt to happen 4 cycles beforce the new line
					//lyOffset = lcdCycles / -HorizontalLineDuration - 4;
					// For now leave it as read only… Need to find a game tring to reset LY.
					break;
				case 0x45: // LYC
					portMemory[0x45] = value;
					if (notifyCoincidence && value == lyRegister)
					{
						videoNotifications |= 0x01;
						InterruptRequest(0x02);
					}
					else videoNotifications &= 0x0E;
					break;
				case 0x46: // DMA
					if (value <= 0xF1) MemoryBlock.Copy(objectAttributeMemory, segmentArray[value], 0xA0);
					break;
				// Undocumented port used for controlling LCD operating mode
				case 0x4C: // LCDM
					if (colorHardware)
					{
						// This port will be set at boot time with the value of the compatibility byte in the ROM header
						// Bit 7 indicates color game boy functions
						colorMode = (value & 0x80) != 0;
						// Bit 3 indicates something too (used to enable B&W mode), but no idea about that yet.
					}
					break;
				case 0x4F: // VBK
					value &= 0x1;
					if (colorMode && !hdmaActive && videoRamBank != value)
					{
						videoRamBank = value;
						MapVideoRamBank();
					}
					break;
				case 0x51: // HDMA1
					hdmaSourceHigh = value;
					break;
				case 0x52: // HDMA2
					hdmaSourceLow = (byte)(value & 0xF0);
					break;
				case 0x53: // HDMA3
					hdmaDestinationHigh = (byte)(0x80 | value & 0x1F);
					break;
				case 0x54: // HDMA4
					hdmaDestinationLow = (byte)(value & 0xF0);
					break;
				case 0x55: // HDMA5
					if (colorMode)
					{
						if (hdmaActive)
						{
							if ((value & 0x80) == 0)
								hdmaActive = false;
						}
						else if ((value & 0x80) != 0)
						{
							hdmaActive = true;
							hdmaCurrentSourceHigh = hdmaSourceHigh;
							hdmaCurrentSourceLow = hdmaSourceLow;
							hdmaCurrentDestinationHigh = hdmaDestinationHigh;
							hdmaCurrentDestinationLow = hdmaDestinationLow;
							hdmaCurrentLength = (byte)(value & 0x7F);
						}
						else HandleDma(hdmaDestinationHigh, hdmaDestinationLow, hdmaSourceHigh, hdmaSourceLow, (byte)(value & 0x7F));
					}
					break;
				// Undocumented port used for palette synchronization
				case 0x6C: // PMAP
					if (colorHardware)
					{
						// Information from the GBC BIOS disassembly suggests than more than being a R/W register,
						// The R/W bit probably controls the color palette to grey palette mapping…
						usePaletteMapping = (value & 0x1) != 0;
						portMemory[0x6C] = (byte)(0xFE | value & 0x01);
					}
					break;
				// Tracked audio ports
				case 0x1A: // NR30
					if ((value & 0x80) != 0)
					{
						portMemory[0x1A] = 0xFF;
						portMemory[0x26] |= 0x04;
					}
					else
					{
						portMemory[0x1A] = 0x7F;
						portMemory[0x26] &= 0xFB;
					}
					break;
				case 0x26: // NR52
					portMemory[0x26] = (value & 0x80) != 0 ? (byte)(0xF0 | portMemory[0x26]) : (byte)0x00;
					break;
				// Tracked video ports
				// Only in non-cgb mode
				case 0x47: // BGP
				case 0x48: // OBP0
				case 0x49: // OBP1
					portMemory[port] = value; // Store the value in memory
					if (!colorMode)
						videoPortAccessList.Add(new PortAccess(frameCycles, port, value)); // Keep track of the write
					break;
				// Only in cgb mode
				case 0x68: // BCPS / BGPI
					if (colorHardware)
					{
						bgpInc = (value & 0x80) != 0;
						bgpIndex = value & 0x3F;
					}
					break;
				case 0x69: // BCPD / BGPD
					if (colorHardware)
					{
						paletteMemory[bgpIndex] = value;
						if (usePaletteMapping) greyPaletteUpdated = true;
						paletteAccessList.Add(new PaletteAccess(frameCycles, (byte)bgpIndex, value));
						if (bgpInc)
							bgpIndex = (bgpIndex + 1) & 0x3F;
					}
					break;
				case 0x6A: // OCPS / OBPI
					if (colorHardware)
					{
						obpInc = (value & 0x80) != 0;
						obpIndex = value & 0x3F;
					}
					break;
				case 0x6B: // OCPD / OBPD
					if (colorHardware)
					{
						paletteMemory[0x40 | obpIndex] = value;
						if (usePaletteMapping) greyPaletteUpdated = true;
						paletteAccessList.Add(new PaletteAccess(frameCycles, (byte)(0x40 | obpIndex), value));
						if (obpInc)
							obpIndex = (obpIndex + 1) & 0x3F;
					}
					break;
				// Always
				case 0x40: // LCDC
					if ((value & 0x80) == 0) DisableVideo();
					else if (!lcdEnabled)
					{
						portMemory[port] = value;
						EnableVideo();
						break;
					}
					goto case 0x42;
				case 0x42: // SCY
				case 0x43: // SCX
				case 0x4A: // WY
				case 0x4B: // WX
					videoPortAccessList.Add(new PortAccess(frameCycles, port, value)); // Keep track of the write
					goto default;
				default:
					portMemory[port] = value; // Store the value in memory
					break;
			}
		}

		public unsafe byte ReadPort(byte port)
		{
			int temp;

			switch (port)
			{
				case 0x00:
					return ReadJoypadRegister();
				case 0x04: // DIV
					return GetDividerValue();
				case 0x05: // TIMA
					return GetTimerValue();
				case 0x0F: // IF
					return RequestedInterrupts;
				case 0x41: // STAT
					if (!lcdEnabled) return (byte)(portMemory[0x41] & 0x78);
					// The check for line 143 fixes X - Xekkusu (which, oddly enough, was working with the previous emulation method…)
					// The game checks for VBLANK in an *active* loop (?!) but the VBLANK interrupt gets triggered before the virtual CPU even got a chance of readign the mode 0…
					// There may be something else i'm emulating wrong, which could cause this bug, but in the mean time, this fixes it well enough…
					// (The better fix being, if there are no other bugs, to emulate the read/write cycles correctly…)
					else if (lcdRealLine >= 144 || lcdRealLine == 143 && rasterCycles >= HorizontalLineDuration - 8)
						temp = 1;
					else
					{
						if (rasterCycles < Mode2Duration) temp = 2;
						else if (rasterCycles < Mode2Duration + Mode3Duration) temp = 3;
						else temp = 0;
					}

					if (lyRegister == portMemory[0x45])
						temp |= 0x4; // Set the coincidence bit based on coincidence

					return (byte)(portMemory[0x41] & 0x78 | temp);
				case 0x44: // LY
					return (byte)lyRegister;
				case 0x4D:
					return (byte)((doubleSpeed ? 0x80 : 0x00) | (prepareSpeedSwitch ? 0x01 : 0x00));
				case 0x4F: // VBK
					return (byte)videoRamBank;
				case 0x50: // BLCK
					return (byte)(0xFE & (internalRomMapped ? 1 : 0));
				case 0x51: // HDMA1
					return hdmaDestinationHigh;
				case 0x52: // HDMA2
					return hdmaDestinationLow;
				case 0x53: // HDMA3
					return hdmaSourceHigh;
				case 0x54: // HDMA4
					return hdmaSourceLow;
				case 0x55: // HDMA5
					return hdmaActive ? hdmaCurrentLength : (byte)0xFF;
				case 0x68: // BGPI
					return colorMode ? bgpInc ? (byte)(0x80 | bgpIndex) : (byte)bgpIndex : (byte)0xFF;
				case 0x69: // BGPD
					return colorMode ? paletteMemory[bgpIndex] : (byte)0xFF;
				case 0x6A: // OBPI
					return colorMode ? obpInc ? (byte)(0x80 | obpIndex) : (byte)obpIndex : (byte)0xFF;
				case 0x6B: // OBPD
					return colorMode ? paletteMemory[0x40 | obpIndex] : (byte)0xFF;
				case 0x6C: // Undocumented port 0x6C
					return colorMode ?
						(byte)(0xFE | portMemory[0x6C] & 0x01) :
						(byte)0xFF;
				case 0x70: // SVBK
					return (byte)workRamBank;
				case 0x75: // Undocumented port 0x75
					return (byte)(0x8F | portMemory[0x75]);
				case 0x76: // Undocumented port 0x76
				case 0x77: // Undocumented port 0x77
					return 0;
				case 0x72: // Undocumented port 0x72
				case 0x73: // Undocumented port 0x73
					return portMemory[port];
				case 0x74: // Undocumented port 0x74
					return colorMode ? portMemory[0x74] : (byte)0xFF;
				default:
					return portMemory[port];
			}
		}

		#endregion
	}
}
