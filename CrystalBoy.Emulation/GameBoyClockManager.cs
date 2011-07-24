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
using System.Diagnostics;
using System.Threading;

namespace CrystalBoy.Emulation
{
	/// <summary>Represent a basic clock manager for Game Boy emulation.</summary>
	/// <remarks>This implementation of <see cref="IClockManager"/> will synchronize emualtion to 60 FPS.</remarks>
	public sealed class GameBoyClockManager : IClockManager
	{
		private const double FrameDuration = 1000d / 60d;

		Stopwatch stopwatch = new Stopwatch();

		/// <summary>Resets this instance.</summary>
		/// <remarks>This method will be called every time the timing has to be restarted.</remarks>
		public void Reset()
		{
			stopwatch.Reset();
			stopwatch.Start();
		}

		/// <summary>Wait before the next event.</summary>
		/// <remarks>This method uses an hybrid wait, relying on <see cref="Thread.Sleep"/> as much as possible.</remarks>
		public void Wait()
		{
			long ellapsedMilliseconds = stopwatch.ElapsedMilliseconds;

			if (ellapsedMilliseconds < 17) // Exact timing for one frame at 60fps is 16⅔ ms
			{
				if (ellapsedMilliseconds < 16)
				{
					// Conversion from long to int is safe since the value is less than 17.
					// Sleep is a really bad tool for precise timing, but it will play its role when needed.
					Thread.Sleep(16 - (int)ellapsedMilliseconds);
				}

				// Do some active wait, even though this is bad…
				while (stopwatch.Elapsed.TotalMilliseconds < FrameDuration) ;
			}

			Reset();
		}
	}
}
