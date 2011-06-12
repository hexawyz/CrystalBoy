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
	public sealed class RealTimeClockState
	{
		// The field ordering here should allow optimal packing
		// (Even though the CLR could supposedly reorder them on its own… But I'm not sure this really happens)
		private DateTime dateTime = DateTime.UtcNow;
		private short days;
		private short latchedDays;
		private byte seconds, minutes, hours;
		private byte latchedSeconds, latchedMinutes, latchedHours;
		private bool frozen;

		public DateTime DateTime
		{
			get { return dateTime; }
			set { dateTime = value; }
		}

		public short Days
		{
			get { return days; }
			set
			{
				Adjust();
				days = value;
			}
		}

		public byte Hours
		{
			get { return hours; }
			set
			{
				Adjust();
				hours = value;
			}
		}

		public byte Minutes
		{
			get { return minutes; }
			set
			{
				Adjust();
				minutes = value;
			}
		}

		public byte Seconds
		{
			get { return seconds; }
			set
			{
				Adjust();
				seconds = value;
			}
		}

		public short LatchedDays
		{
			get { return latchedDays; }
			set { latchedDays = value; }
		}

		public byte LatchedHours
		{
			get { return latchedHours; }
			set { latchedHours = value; }
		}

		public byte LatchedMinutes
		{
			get { return latchedMinutes; }
			set { latchedMinutes = value; }
		}

		public byte LatchedSeconds
		{
			get { return latchedSeconds; }
			set { latchedSeconds = value; }
		}

		public bool Frozen
		{
			get { return frozen; }
			set { frozen = value; }
		}

		private void Adjust()
		{
			if (!frozen)
			{
				var timeSpan = DateTime.UtcNow - dateTime;

				seconds = (byte)(seconds + timeSpan.Seconds);
				minutes = (byte)(minutes + timeSpan.Minutes);
				hours = (byte)(hours + timeSpan.Hours);
				days = (short)(days + timeSpan.Days);

				// Adjusts the values if needed
				// (An overflow might have happened when we added the time span values…)
				if (seconds > 60)
				{
					seconds -= 60;
					minutes++;
				}
				if (minutes > 60)
				{
					minutes -= 60;
					hours++;
				}
				if (hours > 24)
				{
					hours -= 24;
					days++;
				}
			}
			dateTime = DateTime.UtcNow;
		}

		public void Latch()
		{
			if (!frozen)
			{
				TimeSpan timeSpan = DateTime.UtcNow - dateTime;

				latchedSeconds = (byte)(seconds + timeSpan.Seconds);
				latchedMinutes = (byte)(minutes + timeSpan.Minutes);
				latchedHours = (byte)(hours + timeSpan.Hours);
				latchedDays = (short)(days + timeSpan.Days);

				// Adjusts the latched values if needed
				// (An overflow might have happened when we added the time span values…)
				if (latchedSeconds > 60)
				{
					latchedSeconds -= 60;
					latchedMinutes++;
				}
				if (latchedMinutes > 60)
				{
					latchedMinutes -= 60;
					latchedHours++;
				}
				if (latchedHours > 24)
				{
					latchedHours -= 24;
					days++;
				}
			}
			else
			{
				latchedSeconds = seconds;
				latchedMinutes = minutes;
				latchedHours = hours;
				latchedDays = days;
			}
		}
	}
}
