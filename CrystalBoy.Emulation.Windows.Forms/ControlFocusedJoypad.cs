using CrystalBoy.Core;
using System;
using System.Threading;
using System.Windows.Forms;

namespace CrystalBoy.Emulation.Windows.Forms
{
	public abstract class ControlFocusedJoypad : IJoypad
	{
		public abstract GameBoyKeys DownKeys { get; }

		protected int JoypadIndex { get; }
		protected Control RenderControl { get; }
		protected SynchronizationContext SynchronizationContext { get; }

		public ControlFocusedJoypad(Control renderControl, int joypadIndex)
		{
			if (renderControl == null) throw new ArgumentNullException(nameof(renderControl));

			JoypadIndex = joypadIndex;
			RenderControl = renderControl;
			SynchronizationContext = renderControl.InvokeRequired ?
				(SynchronizationContext)renderControl.Invoke((Func<SynchronizationContext>)(() => SynchronizationContext.Current)) :
				SynchronizationContext.Current;
		}

		public virtual void Dispose()
		{
		}
	}
}
