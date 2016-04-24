using System;

namespace CrystalBoy.Core
{
	/// <summary>Game Boy Keys.</summary>
	[Flags]
	public enum GameBoyKeys : byte
	{
		/// <summary>No keys.</summary>
		None = 0x00,
		/// <summary>The rigt arrow key.</summary>
		Right = 0x01,
		/// <summary>The left arrow key.</summary>
		Left = 0x02,
		/// <summary>The up arrow key.</summary>
		Up = 0x04,
		/// <summary>The down arrow key.</summary>
		Down = 0x08,
		/// <summary>The A key.</summary>
		A = 0x10,
		/// <summary>The B key.</summary>
		B = 0x20,
		/// <summary>The Select key.</summary>
		Select = 0x40,
		/// <summary>The Start key.</summary>
		Start = 0x80,
		/// <summary>All keys.</summary>
		All = 0xFF
	}
}
