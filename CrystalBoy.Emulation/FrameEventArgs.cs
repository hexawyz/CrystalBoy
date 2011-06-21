using System;

namespace CrystalBoy.Emulation
{
	public sealed class FrameEventArgs : EventArgs
	{
		internal FrameEventArgs() { }

		internal void Reset() { SkipFrame = false; }

		public bool SkipFrame { get; set; }
	}
}
