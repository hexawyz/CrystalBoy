using CrystalBoy.Core;
using CrystalBoy.Emulation.Windows.Forms;
using SharpDX.XInput;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace CrystalBoy.Emulator.Rendering.SharpDX
{
	[DisplayName("XInput")]
	[Description("Emulates a joypad with an XInput controller.")]
	public sealed class XInputJoypad : ControlFocusedJoypad
	{
		private readonly Controller _controller;

		public XInputJoypad(Control renderControl, int joypadIndex)
			: base(renderControl, joypadIndex)
		{
			if (joypadIndex < 0 || joypadIndex > 3) throw new ArgumentOutOfRangeException(nameof(joypadIndex));

			_controller = new Controller((UserIndex)joypadIndex);
		}

		public override GameBoyKeys DownKeys
		{
			get
			{
				GameBoyKeys keys = GameBoyKeys.None;
				State state;

				if (_controller.GetState(out state))
				{
					if ((state.Gamepad.Buttons & GamepadButtonFlags.DPadRight) != 0) keys |= GameBoyKeys.Right;
					if ((state.Gamepad.Buttons & GamepadButtonFlags.DPadLeft) != 0) keys |= GameBoyKeys.Left;
					if ((state.Gamepad.Buttons & GamepadButtonFlags.DPadUp) != 0) keys |= GameBoyKeys.Up;
					if ((state.Gamepad.Buttons & GamepadButtonFlags.DPadDown) != 0) keys |= GameBoyKeys.Down;
					if ((state.Gamepad.Buttons & GamepadButtonFlags.A) != 0) keys |= GameBoyKeys.A;
					if ((state.Gamepad.Buttons & GamepadButtonFlags.B) != 0) keys |= GameBoyKeys.B;
					if ((state.Gamepad.Buttons & GamepadButtonFlags.Start) != 0) keys |= GameBoyKeys.Start;
					if ((state.Gamepad.Buttons & GamepadButtonFlags.Back) != 0) keys |= GameBoyKeys.Select;
				}

				return keys;
			}
		}
	}
}
