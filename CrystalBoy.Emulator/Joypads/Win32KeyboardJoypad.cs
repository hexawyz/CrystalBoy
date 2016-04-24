#if PINVOKE

using CrystalBoy.Core;
using CrystalBoy.Emulation.Windows.Forms;
using System.Windows.Forms;

namespace CrystalBoy.Emulator.Joypads
{
	internal sealed class Win32KeyboardJoypad : ControlFocusedJoypad
	{
		private Form RenderForm { get; }

		public Win32KeyboardJoypad(Control renderControl, int joypadIndex)
			: base(renderControl, joypadIndex)
		{
			RenderForm = (Form)(RenderControl.TopLevelControl ?? RenderControl);
		}

		public override GameBoyKeys DownKeys
		{
			get
			{
				GameBoyKeys keys = GameBoyKeys.None;

				if (Form.ActiveForm == RenderForm)
				{
					if (IsKeyDown(Keys.Right)) keys |= GameBoyKeys.Right;
					if (IsKeyDown(Keys.Left)) keys |= GameBoyKeys.Left;
					if (IsKeyDown(Keys.Up)) keys |= GameBoyKeys.Up;
					if (IsKeyDown(Keys.Down)) keys |= GameBoyKeys.Down;
					if (IsKeyDown(Keys.X)) keys |= GameBoyKeys.A;
					if (IsKeyDown(Keys.Z)) keys |= GameBoyKeys.B;
					if (IsKeyDown(Keys.RShiftKey)) keys |= GameBoyKeys.Select;
					if (IsKeyDown(Keys.Return)) keys |= GameBoyKeys.Start;
				}

				return keys;
			}
		}

		private static bool IsKeyDown(Keys vKey) { return (NativeMethods.GetAsyncKeyState(vKey) & 0x8000) != 0; }
	}
}

#endif
