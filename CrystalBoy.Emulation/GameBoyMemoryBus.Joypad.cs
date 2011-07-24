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
	/// <summary>
	/// 
	/// </summary>
	partial class GameBoyMemoryBus
	{
		#region JoypadState Structure

		// Note: This structure does not hold any valuable data, and is merely used as an interface to GameBoyMemoryBus' internals.
		/// <summary>Represents the state of the joypad(s) of an emulated game boy system.</summary>
		/// <remarks>Four joypads are emulated.</remarks>
		public struct JoypadState
		{
			private GameBoyMemoryBus @this; // “This” will better emphazize the fact that this structure does not hold any valuable data.

			internal JoypadState(GameBoyMemoryBus bus) { this.@this = bus; }

			/// <summary>Gets or sets the pressed <see cref="CrystalBoy.Emulation.GameBoyKeys"/> for the joypad with the specified index.</summary>
			/// <value>The pressed keys.</value>
			public GameBoyKeys this[int joypadIndex]
			{
				get
				{
					if (joypadIndex < 0 || joypadIndex >= 4) throw new ArgumentOutOfRangeException("joypadIndex");

					return joypadIndex == 0 ? @this.baseKeys : @this.additionalKeys[joypadIndex - 1];
				}
				set
				{
					if (joypadIndex < 0 || joypadIndex >= 4) throw new ArgumentOutOfRangeException("joypadIndex");

					if (joypadIndex == 0) @this.baseKeys = value;
					else @this.additionalKeys[joypadIndex - 1] = value;
					if (joypadIndex == @this.joypadIndex) @this.UpdateJoypadRegister();
				}
			}

			public void NotifyPressedKeys(GameBoyKeys pressedKeys)
			{
				@this.baseKeys |= pressedKeys;
				if (@this.joypadIndex == 0) @this.UpdateJoypadRegister();
			}

			public void NotifyPressedKeys(int joypadIndex, GameBoyKeys pressedKeys)
			{
				if (joypadIndex < 0 || joypadIndex >= 4) throw new ArgumentOutOfRangeException("joypadIndex");

				if (joypadIndex == 0) @this.baseKeys |= pressedKeys;
				else @this.additionalKeys[joypadIndex - 1] |= pressedKeys;
				if (joypadIndex == @this.joypadIndex) @this.UpdateJoypadRegister();
			}

			public void NotifyReleasedKeys(GameBoyKeys releasedKeys)
			{
				@this.baseKeys &= ~releasedKeys;
				if (@this.joypadIndex == 0) @this.UpdateJoypadRegister();
			}

			public void NotifyReleasedKeys(int joypadIndex, GameBoyKeys releasedKeys)
			{
				if (joypadIndex < 0 || joypadIndex >= 4) throw new ArgumentOutOfRangeException("joypadIndex");

				if (joypadIndex == 0) @this.baseKeys &= ~releasedKeys;
				else @this.additionalKeys[joypadIndex - 1] &= releasedKeys;
				if (joypadIndex == @this.joypadIndex) @this.UpdateJoypadRegister();
			}
		}

		#endregion

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

		/// <summary>Gets or sets the pressed keys for the main joypad.</summary>
		/// <remarks>
		/// The main joypad is the one and only joypad on game boy hardware.
		/// Only Super Game Boy hardware allows up to four joypads in SGB mode, thanks to the SNES hardware.
		/// </remarks>
		/// <value>The pressed keys.</value>
		public GameBoyKeys PressedKeys
		{
			get { return baseKeys; }
			set
			{
				baseKeys = value;
				if (joypadIndex == 0) UpdateJoypadRegister();
			}
		}

		/// <summary>Gets access to the joypad state of this instance.</summary>
		/// <remarks>Use the returned value to read or update the state of any of the four joypads.</remarks>
		/// <value>The <see cref="JoypadState"/> for this instance.</value>
		public JoypadState Joypads { get { return new JoypadState(); } }

		#endregion
	}
}
