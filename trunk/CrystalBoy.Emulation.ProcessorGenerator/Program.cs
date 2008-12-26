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
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using CrystalBoy.Disassembly;

namespace CrystalBoy.CoreEmulationGenerator
{
	static class Program
	{
		static Dictionary<string, string> templateDictionary;
		static Regex assignationFilterRegex = new Regex(@"^\s*(?<var>\w+)\s*=\s*\k<var>\s*;\s*$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline),
			equalityFilterRegex = new Regex(@"(?<var>\w+)\s*=\s*(?<expr>.+)\s*==\s*\k<expr>", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline),
			lessThanFilterRegex = new Regex(@"(?<var>\w+)\s*=\s*(?<expr>.+)\s*<\s*\k<expr>", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);

		static void Main(string[] args)
		{
			StringBuilder output;
			string result;

			LoadTemplates();

			output = new StringBuilder(100000);

			output.AppendLine("opcode = bus[pc++];");
			output.AppendLine("switch(opcode)");
			output.AppendLine("{");
			for (int i = 0; i < 256; i++)
			{
				OpcodeInfo opcodeInfo;

				if (i == 0xCB)
				{
					output.AppendFormat("case 0xCB: /* Extended opcodes */");
					output.AppendLine("opcode = bus.ReadByte(pc++);");
					output.AppendLine("switch(opcode)");
					output.AppendLine("{");
					for (int j = 0; j < 256; j++)
					{
						opcodeInfo = Utility.GetExtendedOpcodeInfo((byte)j);
						output.AppendFormat("case 0x{0:X2}: /* {1} */", j, GetOpcodeString(opcodeInfo));
						output.AppendLine();
						GenerateOpcodeStub(opcodeInfo, output);
						output.AppendLine("break;");
					}
					output.AppendLine("}");
					output.AppendLine("break;");
				}
				else
				{
					opcodeInfo = Utility.GetOpcodeInfo((byte)i);

					if (opcodeInfo.Operation != Operation.Invalid)
					{
						output.AppendFormat("case 0x{0:X2}: /* {1} */", i, GetOpcodeString(opcodeInfo));
						output.AppendLine();
						GenerateOpcodeStub(opcodeInfo, output);
						output.AppendLine("break;");
					}
				}
			}
			output.AppendLine("default:");
			output.AppendLine("throw new InvalidOperationException(\"Invalid opcode: \" + opcode.ToString(\"X2\"));");
			output.Append("}");

			result = templateDictionary["Processor"].Replace("%FETCH%", output.ToString());
			result = assignationFilterRegex.Replace(result, "");
			result = equalityFilterRegex.Replace(result, "${var} = true");
			result = lessThanFilterRegex.Replace(result, "${var} = false");

			result = CrystalBoy.Tools.CSharpIndenter.IndentSource(result);

			Console.Write(result);
		}

		#region Template Functions

		static void LoadTemplates()
		{
			templateDictionary = new Dictionary<string, string>();

			foreach (string fileName in Directory.GetFiles("Templates", "*.cs", SearchOption.TopDirectoryOnly))
			{
				string fileTitle = Path.GetFileNameWithoutExtension(fileName);
				string contents = File.ReadAllText(fileName, Encoding.UTF8);

				templateDictionary.Add(fileTitle, contents);
			}
		}

		static void CallTemplate(string templateName, string op1, string op2, StringBuilder output)
		{
			string template;

			if (templateDictionary.TryGetValue(templateName, out template))
			{
				output.Append(template.Replace("%OP1%", op1).Replace("%OP2%", op2));
			}
		}

		#endregion

		#region General utility Functions

		static string GetOpcodeString(OpcodeInfo opcodeInfo)
		{
			return opcodeInfo.FormatString.Replace("${0:X2}", "N").Replace("${0:X4}", "N").Replace("{0:X2}", "00+N");
		}

		#endregion

		#region Stub Generation Functions

		#region Main Function

		static void GenerateOpcodeStub(OpcodeInfo opcodeInfo, StringBuilder output)
		{
			string operand1, operand2;

			if (opcodeInfo.Operation == Operation.Ld && opcodeInfo.SecondOperand == Operand.Sp)
			{
				CallTemplate("LdSp", null, null, output);
				return;
			}
			else
			{
				// Only load the first operand for arithmetic and logical operations (Also for PUSH)
				if (opcodeInfo.Operation == Operation.Inc ||
					opcodeInfo.Operation == Operation.Dec ||
					opcodeInfo.Operation == Operation.Add ||
					opcodeInfo.Operation == Operation.Adc ||
					opcodeInfo.Operation == Operation.Sub ||
					opcodeInfo.Operation == Operation.Sbc ||
					opcodeInfo.Operation == Operation.And ||
					opcodeInfo.Operation == Operation.Xor ||
					opcodeInfo.Operation == Operation.Or ||
					opcodeInfo.Operation == Operation.Cp ||
					opcodeInfo.Operation == Operation.Rlc ||
					opcodeInfo.Operation == Operation.Rrc ||
					opcodeInfo.Operation == Operation.Rl ||
					opcodeInfo.Operation == Operation.Rr ||
					opcodeInfo.Operation == Operation.Sla ||
					opcodeInfo.Operation == Operation.Sra ||
					opcodeInfo.Operation == Operation.Swap ||
					opcodeInfo.Operation == Operation.Srl ||
					opcodeInfo.Operation == Operation.Push ||
					opcodeInfo.Operation == Operation.Jr ||
					opcodeInfo.Operation == Operation.Jp ||
					opcodeInfo.Operation == Operation.Call)
				{
					operand1 = GetOperandLoadStub(opcodeInfo.FirstOperand);
					if (operand1 != null && operand1.Length > 0)
						output.AppendLine(operand1);
				}
				operand2 = GetOperandLoadStub(opcodeInfo.SecondOperand);

				if (operand2 != null && operand2.Length > 0)
					output.AppendLine(operand2);

				operand1 = GetOperandString(opcodeInfo.FirstOperand, opcodeInfo.EmbeddedValue);
				operand2 = GetOperandString(opcodeInfo.SecondOperand, opcodeInfo.EmbeddedValue);

				if (opcodeInfo.Operation == Operation.Add)
				{
					if (opcodeInfo.FirstOperand == Operand.Hl || opcodeInfo.FirstOperand == Operand.Sp)
						CallTemplate("Add16", operand1, operand2, output);
					else
						CallTemplate("Add8", operand1, operand2, output);
				}
				else if (opcodeInfo.Operation == Operation.Inc)
				{
					if (opcodeInfo.FirstOperand == Operand.Bc ||
						opcodeInfo.FirstOperand == Operand.De ||
						opcodeInfo.FirstOperand == Operand.Hl ||
						opcodeInfo.FirstOperand == Operand.Sp)
						CallTemplate("Inc16", operand1, operand2, output);
					else
						CallTemplate("Inc8", operand1, operand2, output);
				}
				else if (opcodeInfo.Operation == Operation.Dec)
				{
					if (opcodeInfo.FirstOperand == Operand.Bc ||
						opcodeInfo.FirstOperand == Operand.De ||
						opcodeInfo.FirstOperand == Operand.Hl ||
						opcodeInfo.FirstOperand == Operand.Sp)
						CallTemplate("Dec16", operand1, operand2, output);
					else
						CallTemplate("Dec8", operand1, operand2, output);
				}
				else if ((opcodeInfo.Operation == Operation.Jr ||
					opcodeInfo.Operation == Operation.Jp ||
					opcodeInfo.Operation == Operation.Call) &&
					opcodeInfo.SecondOperand != Operand.None ||
					opcodeInfo.Operation == Operation.Ret &&
					opcodeInfo.FirstOperand != Operand.None)
				{
					output.AppendLine("if (" + operand1 + ")");
					output.AppendLine("{");
					CallTemplate(opcodeInfo.Operation.ToString(), operand2, "", output);
					output.AppendLine("cycleCount = " + (opcodeInfo.BaseCycleCount + opcodeInfo.ConditionalCycleCount).ToString() + ";");
					output.AppendLine("}");
					output.AppendLine("else");
					output.AppendLine("cycleCount = " + opcodeInfo.BaseCycleCount.ToString() + ";");
					return;
				}
				else if (opcodeInfo.Operation == Operation.Ldi || opcodeInfo.Operation == Operation.Ldd)
					CallTemplate("Ld", operand1, operand2, output);
				else
					CallTemplate(opcodeInfo.Operation.ToString(), operand1, operand2, output);

				if (opcodeInfo.Operation == Operation.Set
					|| opcodeInfo.Operation == Operation.Res)
				{
					operand2 = GetOperandStoreStub(opcodeInfo.SecondOperand);
					if (operand2 != null && operand2.Length > 0)
						output.AppendLine(operand2);
				}
				else if (opcodeInfo.Operation != Operation.Push)
				{
					operand1 = GetOperandStoreStub(opcodeInfo.FirstOperand);
					if (operand1 != null && operand1.Length > 0)
						output.AppendLine(operand1);
				}

				if (opcodeInfo.Operation == Operation.Ldi)
					output.AppendLine("if (++l == 0) h++;");
				else if (opcodeInfo.Operation == Operation.Ldd)
					output.AppendLine ("if (l-- == 0) h--;");
			}

			GenerateFixedFlags(opcodeInfo.AffectedFlags & opcodeInfo.SetFlags, true, output);
			GenerateFixedFlags(opcodeInfo.AffectedFlags & opcodeInfo.ClearFlags, false, output);
			if (opcodeInfo.Operation != Operation.Halt)
				output.AppendLine("cycleCount = " + opcodeInfo.BaseCycleCount.ToString() + ";");
		}

		#endregion

		#region Flags utility Function

		static void GenerateFixedFlags(Flags flags, bool isSet, StringBuilder output)
		{
			string flagValue;

			if (isSet)
				flagValue = " = true;";
			else
				flagValue = " = false;";

			if ((flags & Flags.Zero) != 0)
			{
				output.Append("zeroFlag");
				output.AppendLine(flagValue);
			}
			if ((flags & Flags.Negation) != 0)
			{
				output.Append("negationFlag");
				output.AppendLine(flagValue);
			}
			if ((flags & Flags.HalfCarry) != 0)
			{
				output.Append("halfCarryFlag");
				output.AppendLine(flagValue);
			}
			if ((flags & Flags.Carry) != 0)
			{
				output.Append("carryFlag");
				output.AppendLine(flagValue);
			}
		}

		#endregion

		#region Operand Utility Functions

		static string GetOperandString(Operand operand, byte embeddedValue)
		{
			switch (operand)
			{
				case Operand.A: return "a";
				case Operand.B: return "b";
				case Operand.C: return "c";
				case Operand.D: return "d";
				case Operand.E: return "e";
				case Operand.H: return "h";
				case Operand.L: return "l";

				case Operand.Af: return "__temp16";
				case Operand.Bc: return "__temp16";
				case Operand.De: return "__temp16";
				case Operand.Hl: return "__tempHL";
				case Operand.Sp: return "sp";

				case Operand.Embedded: return embeddedValue.ToString();

				// Memory operations are 8 bit most of the time
				case Operand.Memory:
				case Operand.MemoryBc:
				case Operand.MemoryDe:
				case Operand.MemoryHl:
				case Operand.BytePort:
				case Operand.RegisterPort:
				case Operand.Byte: return "__temp8";

				// Some operands have to be extended to 16 bits for easier manipulation
				case Operand.SByte:
				case Operand.StackRelative:
				case Operand.Word: return "__temp16";

				case Operand.NotZero: return "!zeroFlag";
				case Operand.Zero: return "zeroFlag";
				case Operand.NotCarry: return "!carryFlag";
				case Operand.Carry: return "carryFlag";

				default: return null;
			}
		}

		static string GetOperandLoadStub(Operand operand)
		{
			switch (operand)
			{
				case Operand.Af: return "__temp16 = (ushort)((a << 8) | (zeroFlag?0x80:0) | (negationFlag?0x40:0) | (halfCarryFlag?0x20:0) | (carryFlag?0x10:0));";
				case Operand.Bc: return "__temp16 = (ushort)((b << 8) | c);";
				case Operand.De: return "__temp16 = (ushort)((d << 8) | e);";
				case Operand.Hl: return "__tempHL = (ushort)((h << 8) | l);";

				case Operand.Memory: return "__temp8 = bus.ReadByte(bus.ReadByte(pc++), bus.ReadByte(pc++));";
				case Operand.MemoryBc: return "__temp8 = bus.ReadByte(c, b);";
				case Operand.MemoryDe: return "__temp8 = bus.ReadByte(e, d);";
				case Operand.MemoryHl: return "__temp8 = bus.ReadByte(l, h);";
				case Operand.BytePort: return "__temp8 = bus.ReadPort(bus.ReadByte(pc++));";
				case Operand.RegisterPort: return "__temp8 = bus.ReadPort(c);";
				case Operand.Byte: return "__temp8 = bus.ReadByte(pc++);";

				case Operand.SByte: return "__temp16 = (ushort)(sbyte)bus.ReadByte(pc++);";
				case Operand.StackRelative: return "__temp16 = (ushort)(sp + (sbyte)bus.ReadByte(pc++));";
				case Operand.Word: return "__temp16 = (ushort)(bus.ReadByte(pc++) | (bus.ReadByte(pc++) << 8));";

				default: return null;
			}
		}

		static string GetOperandStoreStub(Operand operand)
		{
			switch (operand)
			{
				case Operand.Af: return "a = (byte)(__temp16 >> 8); zeroFlag = (__temp16 & 0x80) != 0; negationFlag = (__temp16 & 0x40) != 0; halfCarryFlag = (__temp16 & 0x20) != 0; carryFlag = (__temp16 & 0x10) != 0;";
				case Operand.Bc: return "b = (byte)(__temp16 >> 8); c = (byte)(__temp16);";
				case Operand.De: return "d = (byte)(__temp16 >> 8); e = (byte)(__temp16);";
				case Operand.Hl: return "h = (byte)(__tempHL >> 8); l = (byte)(__tempHL);";

				case Operand.Memory: return "bus.WriteByte(bus.ReadByte(pc++), bus.ReadByte(pc++), __temp8);";
				case Operand.MemoryBc: return "bus.WriteByte(c, b, __temp8);";
				case Operand.MemoryDe: return "bus.WriteByte(e, d, __temp8);";
				case Operand.MemoryHl: return "bus.WriteByte(l, h, __temp8);";
				case Operand.BytePort: return "bus.WritePort(bus.ReadByte(pc++), __temp8);";
				case Operand.RegisterPort: return "bus.WritePort(c, __temp8);";

				default: return null;
			}
		}

		#endregion

		#endregion
	}
}
