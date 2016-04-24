using CrystalBoy.Emulation.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrystalBoy.Core;

namespace CrystalBoy.Emulator.Joypads
{
	public sealed class WindowsFormsKeyboardJoypad : ControlFocusedJoypad
	{
		public override GameBoyKeys DownKeys => _keys;

		private volatile GameBoyKeys _keys;
		private Control TopLevelControl { get; }
		private Control[] ControlChain { get; }

		public WindowsFormsKeyboardJoypad(Control renderControl, int joypadIndex)
			: base(renderControl, joypadIndex)
		{
			TopLevelControl = RenderControl.TopLevelControl ?? RenderControl;

			var controlChain = new List<Control>();
			Control control = RenderControl;
			do
			{
				controlChain.Add(control);
				control.PreviewKeyDown += OnPreviewKeyDown;
			}
			while ((control = control.Parent) != null);
			ControlChain = controlChain.ToArray();
			
			TopLevelControl.KeyDown += OnKeyDown;
			TopLevelControl.KeyUp += OnKeyUp;
		}

		private void OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			var key = e.KeyCode & ~Keys.Modifiers;

			if (key == Keys.Left || key == Keys.Up || key == Keys.Right || key == Keys.Down)
			{
				e.IsInputKey = true;
			}
		}

		public override void Dispose()
		{
			foreach (var control in ControlChain) control.PreviewKeyDown -= OnPreviewKeyDown;
			
			TopLevelControl.KeyDown -= OnKeyDown;
			TopLevelControl.KeyUp -= OnKeyUp;

			base.Dispose();
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine(sender);
			var keys = _keys;

			HandleKeyDown(ref keys, e, Keys.Right, GameBoyKeys.Right);
			HandleKeyDown(ref keys, e, Keys.Left, GameBoyKeys.Left);
			HandleKeyDown(ref keys, e, Keys.Up, GameBoyKeys.Up);
			HandleKeyDown(ref keys, e, Keys.Down, GameBoyKeys.Down);
			HandleKeyDown(ref keys, e, Keys.X, GameBoyKeys.A);
			HandleKeyDown(ref keys, e, Keys.Z, GameBoyKeys.B);
			HandleKeyDown(ref keys, e, Keys.RShiftKey, GameBoyKeys.Select);
			HandleKeyDown(ref keys, e, Keys.Return, GameBoyKeys.Start);

			_keys = keys;
		}

		private void OnKeyUp(object sender, KeyEventArgs e)
		{
			var keys = _keys;

			HandleKeyUp(ref keys, e, Keys.Right, GameBoyKeys.Right);
			HandleKeyUp(ref keys, e, Keys.Left, GameBoyKeys.Left);
			HandleKeyUp(ref keys, e, Keys.Up, GameBoyKeys.Up);
			HandleKeyUp(ref keys, e, Keys.Down, GameBoyKeys.Down);
			HandleKeyUp(ref keys, e, Keys.X, GameBoyKeys.A);
			HandleKeyUp(ref keys, e, Keys.Z, GameBoyKeys.B);
			HandleKeyUp(ref keys, e, Keys.RShiftKey, GameBoyKeys.Select);
			HandleKeyUp(ref keys, e, Keys.Return, GameBoyKeys.Start);

			_keys = keys;
		}

		private static void HandleKeyDown(ref GameBoyKeys keys, KeyEventArgs e, Keys key, GameBoyKeys matchingGameBoyKey)
		{
			if ((e.KeyData & ~Keys.Modifiers) == key)
			{
				keys |= matchingGameBoyKey;

				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private static void HandleKeyUp(ref GameBoyKeys keys, KeyEventArgs e, Keys key, GameBoyKeys matchingGameBoyKey)
		{
			if ((e.KeyData & ~Keys.Modifiers) == key)
			{
				keys &= ~matchingGameBoyKey;

				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}
	}
}
