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

namespace CrystalBoy.Decompiler.Analyzer
{
	class JumpInstruction : FlowControlInstruction
	{
		ushort destination;
		JumpType jumpType;

		public JumpInstruction(int offset, JumpType jumpType, ushort destination)
			: this(offset, jumpType, Condition.Always, destination)
		{
		}

		public JumpInstruction(int offset, JumpType jumpType, Condition condition, ushort destination)
			: base(offset, condition)
		{
			this.jumpType = jumpType;
			this.destination = destination;

			if (!(JumpsToRom || JumpsToHighRam))
				throw new NotSupportedException("Jumps outside of ROM or HRAM are unsupported yet.");
		}

		public JumpType JumpType
		{
			get
			{
				return jumpType;
			}
		}

		public ushort Destination
		{
			get
			{
				return destination;
			}
		}

		public bool JumpsToRom
		{
			get
			{
				return destination < 0x8000;
			}
		}

		public bool JumpsToHighRam
		{
			get
			{
				return destination >= 0xFF80 && destination < 0xFFFF;
			}
		}

		public virtual bool IsResolved
		{
			get
			{
				return false;
			}
		}
	}
}
