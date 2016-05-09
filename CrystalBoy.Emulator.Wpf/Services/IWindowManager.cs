namespace CrystalBoy.Emulator.Services
{
	internal interface IWindowManager
	{
		IWindow CreateWindow(string viewName, object viewModel);
	}
}
