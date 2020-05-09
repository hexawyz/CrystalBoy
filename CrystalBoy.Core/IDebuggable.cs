using System;

namespace CrystalBoy.Core
{
    public interface IDebuggable
	{
		int BreakpointCount { get; }
		bool IsBreakpoint(ushort offset);
		void AddBreakpoint(ushort offset);
		void ToggleBreakpoint(ushort offset);
		void ClearBreakpoints();
		event EventHandler BreakpointUpdate;
	}
}
