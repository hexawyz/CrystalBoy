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

namespace CrystalBoy.Emulation
{
	/// <summary>Provides a way of delaying events.</summary>
	/// <remarks>This interface is used in <see cref="GameBoyMemoryBus"/> to provide timing and limit the framerate.</remarks>
	public interface IClockManager
	{
		/// <summary>Resets this instance.</summary>
		/// <remarks>This method will be called every time the timing has to be restarted.</remarks>
		void Reset();
		/// <summary>Wait before the next event.</summary>
		/// <remarks>
		/// This method will be called every time an event needs to be delayed.
		/// The wait may be active, passive or hybrid, as desired.
		/// There may even be no delay at all.
		/// </remarks>
		void Wait();
	}
}
