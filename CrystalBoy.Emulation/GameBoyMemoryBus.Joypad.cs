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

		GameBoyKeys keys;
		bool joypadRow0,
			joypadRow1;

		#endregion

		#region Joypad

		private unsafe void UpdateJoypad()
		{
			byte oldValue, newValue;
			byte keys = (byte)this.keys;

			if (joypadRow0 && joypadRow1)
				newValue = (byte)(~(keys | (keys >> 4)) & 0xF);
			else if (joypadRow0)
				newValue = (byte)(0x20 | ~keys & 0xF);
			else if (joypadRow1)
				newValue = (byte)(0x10 | ~(keys >> 4) & 0xF);
			else
				newValue = 0x3F;

			oldValue = portMemory[0x00];
			portMemory[0x00] = newValue;

			if ((oldValue & ~newValue & 0xF) != 0) // Check High-to-Low Joypad transitions
				InterruptRequest(0x10); // Request Joypad interrupt
		}

		public GameBoyKeys PressedKeys
		{
			get
			{
				return keys;
			}
			set
			{
				keys = value;
				UpdateJoypad();
			}
		}

		public void NotifyPressedKeys(GameBoyKeys pressedKeys)
		{
			keys |= pressedKeys;
			UpdateJoypad();
		}

		public void NotifyReleasedKeys(GameBoyKeys releasedKeys)
		{
			keys &= ~releasedKeys;
			UpdateJoypad();
		}

		#endregion
	}
}
