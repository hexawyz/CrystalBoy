using System;
using System.Windows.Input;
using CrystalBoy.VisualAssembler.Properties;

namespace CrystalBoy.VisualAssembler
{
	internal static class Commands
	{
		private static RoutedUICommand exit = new RoutedUICommand(Resources.ExitCommandText, "Exit", typeof(ICommand), new InputGestureCollection(new[] { new KeyGesture(Resources.ExitCommandKey, Resources.ExitCommandModifierKeys) }));

		public static RoutedUICommand Exit { get { return exit; } }
	}
}
