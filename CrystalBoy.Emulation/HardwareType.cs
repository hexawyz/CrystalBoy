#region Copyright Notice
// This file is part of CrystalBoy.
// Copyright © 2008-2011 Fabien Barbier
// 
// CrystalBoy is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// CrystalBoy is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalBoy.Emulation
{
	/// <summary>Represent an emulated hardware type.</summary>
	public enum HardwareType : byte
	{
		/// <summary>Original Game Boy</summary>
		GameBoy = 0,
		/// <summary>Pocket Game Boy</summary>
		GameBoyPocket = 1,
		/// <summary>Super Game Boy</summary>
		SuperGameBoy = 2,
		/// <summary>Super Game Boy 2</summary>
		SuperGameBoy2 = 3,
		/// <summary>Game Boy Color</summary>
		GameBoyColor = 4,
		/// <summary>Super Game Boy + Game Boy Color</summary>
		/// <remarks>This is used for combining Game Boy Color emulation with Super Game Boy emulation, and doesn't represent real hardware.</remarks>
		SuperGameBoyColor = 5,
		/// <summary>Game Boy Advance</summary>
		/// <remarks>This is mostly like a Game Boy Color when GB(C) games are concerned.</remarks>
		GameBoyAdvance = 6,
		/// <summary>Super Game Boy + Game Boy Advance</summary>
		/// <remarks>This is used for combining Game Boy Advance emulation with Super Game Boy emulation, and doesn't represent real hardware.</remarks>
		SuperGameBoyAdvance = 7
	}
}
