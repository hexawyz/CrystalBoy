namespace CrystalBoy.Emulator.Services
{
	internal interface IFileDialogService
	{
		string ShowOpenFileDialog(string filter);
		string ShowSaveFileDialog(string filter);
	}
}
