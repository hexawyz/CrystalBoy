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
		public sealed class JoypadState
		{
			private GameBoyMemoryBus bus;

			internal JoypadState(GameBoyMemoryBus bus) { this.bus = bus; }

			public GameBoyKeys this[int joypadIndex]
			{
				get
				{
					if (joypadIndex < 0 || joypadIndex >= 4) throw new ArgumentOutOfRangeException("joypadIndex");

					return joypadIndex == 0 ? bus.baseKeys : bus.additionalKeys[joypadIndex - 1];
				}
				set
				{
					if (joypadIndex < 0 || joypadIndex >= 4) throw new ArgumentOutOfRangeException("joypadIndex");

					if (joypadIndex == 0) bus.baseKeys = value;
					else bus.additionalKeys[joypadIndex - 1] = value;
					if (joypadIndex == bus.joypadIndex) bus.UpdateJoypadRegister();
				}
			}

			public void NotifyPressedKeys(GameBoyKeys pressedKeys)
			{
				bus.baseKeys |= pressedKeys;
				if (bus.joypadIndex == 0) bus.UpdateJoypadRegister();
			}

			public void NotifyPressedKeys(int joypadIndex, GameBoyKeys pressedKeys)
			{
				if (joypadIndex < 0 || joypadIndex >= 4) throw new ArgumentOutOfRangeException("joypadIndex");

				if (joypadIndex == 0) bus.baseKeys |= pressedKeys;
				else bus.additionalKeys[joypadIndex - 1] |= pressedKeys;
				if (joypadIndex == bus.joypadIndex) bus.UpdateJoypadRegister();
			}

			public void NotifyReleasedKeys(GameBoyKeys releasedKeys)
			{
				bus.baseKeys &= ~releasedKeys;
				if (bus.joypadIndex == 0) bus.UpdateJoypadRegister();
			}

			public void NotifyReleasedKeys(int joypadIndex, GameBoyKeys releasedKeys)
			{
				if (joypadIndex < 0 || joypadIndex >= 4) throw new ArgumentOutOfRangeException("joypadIndex");

				if (joypadIndex == 0) bus.baseKeys &= ~releasedKeys;
				else bus.additionalKeys[joypadIndex - 1] &= releasedKeys;
				if (joypadIndex == bus.joypadIndex) bus.UpdateJoypadRegister();
			}
		}

		#region Variables

		private ReadKeysEventArgs readKeysEventArgs;
		private GameBoyKeys baseKeys;
		private bool joypadRow0;
		private bool joypadRow1;

		#endregion

		#region Events

		public event EventHandler<ReadKeysEventArgs> ReadKeys;

		#endregion

		#region Initialize

		partial void InitializeJoypad() { readKeysEventArgs = new ReadKeysEventArgs(); }

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

		private void WriteJoypadRegister(byte value)
		{
			// In order for both operations to be executed, use a bitwise or, not a logical or…
			if ((joypadRow0 != (joypadRow0 = (value & 0x10) == 0))
				| (joypadRow1 != (joypadRow1 = (value & 0x20) == 0)))
			{
				if (superFunctions) SuperGameBoyKeyRegisterWrite();
				UpdateJoypadRegister();
			}
		}

		private unsafe void UpdateJoypadRegister()
		{
			byte newValue;
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

		private byte ReadJoypadRegister()
		{
			if (ReadKeys != null)
			{
				readKeysEventArgs.Reset(joypadIndex);
				ReadKeys(this, readKeysEventArgs);
			}

			unsafe { return portMemory[0x00]; }
		}

		public GameBoyKeys PressedKeys
		{
			get { return baseKeys; }
			set
			{
				baseKeys = value;
				UpdateJoypadRegister();
			}
		}

		#endregion
	}
}
