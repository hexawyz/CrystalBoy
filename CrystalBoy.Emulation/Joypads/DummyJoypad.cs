using CrystalBoy.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrystalBoy.Emulation.Joypads
{
	internal sealed class DummyJoypad : IJoypad
	{
		public static readonly DummyJoypad Instance = new DummyJoypad();

		private DummyJoypad() { }

		public GameBoyKeys DownKeys => GameBoyKeys.None;
	}
}
