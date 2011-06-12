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

namespace CrystalBoy.Emulation
{
#if WITH_DEBUGGING
	partial class GameBoyMemoryBus : IDebuggable
	{
		#region Variables

		List<int> breakPointList;

		#endregion

		#region Initialize

		partial void InitializeDebug()
		{
			breakPointList = new List<int>();
		}

		#endregion

		#region Breakpoint Management

		public event EventHandler BreakpointUpdate;

		public int BreakpointCount
		{
			get
			{
				return breakPointList.Count;
			}
		}

		[CLSCompliant(false)]
		public bool IsBreakPoint(ushort offset)
		{
			int fullOffset = GetFullOffset(offset);

			for (int i = 0; i < breakPointList.Count; i++)
			{
				int breakPointOffset = breakPointList[i]; // Get the breakpoint offset

				if (breakPointOffset == fullOffset) // Compare the two offsets
					return true;
				else if ((breakPointOffset & 0xFFFF) > offset) // The list is sorted for efficiency...
					return false;
			}

			return false;
		}

		[CLSCompliant(false)]
		public void AddBreakpoint(ushort offset)
		{
			int fullOffset = GetFullOffset(offset);
			int i;

			for (i = 0; i < breakPointList.Count; i++)
			{
				int breakPointOffset = breakPointList[i]; // Get the breakpoint offset

				if (breakPointOffset == fullOffset) // Compare the two offsets
					return;
				else if ((breakPointOffset & 0xFFFF) > offset) // The list is sorted for efficiency...
					break;
			}

			breakPointList.Insert(i, fullOffset);
			OnBreakpointUpdate(EventArgs.Empty);
		}

		[CLSCompliant(false)]
		public void ToggleBreakpoint(ushort offset)
		{
			int fullOffset = GetFullOffset(offset);
			int i;

			for (i = 0; i < breakPointList.Count; i++)
			{
				int breakPointOffset = breakPointList[i]; // Get the breakpoint offset

				if (breakPointOffset == fullOffset) // Compare the two offsets
				{
					breakPointList.RemoveAt(i);
					OnBreakpointUpdate(EventArgs.Empty);
					return;
				}
				else if ((breakPointOffset & 0xFFFF) > offset) // The list is sorted for efficiency...
					break;
			}

			breakPointList.Insert(i, fullOffset);
			OnBreakpointUpdate(EventArgs.Empty);
		}

		public void ClearBreakpoints()
		{
			breakPointList.Clear();
			OnBreakpointUpdate(EventArgs.Empty);
		}

		private void OnBreakpointUpdate(EventArgs e)
		{
			if (BreakpointUpdate != null)
				BreakpointUpdate(this, e);
		}

		private int GetFullOffset(ushort offset)
		{
			switch (offset >> 12)
			{
				case 0x0:
				case 0x1:
				case 0x2:
				case 0x3:
					return offset | (lowerRomBank << 16);
				case 0x4:
				case 0x5:
				case 0x6:
				case 0x7:
					return offset | (upperRombank << 16);
				case 0x8:
				case 0x9:
					return offset | (videoRamBank << 16);
				case 0xA:
				case 0xB:
					return offset | (ramBank << 16);
				case 0xC:
					return offset;
				case 0xD:
					return offset | (workRamBank << 16);
				case 0xE:
					return offset;
				case 0xF:
					if (offset < 0xFE00)
						return offset | (workRamBank << 16);
					else
						return offset;
				default:
					return offset;
			}
		}

		#endregion
	}
#endif
}
