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
using CrystalBoy.Core;

namespace CrystalBoy.Core.OpcodeTableGenerator
{
	public static class Utility
	{
		static readonly string[] registers = { "BC", "DE", "HL", "SP" };
		static readonly string[] registers2 = { "BC", "DE", "HL", "AF" };
		static readonly string[] destinations = { "B", "C", "D", "E", "H", "L", "(HL)", "A" };
		static readonly string[] alu = { "ADD", "ADC", "SUB", "SBC", "AND", "XOR", "OR", "CP" };
		static readonly string[] flags = { "NZ", "Z", "NC", "C" };
		static readonly string[] incdec = { "INC ", "DEC " };
		static readonly string[] pushpop = { "POP ", "PUSH " };
		static readonly string[] shifts = { "RLC ", "RRC ", "RL ", "RR ", "SLA ", "SRA ", "SWAP ", "SRL " };
		static readonly string[] bits = { "BIT ", "RES ", "SET " };
		static readonly string[] jumps = { "RET ", null, "JP ", null, "CALL ", null, null, null };

		static readonly Dictionary<string, Operation> operationDictionary;
		static readonly Dictionary<string, Operand> operandDictionary;

		static Utility()
		{
			operationDictionary = new Dictionary<string, Operation>();
			operandDictionary = new Dictionary<string, Operand>();

			foreach (string name in Enum.GetNames(typeof(Operation)))
				operationDictionary.Add(name.ToUpperInvariant(), (Operation)Enum.Parse(typeof(Operation), name, true));

			operandDictionary.Add("${0:X2}", Operand.Byte);
			operandDictionary.Add("($FF{0:X2})", Operand.BytePort);
			//operandDictionary.Add("${0:X2}", Operand.SByte);
			operandDictionary.Add("SP+${0:X2}", Operand.StackRelative);
			operandDictionary.Add("${0:X4}", Operand.Word);
			operandDictionary.Add("(${0:X4})", Operand.Memory);
			operandDictionary.Add("A", Operand.A);
			operandDictionary.Add("B", Operand.B);
			operandDictionary.Add("C", Operand.C);
			operandDictionary.Add("D", Operand.D);
			operandDictionary.Add("E", Operand.E);
			operandDictionary.Add("H", Operand.H);
			operandDictionary.Add("L", Operand.L);
			operandDictionary.Add("(BC)", Operand.MemoryBc);
			operandDictionary.Add("(DE)", Operand.MemoryDe);
			operandDictionary.Add("(HL)", Operand.MemoryHl);
			operandDictionary.Add("(C)", Operand.RegisterPort);
			operandDictionary.Add("AF", Operand.Af);
			operandDictionary.Add("BC", Operand.Bc);
			operandDictionary.Add("DE", Operand.De);
			operandDictionary.Add("HL", Operand.Hl);
			operandDictionary.Add("SP", Operand.Sp);
			operandDictionary.Add("NZ", Operand.NotZero);
			operandDictionary.Add("Z", Operand.Zero);
			operandDictionary.Add("NC", Operand.NotCarry);
			//operandDictionary.Add("C", Operand.Carry);
			operandDictionary.Add("{0:X2}", Operand.Embedded);
		}

		public static string GetOpcodeFormatString(byte opcode)
		{
			string register;

			switch (opcode)
			{
				case 0x00: return "NOP";
				case 0x07: return "RLCA";
				case 0x0F: return "RRCA";
				case 0x17: return "RLA";
				case 0x1F: return "RRA";
				case 0x10: return "STOP";
				case 0x18: return "JR ${0:X2}";
				case 0x22: return "LDI (HL),A";
				case 0x2A: return "LDI A,(HL)";
				case 0x32: return "LDD (HL),A";
				case 0x3A: return "LDD A,(HL)";
				case 0x27: return "DAA";
				case 0x2F: return "CPL";
				case 0x37: return "SCF";
				case 0x3F: return "CCF";
				case 0x76: return "HALT";
				case 0xC9: return "RET";
				case 0xD9: return "RETI";
				case 0xC3: return "JP ${0:X4}";
				case 0xCD: return "CALL ${0:X4}";
				case 0x08: return "LD (${0:X4}),SP";
				case 0xE8: return "ADD SP,${0:X2}";
				case 0xF8: return "LD HL,SP+${0:X2}";
				case 0xE0: return "LD ($FF{0:X2}),A";
				case 0xF0: return "LD A,($FF{0:X2})";
				case 0xE2: return "LD (C),A";
				case 0xF2: return "LD A,(C)";
				case 0xEA: return "LD (${0:X4}),A";
				case 0xFA: return "LD A,(${0:X4})";
				case 0xE9: return "JP HL";
				case 0xF9: return "LD SP,HL";
				case 0xF3: return "DI";
				case 0xFB: return "EI";
				case 0xCB: return null; // CB opcodes
				default:
					if ((opcode & 0xC4) == 0 && (opcode & 3) != 0)
					{
						register = registers[opcode >> 4]; // We know that (opcode1 & 0xC0) == 0

						switch (opcode & 0x0F)
						{
							case 0x01: return "LD " + register + ",${0:X4}";
							case 0x09: return "ADD HL," + register;
							case 0x02: return "LD (" + register + "),A";
							case 0x0A: return "LD A,(" + register + ")";
							case 0x03: return "INC " + register;
							case 0x0B: return "DEC " + register;
							default: return null;
						}
					}
					else if ((opcode & 0xC4) == 0x04)
					{
						register = destinations[opcode >> 3];  // We still know that (opcode1 & 0xC0) == 0

						if ((opcode & 0x02) == 0x02) // LD D,N
							return "LD " + register + ",${0:X2}";
						else
							return incdec[opcode & 0x01] + register;
					}
					else if ((opcode & 0xC0) == 0x40) // LD D,D (HALT opcode already checked)
						return "LD " + destinations[(opcode >> 3) & 0x7] + "," + destinations[opcode & 0x7];
					else if ((opcode & 0xC0) == 0x80) // ALU A,D
						return alu[(opcode >> 3) & 0x7] + " " + "A," + destinations[opcode & 0x7];
					else if ((opcode & 0xC7) == 0xC6) // ALU A,N
						return alu[(opcode >> 3) & 0x7] + " " + "A,${0:X2}";
					else if ((opcode & 0xCB) == 0xC1) // PUSH R / POP R
					{
						register = registers2[(opcode >> 4) & 0x3];

						return pushpop[(opcode >> 2) & 0x1] + register;
					}
					else if ((opcode & 0xC7) == 0xC7) // RST N
						return "RST $" + (opcode & 0x38).ToString("X2");
					else if ((opcode & 0xE7) == 0x20) // JR F,N
						return "JR " + flags[(opcode >> 3) & 0x3] + ",${0:X2}";
					else if ((opcode & 0xE1) == 0xC0) // Other conditional jumps
					{
						register = jumps[opcode & 0x7] + flags[(opcode >> 3) & 0x3];

						if ((opcode & 0xE7) == 0xC0) // RET
							return register;
						else
							return register + ",${0:X4}";
					}
					else
						return null;
			}
		}

		public static string GetExtendedOpcodeFormatString(byte opcode)
		{
			string destination = destinations[opcode & 0x7];

			if ((opcode & 0xC0) == 0)
				return shifts[(opcode >> 3) & 0x7] + destination;
			else
				return bits[(opcode >> 6) - 1] + ((opcode >> 3) & 0x7).ToString() + "," + destination;
		}

		public static int GetOpcodeExtraByteCount(byte firstByte)
		{
			if ((firstByte & 0xCF) == 0x01 // LD R,N
				|| (firstByte & 0xE7) == 0xC2 // JP F,N
				|| (firstByte & 0xE7) == 0xC4 // CALL F,N
				|| (firstByte & 0xEF) == 0xEA // LD (N),A / LD A,(N)
				|| firstByte == 0xC3 // JP N
				|| firstByte == 0xCD // CALL N
				|| firstByte == 0x08) // LD (N),SP
				return 2;
			else if ((firstByte & 0xC7) == 0x06 // LD D,N
				|| firstByte == 0x18 // JR N
				|| (firstByte & 0xE7) == 0x20 // JR F,N
				|| (firstByte & 0xC7) == 0xC6 // ALU A,N
				|| (firstByte & 0xE7) == 0xE0 // ADD SP,N / LD (HL),SP+N / LD (FFNN),A / LD A,(FFNN)
				|| firstByte == 0xCB) // CB opcodes
				return 1;
			else
				return 0;
		}

		public static void ParseFormatString(string formatString, out Operation operation, out Operand operand1, out Operand operand2, out byte embedded)
		{
			string[] instruction;

			operation = Operation.Invalid;
			operand1 = Operand.None;
			operand2 = Operand.None;
			embedded = 0;

			if (formatString == null || formatString.Length == 0)
				return;

			instruction = formatString.Split(' ', ',');

			operation = operationDictionary[instruction[0]];

			if (instruction.Length > 1)
			{
				string value = instruction[1];

				if (char.IsDigit(value[0]))
				{
					operand1 = Operand.Embedded;
					embedded = (byte)(value[0] - '0');
				}
				else if (value[0] == '$' && char.IsDigit(value[1]))
				{
					operand1 = Operand.Embedded;
					embedded = byte.Parse(value.Substring(1), System.Globalization.NumberStyles.HexNumber);
				}
				else
					operand1 = operandDictionary[instruction[1]];
			}
			if (instruction.Length > 2)
				operand2 = operandDictionary[instruction[2]];

			// Correction for C register and C flag confusion (C flag for jump instructions, C register for others)
			if (operand1 == Operand.C && (operation == Operation.Jr || operation == Operation.Jp || operation == Operation.Call || operation == Operation.Ret))
				operand1 = Operand.Carry;
			// Correction for signed byte operands
			if (operation == Operation.Jr)
			{
				if (operand2 == Operand.None)
					operand1 = Operand.SByte;
				else
					operand2 = Operand.SByte;
			}
			else if (operation == Operation.Add && operand1 == Operand.Sp)
				operand2 = Operand.SByte;
		}

		public static void GetOpcodeAffectedFlags(Operation operation, Operand operand1, Operand operand2, out Flags affectedFlags, out Flags setFlags, out Flags clearFlags)
		{
			affectedFlags = Flags.None;
			setFlags = Flags.None;
			clearFlags = Flags.None;

			switch (operation)
			{
				case Operation.Ld:
					if (operand1 == Operand.Hl && operand2 == Operand.StackRelative)
					{
						affectedFlags = Flags.All;
						clearFlags = Flags.Zero | Flags.Negation;
					}
					break;

				case Operation.Pop:
					if (operand1 == Operand.Af)
						affectedFlags = Flags.All;
					break;

				case Operation.Add:
					if (operand1 == Operand.Hl)
						affectedFlags = Flags.Negation | Flags.HalfCarry | Flags.Carry;
					else
						affectedFlags = Flags.All;
					if (operand1 == Operand.Sp)
						clearFlags = Flags.Zero | Flags.Negation;
					else
						clearFlags = Flags.Negation;
					break;
				case Operation.Adc:
					affectedFlags = Flags.All;
					clearFlags = Flags.Negation;
					break;
				case Operation.Sub:
					affectedFlags = Flags.All;
					setFlags = Flags.Negation;
					break;
				case Operation.Sbc:
					affectedFlags = Flags.All;
					setFlags = Flags.Negation;
					break;
				case Operation.And:
					affectedFlags = Flags.All;
					setFlags = Flags.HalfCarry;
					clearFlags = Flags.Carry | Flags.Negation;
					break;
				case Operation.Xor:
					affectedFlags = Flags.All;
					clearFlags = Flags.Carry | Flags.HalfCarry | Flags.Negation;
					break;
				case Operation.Or:
					affectedFlags = Flags.All;
					clearFlags = Flags.Carry | Flags.HalfCarry | Flags.Negation;
					break;
				case Operation.Cp:
					affectedFlags = Flags.All;
					setFlags = Flags.Negation;
					break;

				case Operation.Inc:
					if (operand1 != Operand.Bc && operand1 != Operand.De && operand1 != Operand.Hl && operand1 != Operand.Sp)
					{
						affectedFlags = Flags.Zero | Flags.Negation | Flags.HalfCarry;
						clearFlags = Flags.Negation;
					}
					break;
				case Operation.Dec:
					if (operand1 != Operand.Bc && operand1 != Operand.De && operand1 != Operand.Hl && operand1 != Operand.Sp)
					{
						affectedFlags = Flags.Zero | Flags.Negation | Flags.HalfCarry;
						setFlags = Flags.Negation;
					}
					break;

				case Operation.Daa:
					affectedFlags = Flags.Zero | Flags.HalfCarry | Flags.Carry;
					clearFlags = Flags.HalfCarry;
					break;

				case Operation.Cpl:
					affectedFlags = Flags.Negation | Flags.HalfCarry;
					setFlags = Flags.Negation | Flags.HalfCarry;
					break;

				case Operation.Ccf:
					affectedFlags = Flags.Negation | Flags.HalfCarry | Flags.Carry;
					clearFlags = Flags.Negation | Flags.HalfCarry;
					break;
				case Operation.Scf:
					affectedFlags = Flags.Negation | Flags.HalfCarry | Flags.Carry;
					setFlags = Flags.Carry;
					clearFlags = Flags.Negation | Flags.HalfCarry;
					break;

				case Operation.Bit:
					affectedFlags = Flags.Zero | Flags.Negation | Flags.HalfCarry;
					setFlags = Flags.HalfCarry;
					clearFlags = Flags.Negation;
					break;

				case Operation.Swap:
					affectedFlags = Flags.All;
					clearFlags = Flags.Negation | Flags.HalfCarry | Flags.Carry;
					break;

				case Operation.Rlca:
				case Operation.Rrca:
				case Operation.Rla:
				case Operation.Rra:
					affectedFlags = Flags.All;
					clearFlags = Flags.Zero | Flags.Negation | Flags.HalfCarry;
					break;

				case Operation.Rlc:
				case Operation.Rrc:
				case Operation.Rl:
				case Operation.Rr:
				case Operation.Sla:
				case Operation.Sra:
				case Operation.Srl:
					affectedFlags = Flags.All;
					clearFlags = Flags.Negation | Flags.HalfCarry;
					break;
			}
		}

		public static byte GetOperandExtraCycleCount(Operand operand)
		{
			switch (operand)
			{
				case Operand.RegisterPort: return 4;
				case Operand.BytePort: return 8;

				case Operand.Memory: return 12;

				case Operand.MemoryBc: goto case Operand.MemoryHl;
				case Operand.MemoryDe: goto case Operand.MemoryHl;
				case Operand.MemoryHl: return 4;

				case Operand.Byte: return 4;
				case Operand.Word: return 8;

				case Operand.SByte: return 8;
				case Operand.StackRelative: return 4;

				default: return 0;
			}
		}

		public static bool IsWordOperand(Operand operand)
		{
			switch (operand)
			{
				case Operand.Af:
				case Operand.Bc:
				case Operand.De:
				case Operand.Hl:
				case Operand.Sp:

				case Operand.StackRelative:
				case Operand.Word:
					return true;

				default: return false;
			}
		}

		public static bool IsWordCompatibleOperand(Operand operand)
		{
			switch (operand)
			{
				case Operand.Af:
				case Operand.Bc:
				case Operand.De:
				case Operand.Hl:
				case Operand.Sp:

				case Operand.StackRelative:
				case Operand.SByte:
				case Operand.Word:

				case Operand.Memory:
				case Operand.MemoryBc:
				case Operand.MemoryDe:
				case Operand.MemoryHl:

				case Operand.None:
					return true;

				default: return false;
			}
		}

		public static void GetOpcodeCycleCount(Operation operation, Operand operand1, Operand operand2, out byte baseCycleCount, out byte conditionalCycleCount)
		{
			conditionalCycleCount = 0;
			switch (operation)
			{
				case Operation.Jp:
					if (operand2 != Operand.None)
					{
						baseCycleCount = 12;
						conditionalCycleCount = 4;
					}
					else if (operand1 == Operand.Hl)
						baseCycleCount = 4;
					else
						baseCycleCount = 16;
					break;
				case Operation.Jr:
					if (operand2 != Operand.None)
					{
						baseCycleCount = 8;
						conditionalCycleCount = 4;
					}
					else
						baseCycleCount = 12;
					break;
				case Operation.Call:
					if (operand2 != Operand.None)
					{
						baseCycleCount = 12;
						conditionalCycleCount = 12;
					}
					else
						baseCycleCount = 24;
					break;
				case Operation.Rst:
					goto case Operation.Reti;
				case Operation.Reti:
					baseCycleCount = 16;
					break;
				case Operation.Ret:
					if (operand1 != Operand.None)
					{
						baseCycleCount = 8;
						conditionalCycleCount = 12;
					}
					else
						baseCycleCount = 16;
					break;
				case Operation.Push:
					baseCycleCount = 16;
					break;
				case Operation.Pop:
					baseCycleCount = 12;
					break;
				default:
					bool isWordOperation = IsWordOperand(operand1) && IsWordCompatibleOperand(operand2) ||
						IsWordOperand(operand2) && IsWordCompatibleOperand(operand1);

					baseCycleCount = 4;

					// Adds 4 cycles for CB-prefixed instructions
					if (operation == Operation.Rlc || operation == Operation.Rrc ||
						operation == Operation.Rl || operation == Operation.Rr ||
						operation == Operation.Sla || operation == Operation.Sra ||
						operation == Operation.Swap || operation == Operation.Srl ||
						operation == Operation.Bit || operation == Operation.Set || operation == Operation.Res)
						baseCycleCount += 4;

					if (operand1 == Operand.None && operand2 == Operand.None)
						return;

					if (isWordOperation)
						baseCycleCount += 4;

					// Process single-operand operations, where destination is read once and written once
					if (operand2 == Operand.None)
						baseCycleCount += (byte)(2 * GetOperandExtraCycleCount(operand1));
					else if (operand1 == Operand.Embedded && operation != Operation.Bit) // Bit is a special case where the destination is not written
						baseCycleCount += (byte)(2 * GetOperandExtraCycleCount(operand2));
					// Process load operations (destination and source are used only once)
					else if (operation == Operation.Ld || operation == Operation.Ldi || operation == Operation.Ldd)
					{
						if (operand2 == Operand.Word)
							baseCycleCount = 12;
						else
							baseCycleCount += (byte)(GetOperandExtraCycleCount(operand1) + GetOperandExtraCycleCount(operand2));
					}
					// Process remaining operations (mainly arithmetical) where destination is usually read once and written once
					else
						baseCycleCount += (byte)(2 * GetOperandExtraCycleCount(operand1) + GetOperandExtraCycleCount(operand2));
					break;
			}
		}

		public static OpcodeInfo GetOpcodeInfo(byte opcode)
		{
			OpcodeInfo opcodeInfo;

			opcodeInfo.ExtraByteCount = GetOpcodeExtraByteCount(opcode);
			opcodeInfo.FormatString = GetOpcodeFormatString(opcode);
			ParseFormatString(opcodeInfo.FormatString, out opcodeInfo.Operation, out opcodeInfo.FirstOperand, out opcodeInfo.SecondOperand, out opcodeInfo.EmbeddedValue);
			GetOpcodeAffectedFlags(opcodeInfo.Operation, opcodeInfo.FirstOperand, opcodeInfo.SecondOperand, out opcodeInfo.AffectedFlags, out opcodeInfo.SetFlags, out opcodeInfo.ClearFlags);
			GetOpcodeCycleCount(opcodeInfo.Operation, opcodeInfo.FirstOperand, opcodeInfo.SecondOperand, out opcodeInfo.BaseCycleCount, out opcodeInfo.ConditionalCycleCount);

			return opcodeInfo;
		}

		public static OpcodeInfo GetExtendedOpcodeInfo(byte opcode)
		{
			OpcodeInfo opcodeInfo;

			opcodeInfo.ExtraByteCount = 0;
			opcodeInfo.FormatString = GetExtendedOpcodeFormatString(opcode);
			ParseFormatString(opcodeInfo.FormatString, out opcodeInfo.Operation, out opcodeInfo.FirstOperand, out opcodeInfo.SecondOperand, out opcodeInfo.EmbeddedValue);
			GetOpcodeAffectedFlags(opcodeInfo.Operation, opcodeInfo.FirstOperand, opcodeInfo.SecondOperand, out opcodeInfo.AffectedFlags, out opcodeInfo.SetFlags, out opcodeInfo.ClearFlags);
			GetOpcodeCycleCount(opcodeInfo.Operation, opcodeInfo.FirstOperand, opcodeInfo.SecondOperand, out opcodeInfo.BaseCycleCount, out opcodeInfo.ConditionalCycleCount);

			return opcodeInfo;
		}
	}
}
