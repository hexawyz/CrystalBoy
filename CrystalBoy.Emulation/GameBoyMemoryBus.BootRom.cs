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
using System.IO;
using System.Security.Cryptography;
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	partial class GameBoyMemoryBus
	{
		// Includes the MD5 hashes for the various known bootstrap ROMs.
		private readonly byte[] dmgRomHash = { 0x32, 0xFB, 0xBD, 0x84, 0x16, 0x8D, 0x34, 0x82, 0x95, 0x6E, 0xB3, 0xC5, 0x05, 0x16, 0x37, 0xF5, };
		private readonly byte[] sgbRomHash = { 0xD5, 0x74, 0xD4, 0xF9, 0xC1, 0x2F, 0x30, 0x50, 0x74, 0x79, 0x8F, 0x54, 0xC0, 0x91, 0xA8, 0xB4, };
		// This is the hash for the 2KB of bootstrap ROM. It does NOT include the empty space at 0x100-0x1FF.
		private readonly byte[] cgbRomHash = { 0x34, 0xA8, 0xD9, 0x94, 0x95, 0x88, 0x97, 0x74, 0x23, 0x8E, 0x50, 0x6C, 0xC4, 0x2D, 0xCA, 0xEA, };

		private bool dmgBootRomLoaded;
		private bool sgbBootRomLoaded;
		private bool cgbBootRomLoaded;

		partial void InitializeBootRom()
		{
			TryLoadRom(HardwareType.GameBoy, "dmg_rom");
			TryLoadRom(HardwareType.SuperGameBoy, "sgb_rom");
			TryLoadRom(HardwareType.GameBoyColor, "cgb_rom");
		}

		/// <summary>Tries to load the specified bootsrap ROM from assembly resources.</summary>
		/// <remarks>
		/// The various known bootstrap ROMs can be embedded in the executable image.
		/// However, they will not be distributed with the emulator.
		/// </remarks>
		/// <param name="hardwareType">Hardware whose bootstrap ROM should be loaded.</param>
		/// <param name="resourceName">Name of the resource which contain the ROM data.</param>
		private void TryLoadRom(HardwareType hardwareType, string resourceName)
		{
			var stream = typeof(GameBoyMemoryBus).Assembly.GetManifestResourceStream(typeof(GameBoyMemoryBus), resourceName);

			if (stream == null) return;

			try
			{
				byte[] buffer = new byte[stream.Length];

				if (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
					LoadBootRom(hardwareType, buffer);
			}
			finally { stream.Close(); }
		}

		/// <summary>Loads a bootstrap ROM with the specified data.</summary>
		/// <remarks>Data integrity will be checked to ensure a correct bootstrap ROM gets loaded.</remarks>
		/// <param name="hardwareType">Hardware whose bootstrap ROM should be loaded.</param>
		/// <param name="data">Data of the specified bootstrap ROM.</param>
		/// <exception cref="ArgumentOutOfRangeException">The value provided for harware type is not a valid one.</exception>
		/// <exception cref="NotSupportedException">Loading the bootstrap ROM the specified hardware type is not supported.</exception>
		/// <exception cref="InvalidDataException">Data integrity check failed.</exception>
		public unsafe void LoadBootRom(HardwareType hardwareType, byte[] data)
		{
			if (data == null) throw new ArgumentNullException();
			if (!Enum.IsDefined(typeof(HardwareType), hardwareType)) throw new ArgumentOutOfRangeException();

			byte[] referenceHash;
			int requestedLength;
			byte* destination;
			// I finally found a use for C# undocumented keywords… Yeah !
			// (There are other ways to do this if really needed, but in fact it feels kinda cleaner to use this rather than another switch statement here…)
			TypedReference romLoadedField;

			switch (hardwareType)
			{
				case HardwareType.GameBoy:
					requestedLength = 0x100;
					referenceHash = dmgRomHash;
					destination = dmgBootMemory;
					romLoadedField = __makeref(dmgBootRomLoaded);
					break;
				case HardwareType.SuperGameBoy:
					requestedLength = 0x100;
					referenceHash = sgbRomHash;
					destination = sgbBootMemory;
					romLoadedField = __makeref(sgbBootRomLoaded);
					break;
				case HardwareType.GameBoyColor:
					// Trim the GBC rom if needed (we don't need the "decorative" empty sapce)
					if (data.Length == 0x900)
					{
						byte[] tempData = new byte[0x800];

						Buffer.BlockCopy(data, 0, tempData, 0, 0x100);
						Buffer.BlockCopy(data, 0x200, tempData, 0x100, 0x700);

						data = tempData;
					}
					requestedLength = 0x800;
					referenceHash = cgbRomHash;
					destination = cgbBootMemory;
					romLoadedField = __makeref(cgbBootRomLoaded);
					break;
				default:
					throw new NotSupportedException();
			}

			var md5 = MD5.Create();

			if (data.Length != requestedLength || !Equals(md5.ComputeHash(data), referenceHash))
				throw new InvalidDataException();

			fixed (byte* dataPointer = data)
				MemoryBlock.Copy((void*)destination, (void*)dataPointer, requestedLength);

			__refvalue(romLoadedField, bool) = true;
		}

		private bool Equals(byte[] a, byte[] b)
		{
			if (a == null && b != null || b == null || a.Length != b.Length) return false;

			for (int i = 0; i < a.Length; i++)
				if (a[i] != b[i]) return false;

			return true;
		}
	}
}
