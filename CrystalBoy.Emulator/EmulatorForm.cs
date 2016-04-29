using System;
using System.Windows.Forms;

namespace CrystalBoy.Emulator
{
	class EmulatorForm : DpiAwareForm
	{
		EmulatedGameBoy emulatedGameBoy;

		private EmulatorForm()
		{
		}

		public EmulatorForm(EmulatedGameBoy emulatedGameBoy)
		{
			if (emulatedGameBoy == null)
				throw new ArgumentNullException("emulatedGameBoy");
			this.emulatedGameBoy = emulatedGameBoy;
			this.emulatedGameBoy.AfterReset += OnAfterResetInternal;
			this.emulatedGameBoy.RomChanged += OnRomChangedInternal;
			this.emulatedGameBoy.Paused += OnPausedInternal;
			this.emulatedGameBoy.Break += OnBreakInternal;
			this.emulatedGameBoy.EmulationStatusChanged += OnEmulationStatusChangedInternal;
			this.emulatedGameBoy.NewFrame += OnNewFrameInternal;
		}

		protected EmulatedGameBoy EmulatedGameBoy { get { return emulatedGameBoy; } }

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				Hide();
				e.Cancel = true;
			}
			base.OnFormClosing(e);
		}

		private void OnAfterResetInternal(object sender, EventArgs e)
		{
			OnReset(e);
		}

		private void OnRomChangedInternal(object sender, EventArgs e)
		{
			OnRomChanged(e);
		}

		private void OnPausedInternal(object sender, EventArgs e)
		{
			OnPaused(e);
		}

		private void OnBreakInternal(object sender, EventArgs e)
		{
			OnBreak(e);
		}

		private void OnEmulationStatusChangedInternal(object sender, EventArgs e)
		{
			OnEmulationStatusChanged(e);
		}

		private void OnNewFrameInternal(object sender, EventArgs e)
		{
			OnNewFrame(e);
		}

		protected virtual void OnReset(EventArgs e)
		{
		}

		protected virtual void OnRomChanged(EventArgs e)
		{
		}

		protected virtual void OnPaused(EventArgs e)
		{
		}

		protected virtual void OnBreak(EventArgs e)
		{
		}

		protected virtual void OnNewFrame(EventArgs e)
		{
		}

		protected virtual void OnEmulationStatusChanged(EventArgs e)
		{
		}
	}
}
