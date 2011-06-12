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
	abstract class Operand
	{
		#region Static Members

		public static InvalidOperand Invalid = new InvalidOperand();

		public static Operand<Register16> DirectAF = new Operand<Register16>(Register16.Af);
		public static Operand<Register16> DirectBC = new Operand<Register16>(Register16.Bc);
		public static Operand<Register16> DirectDE = new Operand<Register16>(Register16.De);
		public static Operand<Register16> DirectHL = new Operand<Register16>(Register16.Hl);
		public static Operand<Register16> DirectSP = new Operand<Register16>(Register16.Sp);

		public static Operand<Register16> IndirectBC = new Operand<Register16>(Register16.Bc, false);
		public static Operand<Register16> IndirectDE = new Operand<Register16>(Register16.De, false);
		public static Operand<Register16> IndirectHL = new Operand<Register16>(Register16.Hl, false);

		public static Operand<Register8> DirectA = new Operand<Register8>(Register8.A);
		public static Operand<Register8> DirectB = new Operand<Register8>(Register8.B);
		public static Operand<Register8> DirectC = new Operand<Register8>(Register8.C);
		public static Operand<Register8> DirectD = new Operand<Register8>(Register8.D);
		public static Operand<Register8> DirectE = new Operand<Register8>(Register8.E);
		public static Operand<Register8> DirectH = new Operand<Register8>(Register8.H);
		public static Operand<Register8> DirectL = new Operand<Register8>(Register8.L);

		public static Operand<Register8> IndirectC = new Operand<Register8>(Register8.C, false);

		/// <summary>
		/// Determines if a given operand is 8 bit compatible
		/// If both operands of a binary operation are 8 bit, then it is a 8 bit operation, otherwise it is a 16 bit operation
		/// </summary>
		/// <remarks>
		/// Operands of type sbyte are acounted for as 16 bit operands, as they will always be sign-extended to 16 bit
		/// </remarks>
		/// <param name="operand">The operand to test</param>
		/// <returns>Returns true if the operand is 8 bit compatible, false otherwise</returns>
		public static bool IsByteOperand(Operand operand)
		{
			return !operand.IsDirect || operand.ValueType == typeof(Register8) || operand.ValueType == typeof(byte);
		}

		#endregion

		bool isDirect;

		/// <summary>
		/// Initializes a new instance of the Operand class
		/// </summary>
		/// <param name="isDirect"></param>
		public Operand(bool isDirect)
		{
			this.isDirect = isDirect;
		}

		/// <summary>
		/// Gets a boolean value indicating wether the operand is direct or not
		/// </summary>
		public bool IsDirect
		{
			get
			{
				return isDirect;
			}
		}

		/// <summary>
		/// Gets the Type of the operand in this instance
		/// </summary>
		public abstract Type ValueType { get; }
	}

	sealed class InvalidOperand : Operand
	{
		internal InvalidOperand()
			: base(false)
		{
		}

		public override Type ValueType
		{
			get
			{
				return null;
			}
		}
	}

	sealed class Operand<T> : Operand
	{
		T value;

		public Operand(T value)
			: this(value, true)
		{
		}

		public Operand(T value, bool isDirect)
			: base(isDirect)
		{
			this.value = value;
		}

		public T Value
		{
			get
			{
				return value;
			}
		}

		public sealed override Type ValueType
		{
			get
			{
				return typeof(T);
			}
		}
	}
}
