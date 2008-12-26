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
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using CrystalBoy.Core;
using CrystalBoy.Disassembly;
using DisassemblyOperand = CrystalBoy.Disassembly.Operand;

namespace CrystalBoy.Decompiler.Analyzer
{
	class CodeMap
	{
		class AsyncResult : IAsyncResult
		{
			public object AsyncState
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public WaitHandle AsyncWaitHandle
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public bool CompletedSynchronously
			{
				get
				{
					return false;
				}
			}

			public bool IsCompleted
			{
				get
				{
					throw new NotImplementedException();
				}
			}
		}

		public Dictionary<int, Label> labelDictionary;
		public List<Instruction> instructionList;
		MemoryBlock rom;
		Thread analyzeThread;
		bool analyzing, analyzed;
		AsyncResult asyncResult;

		public CodeMap(MemoryBlock rom)
		{
			if (rom == null)
				throw new ArgumentNullException("rom");
			if (rom.Length < 32768) // Refuse to analyze a ROM smaller than 32 KB
				throw new InvalidOperationException();

			this.rom = rom;
			labelDictionary = new Dictionary<int, Label>();
			instructionList = new List<Instruction>();
			analyzeThread = new Thread(Analyze);
			asyncResult = new AsyncResult();
		}

		public IAsyncResult BeginAnalyze()
		{
			if (analyzed || analyzing)
				throw new InvalidOperationException();

			analyzing = true;

			analyzeThread.Start();

			return asyncResult;
		}

		public void EndAnalyze()
		{
			analyzeThread.Abort();
			analyzing = false;
		}

		private void Analyze()
		{
			Stack<int> functionPointerStack;
			Stack<Label> entryPointLabelStack;

			// Creates a stack of function pointers registered for analyzis
			functionPointerStack = new Stack<int>();
			entryPointLabelStack = new Stack<Label>();

			// Pushes the known entry points onto the function pointer stack

			// V-Blank Interrupt
			functionPointerStack.Push(0x0040);
			entryPointLabelStack.Push(DefineLabel(0x100, "VBI"));
			// LCD Status Interrupt
			functionPointerStack.Push(0x0048);
			entryPointLabelStack.Push(DefineLabel(0x100, "LCDSTAT"));
			// Timer Interrupt
			functionPointerStack.Push(0x0050);
			entryPointLabelStack.Push(DefineLabel(0x100, "TIMER"));
			// Serial Interrupt
			functionPointerStack.Push(0x0058);
			entryPointLabelStack.Push(DefineLabel(0x100, "SERIAL"));
			// Joypad Inerrupt
			functionPointerStack.Push(0x0060);
			entryPointLabelStack.Push(DefineLabel(0x100, "JOY"));
			// GameBoy ROM Entry Point
			functionPointerStack.Push(0x0100);
			entryPointLabelStack.Push(DefineLabel(0x100, "RomEntryPoint"));

			StaticAnalyze(functionPointerStack);
			// Run the analyzis
			DynamicAnalyze(entryPointLabelStack);

			analyzed = true;
			analyzing = false;
		}

		/// <summary>
		/// Runs a dynamic analyzis.
		/// This is currently implemented as a dumb simulation process
		/// Every memory operations are ignored except for ROM and HRAM
		/// Reads on ROM will work as expected
		/// Memory bank switching will work as expected, and unresolved ROM calls will be resolved when possible
		/// Memory copies between ROM and HRAM will be tracked, and thus HRAM calls will be remapped to ROM calls
		/// </summary>
		/// <remarks>The analyzis ends once all jumps are resolved</remarks>
		/// <param name="functionPointerStack">Stack containing the entry points to analyze</param>
		private void DynamicAnalyze(Stack<Label> entryPointStack)
		{
			Stack<SimulationContext> contextStack = new Stack<SimulationContext>();

			while (entryPointStack.Count > 0)
			{
				SimulationContext currentContext = new SimulationContext();
				Label entryPointLabel = entryPointStack.Pop();
				int instructionIndex = -1;

				currentContext.PC = entryPointLabel.Offset;

				contextStack.Push(currentContext);

				while (contextStack.Count > 0)
				{
					Instruction instruction;
					Operand operand1, operand2;
					ushort rawValue1, rawValue2;
					ushort value1, value2;
					bool byteOperation;

					currentContext = contextStack.Peek();
					if (instructionIndex < 0)
						instructionIndex = instructionList.FindIndex(i => i.Offset == currentContext.PC);

					instruction = instructionList[instructionIndex];
					currentContext.PC = instruction.Offset;

					if (instruction is FlowControlInstruction)
					{
						Condition condition = ((FlowControlInstruction)instruction).Condition;

						// Skips conditional instructions when the condition is false
						if (condition == Condition.Never
							|| condition == Condition.NotZero && currentContext.ZeroFlag
							|| condition == Condition.Zero && !currentContext.ZeroFlag
							|| condition == Condition.CarryClear && currentContext.CarryFlag
							|| condition == Condition.CarrySet && !currentContext.CarryFlag)
							goto NextInstruction;
					}
					else if (instruction is BinaryInstruction)
					{
						operand1 = ((BinaryInstruction)instruction).Destination;
						operand2 = ((BinaryInstruction)instruction).Source;
						rawValue1 = GetOperandRawValue(currentContext, operand1);
						rawValue2 = GetOperandRawValue(currentContext, operand1);
						byteOperation = Operand.IsByteOperand(operand1) && Operand.IsByteOperand(operand2);
					}
					else if (instruction is UnaryInstruction)
					{
						operand1 = ((BinaryInstruction)instruction).Destination;
						byteOperation = Operand.IsByteOperand(operand1);
					}

					if (instruction is LoadInstruction)
					{
					}
					else if (instruction is JumpInstruction)
					{
					}
					else if (instruction is ReturnInstruction)
					{
						if (((ReturnInstruction)instruction).Condition == Condition.Always)
							contextStack.Pop();
					}
				NextInstruction:
					if (instructionIndex > 0)
					{
						instructionIndex++;
						if (instructionIndex >= instructionList.Count)
							throw new InvalidOperationException();
					}
				}
			}
		}

		private static ushort GetOperandRawValue(SimulationContext context, Operand operand)
		{
			if (operand is Operand<Register8>)
				return context.GetRegister8(((Operand<Register8>)operand).Value);
			else if (operand is Operand<Register16>)
				return context.GetRegister16(((Operand<Register16>)operand).Value);
			else if (operand is Operand<byte>)
				return ((Operand<byte>)operand).Value;
			else if (operand is Operand<ushort>)
				return ((Operand<ushort>)operand).Value;
			else if (operand is Operand<sbyte>)
			{
				if (operand.IsDirect)
					return (ushort)(context.SP + ((Operand<sbyte>)operand).Value);
				else
					return (ushort)(short)((Operand<sbyte>)operand).Value;
			}
			else
				throw new InvalidOperationException();
		}

		private unsafe void StaticAnalyze(Stack<int> functionPointerStack)
		{
			Stack<int> offsetStack = new Stack<int>();
			List<Instruction> fragment = new List<Instruction>();
			byte* pMemory = (byte*)rom.Pointer;
			int offset;

			// Loop on function entry points
			while (functionPointerStack.Count > 0)
			{
				// Get the offset of the next function to analyze
				offset = functionPointerStack.Pop();

				// Skip the analysis if the function has already been analyzed
				if (instructionList.Find(i => i.Offset == offset) != null)
					continue;

				// Create a new fragment
				fragment.Clear();

				// Push the function entry point on the offset stack
				offsetStack.Push(offset);

				// Loop on the label offsets
				while (offsetStack.Count > 0)
				{
					int currentBank, bankUpperBound;

					offset = offsetStack.Pop();
					currentBank = GetBankIndex(offset);
					bankUpperBound = GetBankUpperBound(currentBank);

					// Just skip the loop if the corresponding instruction has already been processed
					if (fragment.Find(i => i.Offset == offset) != null)
						continue;

					while (true)
					{
					#region Instruction Analysis

						OpcodeInfo opcodeInfo;
						byte opcode;
						ushort data = 0;
						int instructionOffset = offset;
						Instruction instruction;

						if (offset >= bankUpperBound)
						{
							fragment.Add(new EndOfBankInstruction(offset));
							break;
						}

						// We read the current opcode byte from the rom
						// Instructions can span on multiple bytes (up to 3 for the GB-z80)
						// Therefore it might happen that we encounter some incomplete instructions, which couldn't fit in only one memory area
						// So if we encounter such instruction, we insert in the stream a special marker named IncompleteInstruction
						// Here are the only two cases where we could possibly encounter a bank-incomplete instruction:
						//  - At the end of Bank 0, continued on any Other Bank (depending on the MBC used)
						//  - At the end of any Bank (depending on the MBC used), continuing on the Video RAM
						// Since Video RAM execution is very unlikely, the only case left is an instruction split over two banks
						// Still, incomplete instructions are very unlikely to happen, with one exception:
						// On roms smaller than 32K, there can be anything between Bank 0 and Bank 1, because these will always be mapped.
						// Therefore for this case only, we consider the ROM non-banked, with a single-bank size of 32768 bytes
						opcode = pMemory[offset++]; // We assume our offset here is always valid ;)

						if (opcode == 0xCB)
						{
							// Check for incomplete instruction
							if (offset >= bankUpperBound)
							{
								fragment.Add(new IncompleteInstruction(instructionOffset));
								break;
							}

							opcode = pMemory[offset++];
							opcodeInfo = Utility.GetExtendedOpcodeInfo(opcode);
						}
						else
							opcodeInfo = Utility.GetOpcodeInfo(opcode);

						// Check for incomplete instruction
						if (offset + opcodeInfo.ExtraByteCount > bankUpperBound)
						{
							fragment.Add(new IncompleteInstruction(instructionOffset));
							break;
						}

						if (opcodeInfo.ExtraByteCount >= 1)
						{
							data = pMemory[offset++];
							if (opcodeInfo.ExtraByteCount >= 2)
								data = (ushort)(data | (pMemory[offset++] << 8));
							instruction = MakeInstruction(instructionOffset, opcodeInfo, data);
						}
						else
							instruction = MakeInstruction(instructionOffset, opcodeInfo, 0);

						fragment.Add(instruction);

						// If we reach the end of a function, we stop the analysis for this branch
						if (instruction is ReturnInstruction)
							break;
						// If the isntruction is a jump and the destination is resolved, we add it on the list
						else if (instruction is ResolvedJumpInstruction)
						{
							ResolvedJumpInstruction jumpInstruction = (ResolvedJumpInstruction)instruction;

							// Process function calls in a different manner
							if (jumpInstruction.JumpType == JumpType.FunctionCall)
								functionPointerStack.Push(jumpInstruction.ResolvedDestination.Offset);
							else
								offsetStack.Push(jumpInstruction.ResolvedDestination.Offset);
						}
						// Then if the instruction was an absolute jump, we stop the analysis here
						if (opcodeInfo.Operation == Operation.Jp)
							break;

					#endregion
					}
				}

				MergeFragment(fragment);
			}
		}

		#region Utility Functions

		#region Instruction Analysis

		private Instruction MakeInstruction(int offset, OpcodeInfo opcodeInfo, ushort extraData)
		{
			Operand operand1, operand2;

			// Handle simple instructions
			if (opcodeInfo.FirstOperand == DisassemblyOperand.None)
			{
				switch (opcodeInfo.Operation)
				{
					case Operation.Nop: return new ControlInstruction(offset, ControlInstructionType.NoOperation);
					case Operation.Stop: return new ControlInstruction(offset, ControlInstructionType.Stop);
					case Operation.Halt: return new ControlInstruction(offset, ControlInstructionType.Halt);

					case Operation.Di: return new ControlInstruction(offset, ControlInstructionType.DisableInterrupts);
					case Operation.Ei: return new ControlInstruction(offset, ControlInstructionType.EnableInterrupts);

					case Operation.Ret: return new ReturnInstruction(offset);
					case Operation.Reti: return new ReturnInstruction(offset, true);

					case Operation.Rlca: return new ShiftInstruction(offset, ShiftInstructionFlags.RotateRightCircular, Operand.DirectA);
					case Operation.Rrca: return new ShiftInstruction(offset, ShiftInstructionFlags.RotateLeftCircular, Operand.DirectA);
					case Operation.Rla: return new ShiftInstruction(offset, ShiftInstructionFlags.RotateRight, Operand.DirectA);
					case Operation.Rra: return new ShiftInstruction(offset, ShiftInstructionFlags.RotateLeft, Operand.DirectA);

					case Operation.Daa: return new UtilityInstruction(offset, UtilityOperation.DecimalAdjustAfter);
					case Operation.Cpl: return new UtilityInstruction(offset, UtilityOperation.DecimalAdjustAfter);
					case Operation.Scf: return new UtilityInstruction(offset, UtilityOperation.DecimalAdjustAfter);
					case Operation.Ccf: return new UtilityInstruction(offset, UtilityOperation.DecimalAdjustAfter);

					default: throw new NotImplementedException() /*return new ControlInstruction(offset, ControlInstructionType.NoOperation)*/;
				}
			}
			// Handle jump instructions (and the conditional return instruction too)
			else if (opcodeInfo.Operation == Operation.Jp
				|| opcodeInfo.Operation == Operation.Jr
				|| opcodeInfo.Operation == Operation.Call
				|| opcodeInfo.Operation == Operation.Rst
				|| opcodeInfo.Operation == Operation.Ret)
			{
				Label label;
				JumpType jumpType;
				Condition condition;
				ushort mappedOffset, destination;

				switch (opcodeInfo.FirstOperand)
				{
					case DisassemblyOperand.NotZero: condition = Condition.NotZero; break;
					case DisassemblyOperand.Zero: condition = Condition.Zero; break;
					case DisassemblyOperand.NotCarry: condition = Condition.CarryClear; break;
					case DisassemblyOperand.Carry: condition = Condition.CarrySet; break;
					default: condition = Condition.Always; break;
				}

				switch (opcodeInfo.Operation)
				{
					case Operation.Ret: return new ReturnInstruction(offset, condition);
					case Operation.Jr: jumpType = JumpType.Relative; break;
					case Operation.Jp: jumpType = JumpType.Absolute; break;
					default: jumpType = JumpType.FunctionCall; break;
				}

				if (offset < 0x8000)
					mappedOffset = (ushort)offset;
				else
					mappedOffset = (ushort)(0x4000 | (offset & 0x3FFF));

				if (opcodeInfo.Operation == Operation.Rst)
					destination = opcodeInfo.EmbeddedValue;
				else if (opcodeInfo.Operation == Operation.Jr)
					unchecked { destination = (ushort)(mappedOffset + 2 + (sbyte)(byte)extraData); } // Add 2 to the offset because of the jump instruction length
				else
					destination = extraData;

				if (destination >= 0x8000 || (IsRomBanked && destination >= 0x4000 && mappedOffset < 0x4000))
					return new JumpInstruction(offset, jumpType, condition, extraData);
				else if (jumpType == JumpType.FunctionCall)
					label = DefineFunctionLabel((offset & ~0x3FFF) | destination);
				else
					label = DefineLabel((offset & ~0x3FFF) | destination);
				return new ResolvedJumpInstruction(offset, jumpType, condition, destination, label);
			}
			// Handle single-operand instructions
			else if (opcodeInfo.SecondOperand == DisassemblyOperand.None)
			{
				operand1 = MakeOperand(opcodeInfo.FirstOperand, opcodeInfo.EmbeddedValue, extraData);

				switch (opcodeInfo.Operation)
				{
					case Operation.Inc: return new ArithmeticInstruction(offset, ArithmeticOperation.Add, operand1, new Operand<byte>(1));
					case Operation.Dec: return new ArithmeticInstruction(offset, ArithmeticOperation.Substract, operand1, new Operand<byte>(1));

					case Operation.Pop: return new PopInstruction(offset, operand1);
					case Operation.Push: return new PushInstruction(offset, operand1);

					case Operation.Rlc: return new ShiftInstruction(offset, ShiftInstructionFlags.RotateRightCircular, operand1);
					case Operation.Rrc: return new ShiftInstruction(offset, ShiftInstructionFlags.RotateLeftCircular, operand1);
					case Operation.Rl: return new ShiftInstruction(offset, ShiftInstructionFlags.RotateRight, operand1);
					case Operation.Rr: return new ShiftInstruction(offset, ShiftInstructionFlags.RotateLeft, operand1);
					case Operation.Sla: return new ShiftInstruction(offset, ShiftInstructionFlags.ShiftLeftArithmetical, operand1);
					case Operation.Sra: return new ShiftInstruction(offset, ShiftInstructionFlags.ShiftRightArithmetical, operand1);
					case Operation.Srl: return new ShiftInstruction(offset, ShiftInstructionFlags.ShiftRightLogical, operand1);

					case Operation.Swap: return new SwapInstruction(offset, operand1);

					default: throw new InvalidOperationException();
				}
			}
			// Handle remaining instructions
			else
			{
				operand1 = MakeOperand(opcodeInfo.FirstOperand, opcodeInfo.EmbeddedValue, extraData);
				operand2 = MakeOperand(opcodeInfo.SecondOperand, opcodeInfo.EmbeddedValue, extraData);

				switch (opcodeInfo.Operation)
				{
					case Operation.Ld: return new LoadInstruction(offset, operand1, operand2);
					case Operation.Ldi: return new LoadInstruction(offset, LoadOperationType.IncrementHl, operand1, operand2);
					case Operation.Ldd: return new LoadInstruction(offset, LoadOperationType.DecrementHl, operand1, operand2);

					case Operation.Add: return new ArithmeticInstruction(offset, ArithmeticOperation.Add, operand1, operand2);
					case Operation.Adc: return new ArithmeticInstruction(offset, ArithmeticOperation.AddWithCarry, operand1, operand2);
					case Operation.Sub: return new ArithmeticInstruction(offset, ArithmeticOperation.Substract, operand1, operand2);
					case Operation.Sbc: return new ArithmeticInstruction(offset, ArithmeticOperation.SubstractWithCarry, operand1, operand2);
					case Operation.And: return new ArithmeticInstruction(offset, ArithmeticOperation.And, operand1, operand2);
					case Operation.Xor: return new ArithmeticInstruction(offset, ArithmeticOperation.Xor, operand1, operand2);
					case Operation.Or: return new ArithmeticInstruction(offset, ArithmeticOperation.Or, operand1, operand2);
					case Operation.Cp: return new ArithmeticInstruction(offset, ArithmeticOperation.Compare, operand1, operand2);

					case Operation.Bit: return new BitInstruction(offset, BitOperation.Test, operand1, operand2);
					case Operation.Set: return new BitInstruction(offset, BitOperation.Set, operand1, operand2);
					case Operation.Res: return new BitInstruction(offset, BitOperation.Clear, operand1, operand2);

					default: throw new InvalidOperationException();
				}
			}
		}

		#endregion

		#region Operand Analysis

		private Operand MakeOperand(DisassemblyOperand operand, byte embeddedData, ushort extraData)
		{
			switch (operand)
			{
				// 16 bit Registers - Direct Addressing Mode
				case DisassemblyOperand.Af: return Operand.DirectAF;
				case DisassemblyOperand.Bc: return Operand.DirectBC;
				case DisassemblyOperand.De: return Operand.DirectDE;
				case DisassemblyOperand.Hl: return Operand.DirectHL;
				case DisassemblyOperand.Sp: return Operand.DirectSP;

				// 16 bit Registers - Indirect Addressing Mode
				case DisassemblyOperand.MemoryBc: return Operand.IndirectBC;
				case DisassemblyOperand.MemoryDe: return Operand.IndirectDE;
				case DisassemblyOperand.MemoryHl: return Operand.IndirectHL;

				// 8 bit Registers - Direct Addressing Mode
				case DisassemblyOperand.A: return Operand.DirectA;
				case DisassemblyOperand.B: return Operand.DirectB;
				case DisassemblyOperand.C: return Operand.DirectC;
				case DisassemblyOperand.D: return Operand.DirectD;
				case DisassemblyOperand.E: return Operand.DirectE;
				case DisassemblyOperand.H: return Operand.DirectH;
				case DisassemblyOperand.L: return Operand.DirectL;

				// 8 bit Registers - Indirect Addressing Mode
				case DisassemblyOperand.RegisterPort: return Operand.IndirectC;

				// 16 bit Immediate Value
				case DisassemblyOperand.Word: return new Operand<ushort>(extraData);

				// 16 bit Address
				case DisassemblyOperand.Memory: return new Operand<ushort>(extraData, false);

				// 8 bit Immediate Value
				case DisassemblyOperand.Byte: return new Operand<byte>((byte)extraData);

				// 8 bit Embedded Value
				case DisassemblyOperand.Embedded: return new Operand<byte>(embeddedData);

				// 8 bit Immediate Address
				case DisassemblyOperand.BytePort: return new Operand<byte>((byte)extraData, false);

				// Stack relative addressing
				case DisassemblyOperand.StackRelative: /*goto case DisassemblyOperand.SByte;
				case DisassemblyOperand.SByte: */return new Operand<SByte>((sbyte)(byte)extraData);

				default: throw new InvalidOperationException();
			}
		}

		#endregion

		#region Label Management

		private Label DefineLabel(int offset)
		{
			return DefineLabelCore(offset, null, false);
		}

		private Label DefineLabel(int offset, string name)
		{
			return DefineLabelCore(offset, name, false);
		}

		private Label DefineFunctionLabel(int offset)
		{
			return DefineLabelCore(offset, null, true);
		}

		private Label DefineFunctionLabel(int offset, string name)
		{
			return DefineLabelCore(offset, name, true);
		}

		private Label DefineLabelCore(int offset, string name, bool isFunction)
		{
			Label label;

			if (!labelDictionary.TryGetValue(offset, out label))
			{
				if (name != null)
					label = new Label(offset, name);
				else
					label = new Label(offset);

				labelDictionary.Add(offset, label);
			}

			if (isFunction)
				label.IsFunction = true;

			return label;
		}

		#endregion

		#region Rom Information

		public int GetBankIndex(int offset)
		{
			if (IsRomBanked)
				return offset >> 14;
			else
				return 0;
		}

		public int GetBankUpperBound(int bankIndex)
		{
			if (IsRomBanked)
				return (bankIndex + 1) << 14;
			else
				return 32768;
		}

		public int RomBankCount
		{
			get
			{
				return rom.Length >> 14;
			}
		}

		public bool IsRomBanked
		{
			get
			{
				return rom.Length > 32768;
			}
		}

		#endregion

		#region Instruction List Management

		private void MergeFragment(List<Instruction> fragment)
		{
			instructionList.AddRange(fragment);

			instructionList.Sort((i1, i2) => i1.Offset - i2.Offset);
		}
		
		#endregion

		#endregion
	}
}
