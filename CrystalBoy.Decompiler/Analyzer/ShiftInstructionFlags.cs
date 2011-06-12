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

namespace CrystalBoy.Decompiler.Analyzer
{
	[Flags]
	enum ShiftInstructionFlags
	{
		Left = 0x00,
		Right = 0x01,

		Rotate = 0x00,
		Shift = 0x04,

		WithoutCarry = 0x00,
		WithCarry = 0x02,

		Arithmetical = 0x00,
		Logical = 0x02,


		RotateLeftCircular = 0x00,
		RotateRightCircular = 0x01,

		RotateLeft = 0x02,
		RotateRight = 0x03,

		ShiftLeftArithmetical = 0x04,
		ShiftRightArithmetical = 0x05,

		ShiftLeftLogical = 0x06, // Not implemented by Z80, replaced by SWAP on GB-Z80
		ShiftRightLogical = 0x07
	}
}
