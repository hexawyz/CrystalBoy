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

namespace CrystalBoy.Disassembly
{
	public struct OpcodeInfo
	{
		public static OpcodeInfo Invalid = new OpcodeInfo(null, 0, Operation.Invalid, Operand.None, Operand.None, 0, Flags.None, Flags.None, Flags.None, 4, 0);

		public OpcodeInfo(string formatString, int extraByteCount, Operation operation, Operand firstOperand, Operand secondOperand, byte embeddedValue, Flags affectedFlags, Flags setFlags, Flags clearFlags, byte baseCycleCount, byte conditionalCycleCount)
		{
			FormatString = formatString;
			ExtraByteCount = extraByteCount;
			Operation = operation;
			FirstOperand = firstOperand;
			SecondOperand = secondOperand;
			EmbeddedValue = embeddedValue;
			AffectedFlags = affectedFlags;
			SetFlags = setFlags;
			ClearFlags = clearFlags;
			BaseCycleCount = baseCycleCount;
			ConditionalCycleCount = conditionalCycleCount;
		}

		public string FormatString;
		public int ExtraByteCount;
		public Operation Operation;
		public Operand FirstOperand;
		public Operand SecondOperand;
		public Flags AffectedFlags,
			SetFlags,
			ClearFlags;
		public byte EmbeddedValue;
		public byte BaseCycleCount;
		public byte ConditionalCycleCount;
	}
}
