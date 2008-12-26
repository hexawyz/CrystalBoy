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
using System.IO;
using System.Collections.Generic;
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	partial class GameBoyMemoryBus
	{
		#region Constants

		const int snapSignature = 0x53454243; // 'CBES'
		const short snapVersion = 0x0100; // 1.00

		#endregion

		// Work in progress....
		public void LoadState(Stream stream)
		{
			BinaryReader reader;
			short version;

			if (!romLoaded)
				throw new InvalidOperationException();

			reader = new BinaryReader(stream);

			// Compare the signatures
			if (reader.ReadInt32() != snapSignature)
				throw new InvalidDataException();

			// Compare the versions (currently no support for old versions, because they don't exist at all)
			if ((version = reader.ReadInt16()) != snapVersion)
				throw new InvalidDataException();

			// Check ROM compatibility
			if ((RomType)reader.ReadInt16() != romInformation.RomType)
				throw new InvalidDataException();
			if (reader.ReadInt32() != romInformation.RomSize)
				throw new InvalidDataException();
			if (reader.ReadInt32() != mapper.RamSize)
				throw new InvalidDataException();

			// General hardware information
			hardwareType = (HardwareType)reader.ReadByte();
			colorMode = reader.ReadBoolean();

			// Load CPU state information
			processor.AF = reader.ReadUInt16();
			processor.BC = reader.ReadUInt16();
			processor.DE = reader.ReadUInt16();
			processor.HL = reader.ReadUInt16();
			processor.SP = reader.ReadUInt16();
			processor.PC = reader.ReadUInt16();
			processor.InterruptMasterEnable = reader.ReadBoolean();
			requestedInterrupts = reader.ReadByte();
			prepareSpeedSwitch = reader.ReadBoolean() && colorMode;
			doubleSpeed = reader.ReadBoolean() && colorMode;

			// Load banking information
			lowerRomBank = reader.ReadInt32();
			upperRombank = reader.ReadInt32();
			if (colorMode)
			{
				// Internal banking works only when in cgb mode
				videoRamBank = reader.ReadInt32() & 1;
				workRamBank = reader.ReadInt32() & 7;
			}
			else
			{
				// Ignore the banking information when not in cgb mode
				videoRamBank = 0;
				workRamBank = 1;
				reader.ReadInt64();
			}

			// Load timing information
			cycleCount = reader.ReadInt32();
			referenceTimerShift = reader.ReadInt32();
			statInterruptCycle = reader.ReadInt32();
			timerInterruptCycle = reader.ReadInt32();
			dividerBaseCycle = reader.ReadInt32();
		}

		public void SaveState(Stream stream)
		{
			BinaryWriter writer;

			if (!romLoaded)
				throw new InvalidOperationException();

			if (!stream.CanWrite)
				throw new InvalidOperationException();

			writer = new BinaryWriter(stream);

			// Write the signature 'CBES' (CrystalBoy Emulation Snapshot)
			writer.Write(snapSignature);
			// Write the format version
			writer.Write(snapVersion);

			// Save some information about the ROM (for some validity checks when loading)
			writer.Write((short)romInformation.RomType); // Save the rom type as a short, but it could fit in a byte
			writer.Write(romInformation.RomSize);
			writer.Write(mapper.RamSize);

			// General hardware information
			writer.Write((byte)hardwareType);
			writer.Write(colorMode);

			// Save CPU information
			writer.Write(processor.AF);
			writer.Write(processor.BC);
			writer.Write(processor.DE);
			writer.Write(processor.HL);
			writer.Write(processor.SP);
			writer.Write(processor.PC);
			writer.Write(processor.InterruptMasterEnable);
			writer.Write(requestedInterrupts);
			writer.Write(prepareSpeedSwitch); // Ignore this when !colorMode
			writer.Write(doubleSpeed); // Ignore this when !colorMode

			// Save banking information
			writer.Write(lowerRomBank);
			writer.Write(upperRombank);
			writer.Write(videoRamBank); // Ignore this when !colorMode
			writer.Write(workRamBank); // Ignore this when !colorMode

			// Save timing information
			writer.Write(cycleCount);
			writer.Write(referenceTimerShift);
			writer.Write(statInterruptCycle);
			writer.Write(timerInterruptCycle);
			writer.Write(dividerBaseCycle);

			// Save timer information
			writer.Write(timerEnabled);
			writer.Write(timerBaseValue);
			writer.Write(timerBaseCycle);
			writer.Write(timerResolution);

			// Save video register information
			writer.Write(lcdEnabled);
			writer.Write(lcdDrawing);
			writer.Write(statInterruptEnabled);
			writer.Write(lyOffset);
			writer.Write(lycMinCycle);
			writer.Write(notifyCoincidence);
			writer.Write(notifyMode0);
			writer.Write(notifyMode1);
			writer.Write(notifyMode2);
		}
	}
}
