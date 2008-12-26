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
using System.Collections.Generic;
using System.Text;

namespace CrystalBoy.Decompiler.Analyzer
{
	sealed class ResolvedJumpInstruction : JumpInstruction
	{
		Label resolvedDestination;

		public ResolvedJumpInstruction(int offset, JumpType jumpType, ushort destination, Label resolvedDestination)
			: this(offset, jumpType, Condition.Always, destination, resolvedDestination)
		{
		}

		public ResolvedJumpInstruction(int offset, JumpType jumpType, Condition condition, ushort destination, Label resolvedDestination)
			: base(offset, jumpType, condition, destination)
		{
			this.resolvedDestination = resolvedDestination;
		}

		public Label ResolvedDestination
		{
			get
			{
				return resolvedDestination;
			}
		}

		public override bool IsResolved
		{
			get
			{
				return true;
			}
		}
	}
}
