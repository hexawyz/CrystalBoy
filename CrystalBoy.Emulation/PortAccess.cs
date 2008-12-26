#region Copyright Notice
// This file is part of CrystalBoy.
// Copyright (C) 2008 Fabien Barbier
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
	public struct PortAccess
	{
		public PortAccess(int clock, byte port, byte value)
		{
			this.Clock = clock;
			this.Port = (Port)port;
			this.Value = value;
		}

		public PortAccess(int clock, Port port, byte value)
		{
			this.Clock = clock;
			this.Port = port;
			this.Value = value;
		}

		/// <summary>
		/// Time in Clock Cycles (Since the last VBlank)
		/// </summary>
		public int Clock;
		/// <summary>
		/// Port index
		/// </summary>
		public Port Port;
		/// <summary>
		/// Value written to the register
		/// </summary>
		public byte Value;

		public override string ToString()
		{
			if (Enum.IsDefined(typeof(Port), Port))
				return "{PortAccess: Clock " + Clock + ", Port " + Port.ToString() + ", Value 0x" + Value.ToString("X2") + "}";
			else
				return "{PortAccess: Clock " + Clock + ", Port 0x" + ((byte)Port).ToString("X2") + ", Value 0x" + Value.ToString("X2") + "}";
		}
	}
}
