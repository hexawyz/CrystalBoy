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

namespace CrystalBoy.Emulation
{
	partial class GameBoyMemoryBus
	{
		#region Variables

		private GameBoyKeys baseKeys;
		private bool joypadRow0;
		private bool joypadRow1;

		#endregion

		#region Reset

		partial void ResetJoypad()
		{
			baseKeys = GameBoyKeys.None;
			joypadRow0 = false;
			joypadRow1 = false;
		}

		#endregion

		#region Joypad

		private void KeyRegisterWrite(byte value)
		{
			// In order for both operations to be executed, use a bitwise or, not a logical or…
			if ((joypadRow0 != (joypadRow0 = (value & 0x10) == 0))
				| (joypadRow1 != (joypadRow1 = (value & 0x20) == 0)))
			{
				if (superFunctions) SuperGameBoyKeyRegisterWrite();
				UpdateKeyRegister();
			}
		}

		private unsafe void UpdateKeyRegister()
		{
			byte oldValue, newValue;
			byte keys = joypadIndex != 0 ? (byte)additionalKeys[joypadIndex - 1] : (byte)this.baseKeys;

			if (joypadRow0 && joypadRow1)
				newValue = (byte)(~(keys | (keys >> 4)) & 0xF);
			else if (joypadRow0)
				newValue = (byte)(0x20 | ~keys & 0xF);
			else if (joypadRow1)
				newValue = (byte)(0x10 | ~(keys >> 4) & 0xF);
			else newValue = (byte)(0x3F - joypadIndex);

			if ((portMemory[0x00] & ~newValue & 0xF) != 0) // Check High-to-Low Joypad transitions
				InterruptRequest(0x10); // Request Joypad interrupt

			portMemory[0x00] = newValue;
		}

		public GameBoyKeys PressedKeys
		{
			get { return baseKeys; }
			set
			{
				baseKeys = value;
				UpdateKeyRegister();
			}
		}

		public void NotifyPressedKeys(GameBoyKeys pressedKeys)
		{
			baseKeys |= pressedKeys;
			UpdateKeyRegister();
		}

		public void NotifyReleasedKeys(GameBoyKeys releasedKeys)
		{
			baseKeys &= ~releasedKeys;
			UpdateKeyRegister();
		}

		#endregion
	}
}
