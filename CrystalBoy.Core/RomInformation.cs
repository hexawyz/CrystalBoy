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

namespace CrystalBoy.Core
{
	public sealed class RomInformation
	{
		private string name, makerCode, makerName;
		private int romSize, ramSize, romBankCount, ramBankCount;
		private bool regularSupport, cgbSupport, sgbSupport, japanese;
		private bool partialLogoCheck, fullLogoCheck;
		private bool hasRam, hasBattery, hasTimer, hasRumble;
		private byte automaticColorPaletteIndex;
		private RomType romType;

		public unsafe RomInformation(MemoryBlock memoryBlock)
		{
			byte* memory;
			int maxNameLength;
			int makerCode;
			byte cgbFlag, sgbFlag;

			if (memoryBlock == null)
				throw new ArgumentNullException("memoryBlock");

			memory = (byte*)memoryBlock.Pointer;

			// Defaults to black&white compatible
			regularSupport = true;

			// Logo check
 			// Defaults to logo ok
			partialLogoCheck = fullLogoCheck = true;
			for (int i = 0; i < NintendoLogo.Data.Length; i++)
				if (memory[0x104 + i] != NintendoLogo.Data[i])
				{
					fullLogoCheck = false;
					if (i < 0x18) // GBC parses only first 24 bytes of logo data, according to PanDocs
						partialLogoCheck = false;
				}

			// Detect CGB support
			cgbFlag = memory[0x143];
			cgbSupport = (cgbFlag & 0x80) != 0;
			if (cgbSupport && ((cgbFlag & 0x40) != 0))
				regularSupport = false;

			// Detect SGB support
			sgbFlag = memory[0x146];
			if (sgbFlag == 0x3)
				sgbSupport = true;

			// Read the name
			if (cgbSupport) maxNameLength = 15;
			else maxNameLength = 16;

			name = "";

			for (int i = 0; i < maxNameLength; i++)
			{
				byte c = memory[0x134 + i];

				if (c == 0) break;
				else name += (char)c;
			}

			// Read the licensee code
			makerCode = memory[0x14B];
			this.makerCode = string.Intern
			(
				makerCode == 0x33 ?
					((char)memory[0x144]).ToString() + ((char)memory[0x145]).ToString() :
					makerCode.ToString("X2", System.Globalization.CultureInfo.InvariantCulture)
			);
			makerName = MakerDictionary.GetMakerName(this.makerCode);

			// Automatic palette detection
			if (!cgbSupport)
			{
				byte titleChecksum = 0;

				for (int i = 0; i < 16; i++) titleChecksum += memory[0x134 + i];

				automaticColorPaletteIndex = PaletteData.FindPaletteIndex(this.makerCode, titleChecksum, memory[0x137]);
			}
			else automaticColorPaletteIndex = 192;

			// Read the cartridge type
			switch (romType = (RomType)memory[0x147])
			{
				case RomType.RomMbc3TimerRamBattery:
					hasRam = true;
					goto case RomType.RomMbc3TimerBattery;
				case RomType.RomMbc3TimerBattery:
					hasTimer = true;
					hasBattery = true;
					break;
				case RomType.RomMbc5RumbleRamBattery:
					hasBattery = true;
					goto case RomType.RomMbc5RumbleRam;
				case RomType.RomMbc5RumbleRam:
					hasRam = true;
					goto case RomType.RomMbc5Rumble;
				case RomType.RomMbc5Rumble:
					hasRumble = true;
					break;
				case RomType.RomRamBattery:
				case RomType.RomMbc1RamBattery:
				case RomType.RomMbc2Battery:
				case RomType.RomMbc3RamBattery:
				case RomType.RomMbc4RamBattery:
				case RomType.RomMbc5RamBattery:
				case RomType.RomMbc7RamBattery:
				case RomType.RomMmm01RamBattery:
				case RomType.RomHuC1RamBattery:
					hasBattery = true;
					goto case RomType.RomRam;
				case RomType.RomRam:
				case RomType.RomMbc1Ram:
				case RomType.RomMbc2: // MBC2 has internal RAM
				case RomType.RomMbc3Ram:
				case RomType.RomMbc4Ram:
				case RomType.RomMbc5Ram:
				case RomType.RomMbc6Ram:
				case RomType.RomMmm01Ram:
					hasRam = true;
					break;
			}

			// Read the ROM size
			romBankCount = GetRomBankCount(memory[0x148]);
			romSize = GetRomSize(memory[0x148]);
			// Read the RAM size
			if (romType == RomType.RomMbc2 || romType == RomType.RomMbc2Battery)
			{
				// MBC2 has an internal RAM of 512 half-bytes, which doesn't count as external RAM, even though it has the same effect.
				ramBankCount = 1;
				ramSize = 512;
			}
			else
			{
				ramBankCount = GetRamBankCount(memory[0x149]);
				ramSize = GetRamSize(memory[0x149]);
			}

			// Read the region flag
			japanese = (memory[0x150] == 0); // Should be 0x01 for non-japanese
		}

		private static int GetRomBankCount(int sizeFlag)
		{
			if (sizeFlag <= 0x7)
				return 2 << sizeFlag;
			else
				switch (sizeFlag)
				{
					case 0x52:
						return 72;
					case 0x53:
						return 80;
					case 0x54:
						return 96;
					default:
						return -1;
				}
		}

		private static int GetRomSize(int sizeFlag)
		{
			int bankCount = GetRomBankCount(sizeFlag);

			if (bankCount > 0)
				return bankCount * 16384;
			else
				return -1;
		}

		private static int GetRamBankCount(int sizeFlag)
		{
			switch (sizeFlag)
			{
				case 0: // 0 Kb (none)
					return 0;
				case 1: // 2 Kb (1 bank)
					return 1;
				case 2: // 8 Kb (1 bank)
					return 1;
				case 3: // 32 Kb (4 banks)
					return 4;
				case 4: // 128 Kb (16 banks)
					return 16;
				default:
					return -1;
			}
		}

		private static int GetRamSize(int sizeFlag)
		{
			switch (sizeFlag)
			{
				case 0: // 0 Kb (none)
					return 0;
				case 1: // 2 Kb (1 bank)
					return 2048;
				case 2: // 8 Kb (1 bank)
					return 8192;
				case 3: // 32 Kb (4 banks)
					return 32768;
				case 4: // 128 Kb (16 banks)
					return 131072;
				default:
					return -1;
			}
		}

		public string Name { get { return name; } }

		public string MakerCode { get { return makerCode; } }

		public string MakerName { get { return makerName; } }

		public byte? AutomaticColorPaletteIndex { get { return automaticColorPaletteIndex < 192 ? automaticColorPaletteIndex : (byte?)null; } }

		public FixedColorPalette? AutomaticColorPalette { get { return automaticColorPaletteIndex < 192 ? PaletteData.GetPalette(automaticColorPaletteIndex) : (FixedColorPalette?)null; } }

		public bool RegularGameBoySupport { get { return regularSupport; } }

		public bool ColorGameBoySupport { get { return cgbSupport; } }

		public bool SuperGameBoySupport { get { return sgbSupport; } }

		public bool PartialLogoCheck { get { return partialLogoCheck; } }

		public bool FullLogoCheck { get { return fullLogoCheck; } }

		public RomType RomType { get { return romType; } }

		public int RomSize { get { return romSize; } }

		public int RamSize { get { return ramSize; } }

		public bool IsJapanese { get { return japanese; } }

		public int RomBankCount { get { return romBankCount; } }

		public int RamBankCount { get { return ramBankCount; } }

		public bool HasRam { get { return hasRam; } }

		public bool HasBattery { get { return hasBattery; } }

		public bool HasTimer { get { return hasTimer; } }

		public bool HasRumble { get { return hasRumble; } }
	}
}
