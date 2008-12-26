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
	sealed class LoadInstruction : BinaryInstruction
	{
		LoadOperationType operation;

		public LoadInstruction(int offset, Operand destination, Operand source)
			: this(offset, LoadOperationType.Normal, destination, source)
		{
		}

		public LoadInstruction(int offset, LoadOperationType operation, Operand destination, Operand source)
			: base(offset, destination, source)
		{
			this.operation = operation;
		}

		public LoadOperationType Operation
		{
			get
			{
				return operation;
			}
		}
	}
}
