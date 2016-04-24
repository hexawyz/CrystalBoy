namespace CrystalBoy.Core
{
	/// <summary>A gameboy joypad.</summary>
	public interface IJoypad
	{
		/// <summary>Gets the keys which are currently down on the joypad.</summary>
		GameBoyKeys DownKeys { get; }
	}
}
