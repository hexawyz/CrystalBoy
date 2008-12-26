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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace CrystalBoy.Disassembly.OpcodeTableGenerator
{
	static class Program
	{
		static void Main(string[] args)
		{
			OpcodeInfo[] opcodes,
				extendedOpcodes;
			StringBuilder output;
			string template, result;

			template = File.ReadAllText("Utility.Template.cs", Encoding.UTF8);

			opcodes = new OpcodeInfo[256];
			extendedOpcodes = new OpcodeInfo[256];

			for (int i = 0; i <= 255; i++)
			{
				opcodes[i] = Utility.GetOpcodeInfo((byte)i);
				extendedOpcodes[i] = Utility.GetExtendedOpcodeInfo((byte)i);
			}

			output = new StringBuilder(1000);

			output.AppendLine("static readonly OpcodeInfo[] opcodeInfoArray = {");

			for (int i = 0; i <= 255; i++)
			{
				OpcodeInfo opcodeInfo = opcodes[i];

				if (opcodeInfo.FormatString != null && opcodeInfo.FormatString.Length > 0)
					output.AppendFormat(CultureInfo.InvariantCulture, "/* 0x{0:X2} */ new OpcodeInfo(\"{1}\", {2}, Operation.{3}, Operand.{4}, Operand.{5}, {6}, (Flags){7:D}, (Flags){8:D}, (Flags){9:D}, {10}, {11})", i, opcodeInfo.FormatString, opcodeInfo.ExtraByteCount, opcodeInfo.Operation, opcodeInfo.FirstOperand, opcodeInfo.SecondOperand, opcodeInfo.EmbeddedValue, opcodeInfo.AffectedFlags, opcodeInfo.SetFlags, opcodeInfo.ClearFlags, opcodeInfo.BaseCycleCount, opcodeInfo.ConditionalCycleCount);
				else
					output.AppendFormat(CultureInfo.InvariantCulture, "/* 0x{0:X2} */ OpcodeInfo.Invalid", i);
				if (i < 255)
					output.Append(',');
				output.AppendLine();
			}

			output.AppendLine("};");

			output.AppendLine();

			output.AppendLine("static readonly OpcodeInfo[] extendedOpcodeInfoArray = {");

			for (int i = 0; i <= 255; i++)
			{
				OpcodeInfo opcodeInfo = extendedOpcodes[i];

				if (opcodeInfo.FormatString != null && opcodeInfo.FormatString.Length > 0)
					output.AppendFormat(CultureInfo.InvariantCulture, "/* 0x{0:X2} */ new OpcodeInfo(\"{1}\", {2}, Operation.{3}, Operand.{4}, Operand.{5}, {6}, (Flags){7:D}, (Flags){8:D}, (Flags){9:D}, {10}, {11})", i, opcodeInfo.FormatString, opcodeInfo.ExtraByteCount, opcodeInfo.Operation, opcodeInfo.FirstOperand, opcodeInfo.SecondOperand, opcodeInfo.EmbeddedValue, opcodeInfo.AffectedFlags, opcodeInfo.SetFlags, opcodeInfo.ClearFlags, opcodeInfo.BaseCycleCount, opcodeInfo.ConditionalCycleCount);
				else
					output.AppendFormat(CultureInfo.InvariantCulture, "/* 0x{0:X2} */ OpcodeInfo.Invalid", i);
				if (i < 255)
					output.Append(',');
				output.AppendLine();
			}

			output.Append("};");

			result = CrystalBoy.Tools.CSharpIndenter.IndentSource(template.Replace("%OPCODE_TABLES%", output.ToString()));

			Console.WriteLine(result);
		}
	}
}
