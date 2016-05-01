using CrystalBoy.Core;
using System;
using System.Threading;
using System.Windows.Forms;

namespace CrystalBoy.Emulation.Windows.Forms
{
	public abstract class ControlFocusedJoypad : ControlBasedPlugin, IJoypad
	{
		public abstract GameBoyKeys DownKeys { get; }

		protected int JoypadIndex { get; }

		public ControlFocusedJoypad(Control renderControl, int joypadIndex)
			: base (renderControl)
		{
			JoypadIndex = joypadIndex;
		}
	}
}
