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
using System.Collections.Generic;
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	partial class GameBoyMemoryBus : IPortMap
	{
		#region Variables

		// Snapshot of the video ports, used for frame rendering
		VideoPortSnapshot videoPortSnapshot;
		// Recorded video and audio port accesses, used for video and audio frame rendering
		List<PortAccess> videoPortAccessList, audioPortAccessList;

		int bgpIndex, obpIndex;
		bool bgpInc, obpInc;

		byte hdmaSourceHigh, hdmaSourceLow, hdmaDestinationHigh, hdmaDestinationLow;
		// For Horizontal Blank DMA
		int hdmaNextCycle;
		bool hdmaActive;
		byte hdmaCurrentSourceHigh, hdmaCurrentSourceLow, hdmaCurrentDestinationHigh, hdmaCurrentDestinationLow, hdmaCurrentLength;

		#endregion

		#region Initialize

		partial void InitializePorts()
		{
			this.videoPortAccessList = new List<PortAccess>();
			this.audioPortAccessList = new List<PortAccess>();
		}

		#endregion

		#region Reset

		partial void ResetPorts()
		{
			hdmaActive = false;

			WritePort(Port.JOYP, 0x30); // Reset the joypad

			WritePort(Port.TIMA, 0);
			WritePort(Port.TMA, 0);
			WritePort(Port.TAC, 0);

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

			WritePort(Port.LCDC, 0x91);
			WritePort(Port.LY, 0x00);
			WritePort(Port.SCY, 0x00);
			WritePort(Port.SCX, 0x00);
			WritePort(Port.LYC, 0x00);
			WritePort(Port.BGP, 0xFC);
			WritePort(Port.OBP0, 0xFF);
			WritePort(Port.OBP1, 0xFF);
			WritePort(Port.WY, 0x00);
			WritePort(Port.WX, 0x00);
			WritePort(Port.IE, 0x00);

			videoPortAccessList.Clear();
			audioPortAccessList.Clear();

			videoPortSnapshot = new VideoPortSnapshot(this);
		}

		#endregion

		#region Port I/O

		public byte ReadPort(Port port)
		{
			return ReadPort((byte)port);
		}

		public void WritePort(Port port, byte value)
		{
			WritePort((byte)port, value);
		}

		public unsafe void WritePort(byte port, byte value)
		{
			switch (port)
			{
				// Joypad
				case 0x00: // JOYP
					joypadRow0 = (value & 0x10) == 0;
					joypadRow1 = (value & 0x20) == 0;
					UpdateJoypad();
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
						switch (value & 0x2)
						{
							case 0: EnableTimer(1024); break;
							case 1: EnableTimer(16); break;
							case 2: EnableTimer(64); break;
							case 3: EnableTimer(256); break;
						}
					}
					else
						DisableTimer();
					portMemory[0x07] = value;
					break;
				case 0x0F: // IF
					requestedInterrupts = (byte)(value & 0x1F);
					break;
				case 0x4D: // KEY1
					if (colorMode)
						prepareSpeedSwitch = (value & 0x1) != 0;
					break;
				case 0x4F: // VBK
					value &= 0x1;
					if (colorMode && videoRamBank != value)
					{
						videoRamBank = value;
						MapVideoRamBank();
					}
					break;
				case 0x70: // SVBK
					value &= 0x7;
					if (value == 0)
						value = 1;
					if (colorMode && workRamBank != value)
					{
						workRamBank = value;
						MapWorkRamBank();
					}
					break;
				// Video Ports
				case 0x41: // STAT
					notifyCoincidence = (value & 0x40) != 0;
					notifyMode2 = (value & 0x20) != 0;
					notifyMode1 = (value & 0x10) != 0;
					notifyMode0 = (value & 0x08) != 0;
					UpdateVideoStatusInterrupt();
					break;
				case 0x44: // LY
					lyOffset = cycleCount / -HorizontalLineDuration; // We use a negative offset here...
					break;
				case 0x45: // LYC
					if (value < 154) // Valid values are between 0 and 153
						lycMinCycle = value * HorizontalLineDuration;
					else
						lycMinCycle = -1;
					if (notifyCoincidence)
						UpdateVideoStatusInterrupt();
					portMemory[0x45] = value;
					break;
				case 0x46: // DMA
					if (value <= 0xF1)
						Memory.Copy(objectAttributeMemory, segmentArray[value], 0xA0);
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

							if (cycleCount < FrameDuration - VerticalBlankDuration - HorizontalBlankDuration)
								hdmaNextCycle = cycleCount - cycleCount % HorizontalLineDuration + Mode2Duration + Mode3Duration;
							else
								hdmaNextCycle = FrameDuration + Mode2Duration + Mode3Duration;
						}
						else
							HandleDma(hdmaDestinationHigh, hdmaDestinationLow, hdmaSourceHigh, hdmaSourceLow, (byte)(value & 0x7F));
					}
					break;
				// Tracked video ports
				// Only in non-cgb mode
				case 0x47: // BGP
				case 0x48: // OBP0
				case 0x49: // OBP1
					portMemory[port] = value; // Store the value in memory
					if (!colorMode)
						videoPortAccessList.Add(new PortAccess(cycleCount, port, value)); // Keep track of the write
					break;
				// Only in cgb mode
				case 0x68: // BGPI
					bgpInc = (value & 0x80) != 0;
					bgpIndex = value & 0x3F;
					break;
				case 0x69: // BGPD
					paletteMemory[bgpIndex] = value;
					if (bgpInc)
						bgpIndex = (bgpIndex + 1) & 0x3F;
					break;
				case 0x6A: // OBPI
					obpInc = (value & 0x80) != 0;
					obpIndex = value & 0x3F;
					break;
				case 0x6B: // OBPD
					paletteMemory[0x40 | obpIndex] = value;
					if (obpInc)
						obpIndex = (obpIndex + 1) & 0x3F;
					break;
				// Always
				case 0x40: // LCDC
					if ((value & 0x80) == 0)
						DisableVideo();
					else if (!lcdEnabled)
						EnableVideo();
					goto case 0x42;
				case 0x42: // SCY
				case 0x43: // SCX
				case 0x4A: // WY
				case 0x4B: // WX
					videoPortAccessList.Add(new PortAccess(cycleCount, port, value)); // Keep track of the write
					portMemory[port] = value; // Store the value in memory
					break;
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
				case 0x04: // DIV
					return GetDividerValue();
				case 0x05: // TIMA
					return GetTimerValue();
				case 0x0F: // IF
					return requestedInterrupts;
				case 0x41: // STAT
					if (!lcdEnabled)
						return (byte)(portMemory[0x41] & 0x78);
					else if (cycleCount < VerticalBlankDuration)
						temp = 1;
					else
					{
						temp = cycleCount % HorizontalLineDuration;

						if (temp < Mode2Duration)
							temp = 2;
						else if (temp < Mode2Duration + Mode3Duration)
							temp = 3;
						else
							temp = 0;
					}

					if (cycleCount >= lycMinCycle && cycleCount < lycMinCycle + 456)
						temp |= 0x4; // Set the coincidence bit based on coincidence
					return (byte)(portMemory[0x41] & 0x78 | temp);
				case 0x44: // LY
					if (lcdEnabled)
						return (byte)(lyOffset + cycleCount / HorizontalLineDuration);
					else
						return 0;
				case 0x4D:
					return (byte)((doubleSpeed ? 0x80 : 0x00) | (prepareSpeedSwitch ? 0x01 : 0x00));
				case 0x4F: // VBK
					return (byte)videoRamBank;
				case 0x51: // HDMA1
					return hdmaDestinationHigh;
				case 0x52: // HDMA2
					return hdmaDestinationLow;
				case 0x53: // HDMA3
					return hdmaSourceHigh;
				case 0x54: // HDMA4
					return hdmaSourceLow;
				case 0x55: // HDMA5
					if (hdmaActive)
						return hdmaCurrentLength;
					else
						return 0xFF;
				case 0x68: // BGPI
					if (bgpInc)
						return (byte)(0x80 | bgpIndex);
					else
						return (byte)bgpIndex;
				case 0x69: // BGPD
					return paletteMemory[bgpIndex];
				case 0x6A: // OBPI
					if (obpInc)
						return (byte)(0x80 | obpIndex);
					else
						return (byte)obpIndex;
				case 0x6B: // OBPD
					return paletteMemory[0x40 | obpIndex];
				case 0x70: // SVBK
					return (byte)workRamBank;
				default:
					return portMemory[port];
			}
		}

		#endregion
	}
}
