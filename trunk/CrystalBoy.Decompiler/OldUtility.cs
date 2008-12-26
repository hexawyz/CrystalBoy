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
using System.Linq;
using System.Text;

namespace GameBoyDecompiler
{
	static class Utility
	{
		static readonly string[] registers = { "BC", "DE", "HL", "SP" };
		static readonly string[] destinations = { "B", "C", "D", "E", "H", "L", "(HL)", "A" };
		static readonly string[] alu = { "ADD", "ADC", "SUB", "SBC", "AND", "XOR", "OR", "CP" };
		static readonly string[] flags = { "NZ", "Z", "NC", "C" };
		static readonly string[] incdec = { "INC\t", "DEC\t" };
		static readonly string[] pushpop = { "POP\t", "PUSH\t" };

		public static unsafe int Disassemble(MemoryBlock memoryBlock, int offset, out string instruction)
		{
		    byte* pMemory = (byte*)memoryBlock.Pointer;
			byte opcode1,
				opcode2 = 0x00,
				opcode3 = 0x00;
			string opcode2String = null,
				opcode3String = null,
				register;
		    int extraByteCount;

			instruction = "";

			if (offset >= memoryBlock.Length)
				return 0;

			opcode1 = pMemory[offset++];

			extraByteCount = GetInstructionExtraSize(opcode1);

			if (offset + extraByteCount > memoryBlock.Length)
			{
				instruction = "DB $" + opcode1.ToString("X2");
				return 1;
			}

			if (extraByteCount >= 1)
			{
				opcode2 = pMemory[offset++];
				if (opcode1 != 0xCB)
					opcode2String = opcode2.ToString("X2");
			}
			if (extraByteCount >= 2)
			{
				opcode3 = pMemory[offset++];
				opcode3String = opcode3.ToString("X2");
			}

			switch (opcode1)
			{
				case 0x00: instruction = "NOP"; break;
				case 0x07: instruction = "RLCA"; break;
				case 0x0F: instruction = "RRCA"; break;
				case 0x17: instruction = "RLA"; break;
				case 0x1F: instruction = "RRA"; break;
				case 0x10: instruction = "STOP"; break;
				case 0x18: instruction = "JR\t$" + opcode2String + "\t[" + (offset + (sbyte)opcode2).ToString("X4") + "]"; break;
				case 0x22: instruction = "LDI\t(HL),A"; break;
				case 0x2A: instruction = "LDI\tA,(HL)"; break;
				case 0x32: instruction = "LDD\t(HL),A"; break;
				case 0x3A: instruction = "LDD\tA,(HL)"; break;
				case 0x27: instruction = "DAA"; break;
				case 0x2F: instruction = "CPL"; break;
				case 0x37: instruction = "SCF"; break;
				case 0x3F: instruction = "CCF"; break;
				case 0x76: instruction = "HALT"; break;
				case 0xC9: instruction = "RET"; break;
				case 0xD9: instruction = "RETI"; break;
				case 0xC3: instruction = "JP\t$" + opcode3String + opcode2String; break;
				case 0xCD: instruction = "CALL\t$" + opcode3String + opcode2String; break;
				case 0xE8: instruction = "ADD\tSP,$" + opcode2String; break;
				case 0xF8: instruction = "LD\t(HL),SP+$" + opcode2String; break;
				case 0xE0: instruction = "LD\t($FF" + opcode2String + "),A"; break;
				case 0xF0: instruction = "LD\tA,($FF" + opcode2String + ")"; break;
				case 0xE2: instruction = "LD\t(C),A"; break;
				case 0xF2: instruction = "LD\tA,(C)"; break;
				case 0xEA: instruction = "LD\t($" + opcode3String + opcode2String + "),A"; break;
				case 0xFA: instruction = "LD\tA,($" + opcode3String + opcode2String + ")"; break;
				case 0xE9: instruction = "JP\tHL"; break;
				case 0xF9: instruction = "LD\tSP,HL"; break;
				case 0xF3: instruction = "DI"; break;
				case 0xFB: instruction = "EI"; break;
				case 0xCB:
					if ((opcode2 & 0xC0) == 0)
					{
						switch ((opcode2 >> 3) & 0x7)
						{
							case 0: instruction = "RLC\t"; break;
							case 1: instruction = "RRC\t"; break;
							case 2: instruction = "RL\t"; break;
							case 3: instruction = "RR\t"; break;
							case 4: instruction = "SLA\t"; break;
							case 5: instruction = "SRA\t"; break;
							case 6: instruction = "SWAP\t"; break;
							case 7: instruction = "SRL\t"; break;
						}
					}
					else
					{
						switch (opcode2 >> 6)
						{
							case 1: instruction = "BIT\t"; break;
							case 2: instruction = "SET\t"; break;
							case 3: instruction = "RES\t"; break;
						}

						instruction += ((opcode2 >> 3) & 0x7).ToString() + ",";
					}
					instruction += destinations[opcode2 & 0x7];
					break;
				default:
					if ((opcode1 & 0xC4) == 0 && (opcode1 & 3) != 0)
					{
						register = registers[opcode1 >> 4]; // We know that (opcode1 & 0xC0) == 0

						switch (opcode1 & 0x0F)
						{
							case 0x01: instruction = "LD\t" + register + ",$" + opcode3String + opcode2String; break;
							case 0x09: instruction = "ADD\tHL," + register; break;
							case 0x02: instruction = "LD\t(" + register + "),A"; break;
							case 0x0A: instruction = "LD\tA,(" + register + ")"; break;
							case 0x03: instruction = "INC\t" + register; break;
							case 0x0B: instruction = "DEC\t" + register; break;
						}
					}
					else if ((opcode1 & 0xC4) == 0x04)
					{
						register = destinations[opcode1 >> 3];  // We still know that (opcode1 & 0xC0) == 0

						if ((opcode1 & 0x02) == 0x02) // LD D,N
							instruction = "LD\t" + register + ",$" + opcode2String;
						else
							instruction = incdec[opcode1 & 0x01] + register;
					}
					else if ((opcode1 & 0xC0) == 0x40) // LD D,D (HALT opcode already checked)
						instruction = "LD\t" + destinations[(opcode1 >> 3) & 0x7] + "," + destinations[opcode1 & 0x7];
					else if ((opcode1 & 0xC0) == 0x80) // ALU A,D
						instruction = alu[(opcode1 >> 3) & 0x7] + "\t" + "A," + destinations[opcode1 & 0x7];
					else if ((opcode1 & 0xC7) == 0xC6) // ALU A,N
						instruction = alu[(opcode1 >> 3) & 0x7] + "\t" + "A,$" + opcode2String;
					else if ((opcode1 & 0xCB) == 0xC1) // PUSH R / POP R
					{
						register = registers[(opcode1 >> 4) & 0x3];

						instruction = pushpop[(opcode1 >> 2) & 0x1] + register;
					}
					else if ((opcode1 & 0xC7) == 0xC7) // RST N
						instruction = "RST\t$" + (opcode1 & 0x38).ToString("X2");
					else if ((opcode1 & 0xE7) == 0x20) // JR F,N
						instruction = "JR\t" + flags[(opcode1 >> 3) & 0x3] + ",$" + opcode2String + " [" + (offset + (sbyte)opcode2).ToString("X4") + "]";
					else if ((opcode1 & 0xE1) == 0xC0) // Other conditional jumps
					{
						register = flags[(opcode1 >> 3) & 0x3];

						switch (opcode1 & 0x3)
						{
							case 0x00: instruction = "RET\t"; break;
							case 0x02: instruction = "JP\t"; break;
							case 0x04: instruction = "CALL\t"; break;
						}
						if ((opcode1 & 0xE7) == 0xC0) // RET
							instruction += register;
						else
							instruction += register + ",$" + opcode3String + opcode2String;
					}
					else
						instruction = "DB $" + opcode1.ToString("X2");
					break;
			}

			return 1 + extraByteCount;
		}

		public static int GetInstructionExtraSize(byte firstByte)
		{
			if ((firstByte & 0xCF) == 0x01 // LD R,N
				|| (firstByte & 0xE7) == 0xC2 // JP F,N
				|| (firstByte & 0xE7) == 0xC4 // CALL F,N
				|| (firstByte & 0xEF) == 0xEA // LD (N),A / LD A,(N)
				|| firstByte == 0xC3 // JP N
				|| firstByte == 0xCD) // CALL N
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
	}
}
