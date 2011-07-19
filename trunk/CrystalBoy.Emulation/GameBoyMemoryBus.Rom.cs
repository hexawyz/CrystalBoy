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
	partial class GameBoyMemoryBus
	{
		#region Variables

		RomInformation romInformation;
		Mapper mapper;
		bool romLoaded;

		#endregion

		#region Properties

		public Mapper Mapper { get { return mapper; } }

		#endregion

		#region Reset

		partial void ResetRom()
		{
			if (mapper != null)
				mapper.Reset();
		}

		#endregion

		#region ROM Information

		public RomInformation RomInformation { get { return romInformation; } }

		public bool RomLoaded { get { return romLoaded; } }

		#endregion

		#region ROM Loading / Unloading

		public void LoadRom(MemoryBlock externalRom)
		{
			RomInformation romInformation;

			if (externalRom == null) throw new ArgumentNullException("externalRom");

			if ((externalRom.Length & 0x3FFF) != 0
				|| (externalRom.Length >> 14) > 256)
				throw new InvalidOperationException();

			romInformation = new RomInformation(externalRom);

			if (romInformation.RomSize != externalRom.Length) throw new InvalidOperationException();

			Mapper mapper;

			switch (romInformation.RomType)
			{
				case RomType.RomOnly:
				case RomType.RomRam:
				case RomType.RomRamBattery:
					mapper = new Mappers.RomController(this);
					break;
				case RomType.RomMbc1:
				case RomType.RomMbc1Ram:
				case RomType.RomMbc1RamBattery:
					mapper = new Mappers.MemoryBankController1(this);
					break;
				case RomType.RomMbc2:
				case RomType.RomMbc2Battery:
					mapper = new Mappers.MemoryBankController2(this);
					break;
				case RomType.RomMbc3:
				case RomType.RomMbc3Ram:
				case RomType.RomMbc3RamBattery:
				case RomType.RomMbc3TimerBattery:
				case RomType.RomMbc3TimerRamBattery:
					mapper = new Mappers.MemoryBankController3(this);
					break;
				case RomType.RomMbc5:
				case RomType.RomMbc5Ram:
				case RomType.RomMbc5RamBattery:
				case RomType.RomMbc5Rumble:
				case RomType.RomMbc5RumbleRam:
				case RomType.RomMbc5RumbleRamBattery:
					mapper = new Mappers.MemoryBankController5(this);
					break;
				default:
					throw new NotSupportedException("Unsupported Cartidge Type");
			}

#if WITH_THREADING
			SuspendEmulation();
#endif

			this.romInformation = romInformation;
			this.externalRomBlock = externalRom;
			this.mapper = mapper;
			this.colorMode = ColorHardware & romInformation.ColorGameBoySupport;

#if WITH_DEBUGGING
			ClearBreakpoints();
#endif
			Reset(); // Will call “ResumeEmulation”…

			// Fills the external RAM with random data.
			// It can be loaded with real data later.
			unsafe { RandomFill((byte*)externalRamBlock.Pointer, externalRamBlock.Length); }

			romLoaded = true;
		}

		public void UnloadRom()
		{
			if (externalRomBlock != null)
			{
#if WITH_THREADING
				SuspendEmulation();
#endif
				this.mapper = null;
				this.externalRomBlock = null;
				this.romInformation = null;
				this.colorMode = ColorHardware;
#if WITH_DEBUGGING
				ClearBreakpoints();
#endif
				Reset(); // Will call “ResumeEmulation”…
			}
		}

		#endregion
	}
}
