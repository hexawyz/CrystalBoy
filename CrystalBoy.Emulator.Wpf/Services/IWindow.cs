using System;

namespace CrystalBoy.Emulator.Services
{
	interface IWindow : IDisposable
	{
		void Show();
		void Hide();
	}
}
