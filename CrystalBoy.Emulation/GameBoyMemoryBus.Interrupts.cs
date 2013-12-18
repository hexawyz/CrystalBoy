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
using CrystalBoy.Core;
using System.Threading;

namespace CrystalBoy.Emulation
{
	partial class GameBoyMemoryBus
	{
		#region Variables

		private volatile int requestedInterrupts;

		#endregion

		#region Interrupt Management

		public int WaitForInterrupts()
		{
			int vbi = int.MaxValue;
			int stat = int.MaxValue;
			int timer;
			int temp;

			if (lcdEnabled && (EnabledInterrupts & 0x3) != 0)
			{
				temp = (144 - lcdRealLine) * HorizontalLineDuration - rasterCycles;
				if (lcdRealLine > 144) temp += FrameDuration;

				if ((EnabledInterrupts & 0x1) != 0) vbi = temp;

				if ((EnabledInterrupts & 0x2) != 0)
				{
					if (notifyMode1) stat = temp;
					if (notifyCoincidence)
					{
						unsafe { temp = portMemory[0x45]; }

						if (temp < 154)
						{
							if (lyRegister < temp) // Case where LYC is 1-152 and greater than LY
								stat = Math.Min(stat, (temp - lcdRealLine) * HorizontalLineDuration - 4 - rasterCycles); // We use the real raster line (not advanced by 4 cycles ;)) for computations here
							else if (temp == 0) // Case where LYC is 0… (Line 153 ends early, so this is a special case)
								stat = Math.Min(stat, (153 - lcdRealLine) * HorizontalLineDuration + 8 - rasterCycles + (lcdRealLine == 153 && rasterCycles > 8 ? FrameDuration : 0)); // The coincidence for LY = 0 should happen at the beginning of line 153
							else if (lyRegister > temp) // Case where LYC is 1-152 and lesser or equal to LY
								stat = Math.Min(stat, FrameDuration - (lcdRealLine - temp) * HorizontalLineDuration - 4 - rasterCycles);
							else // Case where LY == LYC
								stat = Math.Min(stat, FrameDuration - rasterCycles - 4);
						}
					}
					if (notifyMode0) stat = Math.Min(stat, (rasterCycles > Mode2Duration + Mode3Duration ? HorizontalLineDuration : lcdRealLine < 143 ? 0 : (154 - lcdRealLine) * HorizontalLineDuration) + Mode2Duration + Mode3Duration - rasterCycles);
					if (notifyMode2) stat = Math.Min(stat, lcdRealLine < 143 ? HorizontalLineDuration - rasterCycles : (154 - lcdRealLine) * HorizontalLineDuration - rasterCycles);
				}
			}

			timer = timerEnabled && (EnabledInterrupts & 0x4) != 0 ? timerOverflowCycles - timerCycles : int.MaxValue;

			temp = Math.Min(vbi, Math.Min(stat, timer));

			return temp == int.MaxValue ? -1 : temp;
		}

		public void InterruptRequest(Interrupt interrupt)
		{
			InterruptRequest((byte)interrupt);
		}

		public void InterruptRequest(byte interrupt)
		{
			while (true)
			{
				var requestedInterrupts = this.requestedInterrupts;
#pragma warning disable 0420
				if (Interlocked.CompareExchange(ref this.requestedInterrupts, requestedInterrupts | interrupt, requestedInterrupts) == requestedInterrupts)
					return;
#pragma warning restore 0420
			}
		}

		public void InterruptHandled(Interrupt interrupt) { InterruptHandled((byte)interrupt); }

		public void InterruptHandled(byte interrupt)
		{
			while (true)
			{
				var requestedInterrupts = this.requestedInterrupts;

#pragma warning disable 0420
				if (Interlocked.CompareExchange(ref this.requestedInterrupts, requestedInterrupts & ~interrupt, requestedInterrupts) == requestedInterrupts)
					return;
#pragma warning restore 0420
			}
		}

		public byte RequestedInterrupts
		{
			get { return (byte)requestedInterrupts; }
			set { requestedInterrupts = value & 0x1F; }
		}

		public unsafe byte EnabledInterrupts { get { return portMemory[0xFF]; } }

		#endregion
	}
}
