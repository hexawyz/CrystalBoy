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

		private Processor processor;

		#endregion

		#region Properties

		public Processor Processor
		{
			get
			{
				return processor;
			}
		}

		#endregion

		#region Initialize

		partial void InitializeProcessor()
		{
			processor = new Processor(this);
		}

		#endregion

		#region Reset

		partial void ResetProcessor()
		{
			processor.Reset();

			// Simulate Boot ROM behavior
			if (!useBootRom)
			{
				if (hardwareType == HardwareType.GameBoyPocket
					|| hardwareType == HardwareType.SuperGameBoy2)
					processor.AF = 0xFFB0;
				else if (hardwareType == HardwareType.GameBoyColor
					|| hardwareType == HardwareType.SuperGameBoyColor
					|| hardwareType == HardwareType.GameBoyAdvance
					|| hardwareType == HardwareType.SuperGameBoyAdvance)
					processor.AF = 0x11B0;
				else processor.AF = 0x01B0;

				if (hardwareType == HardwareType.GameBoyAdvance
					|| hardwareType == HardwareType.SuperGameBoyAdvance)
					processor.BC = 0x0113;
				else processor.BC = 0x0013;

				processor.DE = 0x00D8;
				processor.HL = 0x014D;
				processor.SP = 0xFFFE;
				processor.PC = 0x0100; // 0x100 is the start address after the boot ROM has executed
			}
		}

		#endregion
	}
}
