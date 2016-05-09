using System.Windows.Input;

namespace CrystalBoy.Emulator
{
	internal static class EmulatorCommands
	{
		private static readonly RoutedUICommand _showRomInformationCommand = new RoutedUICommand("Rom information…", "ShowRomInformation", typeof(EmulatorCommands));
		private static readonly RoutedUICommand _pauseCommand = new RoutedUICommand("Pause", "Pause", typeof(EmulatorCommands), new InputGestureCollection(new[] { new KeyGesture(Key.P, ModifierKeys.Control) }));
		private static readonly RoutedUICommand _exitCommand = new RoutedUICommand("Exit", "Exit", typeof(EmulatorCommands), new InputGestureCollection(new[] { new KeyGesture(Key.X, ModifierKeys.Control) }));
		private static readonly RoutedUICommand _aboutCommand = new RoutedUICommand("About…", "About", typeof(EmulatorCommands));

		public static RoutedUICommand ShowRomInformation => _showRomInformationCommand;
		public static RoutedUICommand Pause => _pauseCommand;
		public static RoutedUICommand Exit => _exitCommand;
		public static RoutedUICommand About => _aboutCommand;
	}
}
