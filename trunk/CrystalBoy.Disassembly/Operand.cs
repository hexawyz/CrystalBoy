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

namespace CrystalBoy.Disassembly
{
	public enum Operand : byte
	{
		// No operand
		None,
		// 8 bit Immediate
		Byte,
		BytePort, // ($FFNN)
		SByte, // Sign extended
		StackRelative, // Sign extended (SP+NN)
		// 16 bit Immediate
		Word,
		Memory,
		// 8 bit Destination
		A,
		B,
		C,
		D,
		E,
		H,
		L,
		MemoryBc, // (BC)
		MemoryDe, // (DE)
		MemoryHl, // (HL)
		RegisterPort, // (C)
		// 16 bit Register
		Af,
		Bc,
		De,
		Hl,
		Sp,
		// Condition
		NotZero,
		Zero,
		NotCarry,
		Carry,
		// Embedded immediate value (Used by RST, BIT, SET and RES)
		Embedded
	}
}
