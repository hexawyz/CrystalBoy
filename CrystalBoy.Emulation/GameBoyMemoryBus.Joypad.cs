using CrystalBoy.Core;
using CrystalBoy.Emulation.Joypads;
using System;

namespace CrystalBoy.Emulation
{
	/// <summary>
	/// 
	/// </summary>
	partial class GameBoyMemoryBus
	{
		#region Variables
		
		private volatile GameBoyKeys baseKeys;
		private volatile IJoypad joypad0;
		private volatile IJoypad joypad1;
		private volatile IJoypad joypad2;
		private volatile IJoypad joypad3;
		private bool joypadRow0;
		private bool joypadRow1;

		#endregion

		#region Initialize

		partial void InitializeJoypad()
		{
			joypad0 = DummyJoypad.Instance;
			joypad1 = DummyJoypad.Instance;
			joypad2 = DummyJoypad.Instance;
			joypad3 = DummyJoypad.Instance;
		}

		#endregion

		#region Reset

		partial void ResetJoypad()
		{
			baseKeys = GameBoyKeys.None;
			joypadRow0 = false;
			joypadRow1 = false;
		}

		#endregion

		/// <summary>Assigns a specific joypad.</summary>
		/// <param name="joypadIndex">The idnex of the joypad to assign.</param>
		/// <param name="joypad">The joypad instance to use, or null to unassign.</param>
		public void SetJoypad(int joypadIndex, IJoypad joypad)
		{
			joypad = joypad ?? DummyJoypad.Instance;

			switch (joypadIndex)
			{
				case 0: joypad0 = joypad; break;
				case 1: joypad1 = joypad; break;
				case 2: joypad2 = joypad; break;
				case 3: joypad3 = joypad; break;
			}
		}

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

		private unsafe byte ReadJoypadRegister()
		{
			GameBoyKeys keys = GameBoyKeys.None;

			switch (joypadIndex)
			{
				case 0: keys = joypad0.DownKeys; break;
				case 1: keys = joypad1.DownKeys; break;
				case 2: keys = joypad2.DownKeys; break;
				case 3: keys = joypad3.DownKeys; break;
			}

			baseKeys = keys;

			if (joypadIndex == 0) UpdateJoypadRegister();

			return portMemory[0x00];
		}

		#endregion
	}
}
