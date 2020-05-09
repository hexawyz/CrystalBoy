using System;
using System.Collections.Generic;
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
#if WITH_DEBUGGING
    partial class GameBoyMemoryBus : IDebuggable
	{
		#region Variables

		private List<int> breakPointList;
#if DEBUG_CYCLE_COUNTER
		private int debugCycleCount;
#endif

		#endregion

		#region Events

		public event EventHandler Breakpoint;

		private NotificationHandler breakpointHandler;

		#endregion

		#region Initialize

		partial void InitializeDebug()
		{
			breakpointHandler = null;
			breakPointList = new List<int>();
		}

		#endregion

		#region Reset

#if DEBUG_CYCLE_COUNTER || DEBUG
		partial void ResetDebug()
		{
#if DEBUG_CYCLE_COUNTER
			debugCycleCount = 0;
#endif
#if DEBUG && DEBUG_OAM
			segmentWriteHandlerArray[0xFE] += DebugHandleOamWrite;
#endif
		}
#endif

		#endregion

		#region Breakpoint Management

		public event EventHandler BreakpointUpdate;

		public int BreakpointCount { get { return breakPointList.Count; } }

		private void OnBreakpoint(EventArgs e) { if (Breakpoint != null) Breakpoint(this, e); }

		public bool IsBreakpoint(ushort offset)
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

		#region Debug Cycle Counter

#if DEBUG_CYCLE_COUNTER
		public int DebugCycleCount { get { return debugCycleCount; } }

		public void ResetDebugCycleCounter() { debugCycleCount = 0; }
#endif

		#endregion

		#region Memory Write Debugger

#if DEBUG && DEBUG_OAM
		private unsafe void DebugHandleOamWrite(byte offsetLow, byte offsetHigh, byte value)
		{
			//Debug.WriteLine("Memory Write [0x" + ((offsetHigh << 8) | offsetLow).ToString("X4") + "] = 0x" + value.ToString("X2"));
			objectAttributeMemory[offsetLow] = value;

		}
#endif

		#endregion
	}
#endif
}
