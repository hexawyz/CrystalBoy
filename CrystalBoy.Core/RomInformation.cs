using System;
using System.Text;

namespace CrystalBoy.Core
{
	public sealed class RomInformation
	{
		/// <summary>Gets the name of the ROM.</summary>
		public string Name { get; }

		/// <summary>Gets the maker code indicated in the ROM.</summary>
		public string MakerCode { get; }

		/// <summary>Gets the maker name, as determined from the internal database.</summary>
		public string MakerName { get; }

		/// <summary>Gets the automatic color palette index used for displaying a non-CGB game on a CGB.</summary>
		public byte? AutomaticColorPaletteIndex => _automaticColorPaletteIndex < 192 ?
			_automaticColorPaletteIndex :
			null as byte?;

		/// <summary>Gets the automatic color palette used for displaying a non-CGB game on a CGB.</summary>
		public FixedColorPalette? AutomaticColorPalette => _automaticColorPaletteIndex < 192 ?
			PaletteData.GetPalette(_automaticColorPaletteIndex) :
			null as FixedColorPalette?;

		/// <summary>Gets a value indicating whether this ROM has support for the regular GameBoy hardware.</summary>
		public bool SupportsRegularGameBoy { get; }

		/// <summary>Gets a value indicating whether this ROM has support for the GameBoy Color hardware.</summary>
		public bool SupportsColorGameBoy { get; }

		/// <summary>Gets a value indicating whether this ROM has support for the Super GameBoy hardware.</summary>
		public bool SupportsSuperGameBoy { get; }

		/// <summary>Gets a value indicating whether the Nintendo logo is valid from a GameBoy Color bootstrap ROM's point of view.</summary>
		public bool IsLogoPartiallyValid { get; }

		/// <summary>Gets a value indicating whether the Nintendo logo is valid from a regular GameBoy ROM's point of view.</summary>
		public bool IsLogoFullyValid { get; }

		/// <summary>Gets a value indicating the type of ROM embedded in the cartidge.</summary>
		public RomType RomType { get; }

		/// <summary>Gets a value indicating the size of the ROM embedded in the cartidge.</summary>
		public int RomSize { get; }

		/// <summary>Gets a value indicating the size of the RAM embedded in the cartidge.</summary>
		public int RamSize { get; }

		/// <summary>Gets a value indicating whether the cartidge is Japanese.</summary>
		public bool IsJapanese { get; }

		/// <summary>Gets a value indicating the number of ROM banks embedded in the cartidge.</summary>
		public int RomBankCount { get; }

		/// <summary>Gets a value indicating the number of RAM banks embedded in the cartidge.</summary>
		public int RamBankCount { get; }

		/// <summary>Gets a value indicating whether the cartidge has any kind of RAM embedded.</summary>
		public bool HasRam { get; }

		/// <summary>Gets a value indicating whether the cartidge has a battery.</summary>
		public bool HasBattery { get; }

		/// <summary>Gets a value indicating whether the cartidge has a RTC timer.</summary>
		public bool HasTimer { get; }

		/// <summary>Gets a value indicating whether the cartidge has rumble features.</summary>
		public bool HasRumble { get; }

		private readonly byte _automaticColorPaletteIndex;

		public unsafe RomInformation(MemoryBlock memoryBlock)
		{
			byte* memory;

			if (memoryBlock == null)
				throw new ArgumentNullException("memoryBlock");

			memory = (byte*)memoryBlock.Pointer;

			// Defaults to black&white compatible
			SupportsRegularGameBoy = true;

			// Logo check
			// Defaults to logo ok
			bool partialLogoCheck = true;
			bool fullLogoCheck = true;
			for (int i = 0; i < NintendoLogo.Data.Length; i++)
			{
				if (memory[0x104 + i] != NintendoLogo.Data[i])
				{
					fullLogoCheck = false;
					if (i < 0x18) // GBC parses only first 24 bytes of logo data, according to PanDocs
						partialLogoCheck = false;
				}
			}

			IsLogoPartiallyValid = partialLogoCheck;
			IsLogoFullyValid = fullLogoCheck;

			// Read the licensee code
			byte internalMakerCode = memory[0x14B];
			MakerCode = internalMakerCode == 0x33 ?
				((char)memory[0x144]).ToString() + ((char)memory[0x145]).ToString() :
				internalMakerCode.ToString("X2", System.Globalization.CultureInfo.InvariantCulture);
			MakerName = MakerDictionary.GetMakerName(MakerCode);

			// Detect CGB support
			byte cgbFlag = memory[0x143];
			SupportsColorGameBoy = (cgbFlag & 0x80) != 0;
			if (SupportsColorGameBoy && ((cgbFlag & 0x40) != 0))
				SupportsRegularGameBoy = false;

			// Detect SGB support
			byte sgbFlag = memory[0x146];
			if (sgbFlag == 0x3 && internalMakerCode == 0x33)
				SupportsSuperGameBoy = true;
			

			// Parse the game name (NB: part of it may contain some kind of manufacturer code)
			var sb = new StringBuilder(SupportsColorGameBoy ? 15 : 16);
			for (int i = 0; i < sb.Capacity; i++)
			{
				byte c = memory[0x134 + i];

				if (c == 0) break;
				else sb.Append((char)c);
			}
			Name = sb.ToString();

			// Automatic palette detection
			if (!SupportsColorGameBoy)
			{
				byte titleChecksum = 0;

				for (int i = 0; i < 16; i++) titleChecksum += memory[0x134 + i];

				_automaticColorPaletteIndex = PaletteData.FindPaletteIndex(MakerCode, titleChecksum, memory[0x137]);
			}
			else _automaticColorPaletteIndex = 192;

			// Read the cartridge type
			switch (RomType = (RomType)memory[0x147])
			{
				case RomType.RomMbc3TimerRamBattery:
					HasRam = true;
					goto case RomType.RomMbc3TimerBattery;
				case RomType.RomMbc3TimerBattery:
					HasTimer = true;
					HasBattery = true;
					break;
				case RomType.RomMbc5RumbleRamBattery:
					HasBattery = true;
					goto case RomType.RomMbc5RumbleRam;
				case RomType.RomMbc5RumbleRam:
					HasRam = true;
					goto case RomType.RomMbc5Rumble;
				case RomType.RomMbc5Rumble:
					HasRumble = true;
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
					HasBattery = true;
					goto case RomType.RomRam;
				case RomType.RomRam:
				case RomType.RomMbc1Ram:
				case RomType.RomMbc2: // MBC2 has internal RAM
				case RomType.RomMbc3Ram:
				case RomType.RomMbc4Ram:
				case RomType.RomMbc5Ram:
				case RomType.RomMbc6Ram:
				case RomType.RomMmm01Ram:
					HasRam = true;
					break;
			}

			// Read the ROM size
			RomBankCount = GetRomBankCount(memory[0x148]);
			RomSize = GetRomSize(memory[0x148]);
			// Read the RAM size
			if (RomType == RomType.RomMbc2 || RomType == RomType.RomMbc2Battery)
			{
				// MBC2 has an internal RAM of 512 half-bytes, which doesn't count as external RAM, even though it has the same effect.
				RamBankCount = 1;
				RamSize = 512;
			}
			else
			{
				RamBankCount = GetRamBankCount(memory[0x149]);
				RamSize = GetRamSize(memory[0x149]);
			}

			// Read the region flag
			IsJapanese = (memory[0x150] == 0); // Should be 0x01 for non-japanese
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
	}
}
