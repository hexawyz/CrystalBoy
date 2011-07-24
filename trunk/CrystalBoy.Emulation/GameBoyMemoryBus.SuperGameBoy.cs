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
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	partial class GameBoyMemoryBus
	{
		#region Variables

		private byte[] sgbCharacterData;
		private ushort[] sgbBorderMapData;
		private byte[] sgbCommandBuffer;
		private GameBoyKeys[] additionalKeys;
		private ushort sgbCommandBufferStatus;
		private byte joypadCount;
		private byte joypadIndex;
		private bool sgbPendingTransfer;
		private bool sgbCustomBorder;
		private byte sgbScreenStatus;
		private byte sgbStatus;

		#endregion

		#region Events

		public event EventHandler BorderChanged;

		private NotificationHandler borderChangedHandler;

		#endregion

		#region Initialize

		partial void InitializeSuperGameBoy()
		{
			borderChangedHandler = OnBorderChanged;
			additionalKeys = new GameBoyKeys[3];
			sgbCharacterData = new byte[0x2000];
			sgbBorderMapData = new ushort[0x800]; // This will also include palettes 4 - 7
			sgbCommandBuffer = new byte[7 * 16];
		}

		#endregion

		#region Reset

		partial void ResetSuperGameBoy()
		{
			joypadCount = 1;
			joypadIndex = 0;
			for (int i = 0; i < additionalKeys.Length; i++)
				additionalKeys[i] = GameBoyKeys.None;
			Array.Clear(sgbCharacterData, 0, sgbCharacterData.Length);
			Array.Clear(sgbBorderMapData, 0, sgbBorderMapData.Length);
			Array.Clear(sgbCommandBuffer, 0, sgbCommandBuffer.Length);
			sgbCommandBufferStatus = 0;
			sgbPendingTransfer = false;
			sgbCustomBorder = false;
			sgbScreenStatus = 0;
			sgbStatus = useBootRom ? (byte)6 : (byte)0;
		}

		#endregion

		public bool HasCustomBorder { get { return sgbCustomBorder; } }

		private void OnBorderChanged(EventArgs e) { if (BorderChanged != null) BorderChanged(this, e); }

		private void SuperGameBoyKeyRegisterWrite()
		{
			if (sgbPendingTransfer) return;

			// A sgb packet is one reset pulse plus 128 bits plus one stop bit.
			// Up to seven packets can be transferred.
			// Each packet is 16 bytes long, with the first byte of the first apcket indicating the command and transfert length.

			// TODO: find what is the exact hardware behavior on invalid SGB packets. Are they detected late or early ? (Currently, they are emulated as detected late)
			// NOTE: The fields joypadRow0 and joypadRow1 are inversed compared to GB's P14/P15.

			// Get the packet index and packet offset
			int packetIndex = sgbCommandBufferStatus / 260;
			int packetOffset = packetIndex << 4;
			// Get the packet-local status
			int commandBufferStatus = sgbCommandBufferStatus % 260;

			if (joypadRow0 && joypadRow1) // Handle the “RESET” command
			{
				// Totally reset the buffering if we are in an uninitialized state (0) or an invalid state (reset is triggered in the middle of a packet)
				if (commandBufferStatus != 0 || sgbCommandBufferStatus == 0)
				{
					sgbCommandBufferStatus = 1;
					Array.Clear(sgbCommandBuffer, 0, sgbCommandBuffer.Length);
				}
				// Increment the status word only if moving to the next data packet
				else sgbCommandBufferStatus++;
			}
			else if (sgbCommandBufferStatus > 0)
			{
				if (!(joypadRow0 || joypadRow1))
				{
					if ((sgbCommandBufferStatus & 0x1) != 0)
					{
						sgbCommandBufferStatus++;
						// Handle the last bit (end of packet)
						// If we are at the last packet in the series, reset the buffering.
						if (commandBufferStatus == 259)
						{
							// Decrease the SGB status flag (used for bootstrap ROM emulation)
							if (sgbStatus != 0) sgbStatus--; // The bootstrap ROM will send 6 packets not following the usual command packet format…

							if (sgbStatus == 0 && packetIndex + 1 >= (sgbCommandBuffer[0] & 0x7))
							{
								ProcessSuperGameBoyCommand(false);
								sgbCommandBufferStatus = 0;
							}
						}
					}
				}
				else if ((sgbCommandBufferStatus & 0x1) == 0)
				{
					int bitOffset = (commandBufferStatus >> 1) - 1;
					int byteOffset = bitOffset >> 3;

					sgbCommandBufferStatus++;

					// If row 1 is down (then row 0 is up, and we know that from the previous tests), set the bit in the buffer
					if (joypadRow1)
						// The last bit of a packet (128) must be 0 (stop bit). Ignore the packet if this is not the case…
						if (commandBufferStatus == 258 && joypadRow0) sgbCommandBufferStatus = 0;
						else sgbCommandBuffer[packetOffset + byteOffset] |= (byte)(1 << (bitOffset & 0x7));
				}
			}
			else if (!(joypadRow0 || joypadRow1)) joypadIndex = (byte)((joypadIndex + 1) & (joypadCount - 1));
		}

		private void ProcessSuperGameBoyCommand(bool doNotWait)
		{
			byte commandLength = (byte)((sgbCommandBuffer[0] & 0x7));
			SuperGameBoyCommand command = (SuperGameBoyCommand)(sgbCommandBuffer[0] >> 3);

			// Ignore packets with invalid length (Should they be processed ?)
			if (commandLength < 1) return;

			// Note that when called with “doNotWait” set to true, the value of “sgbCommandBufferStatus” will always be 0…
			switch (command)
			{
				case SuperGameBoyCommand.PAL01:
					break;
				case SuperGameBoyCommand.PAL23:
					break;
				case SuperGameBoyCommand.PAL03:
					break;
				case SuperGameBoyCommand.PAL12:
					break;
				case SuperGameBoyCommand.ATTR_BLK:
					break;
				case SuperGameBoyCommand.ATTR_LIN:
					break;
				case SuperGameBoyCommand.ATTR_DIV:
					break;
				case SuperGameBoyCommand.ATTR_CHR:
					break;
				case SuperGameBoyCommand.ATTR_TRN:
					break;
				case SuperGameBoyCommand.SOUND: break; // Not emulated yet, ignore…
				case SuperGameBoyCommand.SOU_TRN: break; // Not emulated yet, ignore…
				case SuperGameBoyCommand.PAL_SET:
					break;
				case SuperGameBoyCommand.PAL_TRN:
					break;
				case SuperGameBoyCommand.MLT_REQ:
					joypadCount = (sgbCommandBuffer[1] & 0x1) != 0 ? (sgbCommandBuffer[1] & 0x2) != 0 ? (byte)4 : (byte)2 : (byte)1;
					joypadIndex = (sgbCommandBuffer[1] & 0x3) == 0x3 ? (byte)0 : (byte)1;
					break;
				case SuperGameBoyCommand.CHR_TRN:
					if (doNotWait) SgbCharacterTransfer((sgbCommandBuffer[1] & 0x1) != 0);
					else sgbPendingTransfer = true;
					break;
				case SuperGameBoyCommand.PCT_TRN:
					if (doNotWait) SgbPictureTransfer();
					else sgbPendingTransfer = true;
					break;
				case SuperGameBoyCommand.MASK_EN:
					// Need to add a flag to prevent swapping the rendering buffers, and re-render the previous frame.
					sgbScreenStatus = (byte)(sgbCommandBuffer[1] & 0x3);
					break;
				// Commands specific to real SGB hardware, and thus, not emulated.
				// Those would require emulating a real SNES, and a real SGB with its SNES side and its internal GB side…
				case SuperGameBoyCommand.ATRC_EN: // No attraction mode… (Screensaver)
				case SuperGameBoyCommand.ICON_EN: // No icon tweaking… (Menus)
				case SuperGameBoyCommand.DATA_SND: // No writing data at a random place in SNES memory…
				case SuperGameBoyCommand.DATA_TRN: // No writing data at a random place in SNES memory…
				case SuperGameBoyCommand.JUMP: // No jumping into native SNES code…
					break;
				// SGB GB Bootstrap ROM “commands” used to transfer game information to the SNES ROM…
				case (SuperGameBoyCommand)0x1E:
				//case (SuperGameBoyCommand)0x1F: // We don't have to process this, since it will be included in the 6 packet-long buffer…
					// This code can only be reached by the means of SGB “BIOS” emulation, because such a configuration cannot be faked by real GB code.
					if (sgbCommandBuffer[0] == 0xF1 && sgbCommandBufferStatus == 1560) // The first byte indicates a packet length of 1 but the internal counter gives the real length of the data…
						superFunctions = sgbCommandBuffer[0x53] == 0x33 && sgbCommandBuffer[0x4C] == 0x03; // This may disable the SGB functions or keep them enabled, depending on the received data…
					break;
				//default:
				//    throw new InvalidOperationException("Unknown SGB command: 0x" + command.ToString("X"));
			}
		}

		private unsafe void SgbCharacterTransfer(bool high)
		{
			fixed (byte* sgbCharacterDataPointer = sgbCharacterData)
				SgbVideoTransfer(high ? (void*)(sgbCharacterDataPointer + 0x1000) : (void*)sgbCharacterDataPointer, videoStatusSnapshot.VideoMemory);
		}

		private unsafe void SgbPictureTransfer()
		{
			fixed (ushort* sgbBorderMapDataPointer = sgbBorderMapData)
				SgbVideoTransfer((void*)sgbBorderMapDataPointer, videoStatusSnapshot.VideoMemory);

			RenderBorder();

			PostUINotification(borderChangedHandler);
		}

		private unsafe void SgbVideoTransfer(void* destination, void* videoMemory)
		{
			// This is a very approximate emulation, but it should work OK in most cases… (I hope)
			// A real emulation would require to actually wait for the next frame and then render it specifically in black (and gray) and white, then recompose the data from that.
			// As this would require to write a specific rendering method, outputting to a 2bpp buffer, I'd rather finalize the implementation of the actual render methods before adding a third one…

			byte lcdc = ReadPort(Port.LCDC);

			ulong* dst = (ulong*)destination;
			byte* bgMap = (lcdc & 0x08) != 0 ? (byte*)videoMemory + 0x1C00: (byte*)videoMemory + 0x1800;

			for (int i = 13; i-- != 0; )
			{
				for (int j = i != 0 ? 20 : 16; j-- != 0; )
				{
					// Assume the data is aligned, and copy one tile as two QWORD.
					// This seems to be a safe guess (malloc will always alloc aligned memory blocks, right ?), and should work in all cases on x86… (Since it allows for unaligned accesses)
					ulong* src = (ulong*)((byte*)videoMemory + (((lcdc & 0x10) != 0 ? *bgMap++ : 0x100 + unchecked((sbyte)*bgMap++)) << 4));

					*dst++ = *src++;
					*dst++ = *src;
				}

				bgMap += 12;
			}
		}

		internal byte SuperGameBoyScreenStatus { get { return sgbScreenStatus; } }
	}
}
