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
using CrystalBoy.Core;

namespace CrystalBoy.Disassembly
{
	public static partial class Utility
	{
		[CLSCompliant(false)]
		public static ushort GetPreviousInstructionOffset(IMemory memory, ushort offset)
		{
			return 0;
		}

		[CLSCompliant(false)]
		public static string Disassemble(IMemory memory, ushort offset, DisassembleFlags flags, out ushort length)
		{
			OpcodeInfo opcodeInfo;
			byte opcode, extraByte1, extraByte2;
			ushort data;
			string offsetText, rawDataText, opcodeText;
			string formatString;

			if (flags == DisassembleFlags.Instruction)
				formatString = "{2}";
			else if (flags == DisassembleFlags.Offset)
				formatString = "{0}  {2}";
			else if (flags == DisassembleFlags.Instruction)
				formatString = "{1,-8}  {2}";
			else
				formatString = "{0}  {1,-8}  {2}";

			offsetText = offset.ToString("X4");

			opcode = memory.ReadByte(offset++);

			rawDataText = opcode.ToString("X2");

			if (opcode == 0xCB)
			{
				opcode = memory.ReadByte(offset++);
				rawDataText += " " + opcode.ToString("X2");

				opcodeInfo = extendedOpcodeInfoArray[opcode];
				length = 2;
			}
			else
			{
				opcodeInfo = opcodeInfoArray[opcode];
				length = (ushort)(1 + opcodeInfo.ExtraByteCount);
			}

			if (opcodeInfo.ExtraByteCount == 1)
			{
				extraByte1 = memory.ReadByte(offset);

				rawDataText += " " + extraByte1.ToString("X2");

				data = extraByte1;
			}
			else if (opcodeInfo.ExtraByteCount == 2)
			{
				extraByte1 = memory.ReadByte(offset);
				extraByte2 = memory.ReadByte((ushort)(offset + 1));

				rawDataText += " " + extraByte1.ToString("X2") + " " + extraByte2.ToString("X2");

				data = (ushort)(extraByte1 | (extraByte2 << 8));
			}
			else
				data = 0;

			if (opcodeInfo.FormatString == null || opcodeInfo.FormatString.Length == 0)
				opcodeText = string.Format("DB ${0:X2}", opcode);
			else
				opcodeText = string.Format(opcodeInfo.FormatString, data);

			return string.Format(formatString, offsetText, rawDataText, opcodeText);
		}

		public static OpcodeInfo GetOpcodeInfo(byte opcode)
		{
			return opcodeInfoArray[opcode];
		}

		public static OpcodeInfo GetExtendedOpcodeInfo(byte opcode)
		{
			return extendedOpcodeInfoArray[opcode];
		}
	}
}
