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
	/// <remarks>This implementation of <see cref="IClockManager"/> will synchronize emulation to 60 FPS.</remarks>
	public sealed class GameBoyClockManager : IClockManager
	{
		public static readonly long ApproximateFrameTickDuration = 166 * Stopwatch.Frequency / 10000; // The "exact" frame duration in ticks would be 166666, but we can't be that precise…

		Stopwatch stopwatch = new Stopwatch();

		/// <summary>Resets this instance.</summary>
		/// <remarks>This method will be called every time the timing has to be restarted.</remarks>
		public void Reset()
		{
			stopwatch.Restart();
		}

		/// <summary>Wait before the next event.</summary>
		/// <remarks>This method uses an hybrid wait, relying on <see cref="Thread.Sleep(int)"/> when possible.</remarks>
		public void Wait()
		{
			const long FrameTickDuration = TimeSpan.TicksPerSecond / 60;

			long timer = stopwatch.Elapsed.Ticks;

			if (timer < FrameTickDuration)	// Exact timing for one frame at 60fps is 16⅔ ms
			{
				if (timer < FrameTickDuration - TimeSpan.TicksPerMillisecond)
				{
					Thread.Sleep((int)(FrameTickDuration / TimeSpan.TicksPerMillisecond) - 1);
				}

				// Do some active wait, even though this is bad…
				while (stopwatch.ElapsedTicks < ApproximateFrameTickDuration)
				{
					Thread.SpinWait(1000);
				}
			}

			Reset();
		}
	}
}
