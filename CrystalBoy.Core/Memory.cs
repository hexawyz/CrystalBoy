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
using System.Reflection.Emit;

namespace CrystalBoy.Core
{
	internal sealed class Memory
	{
		private unsafe delegate void CopyMemoryDelegate(void* destination, void* source, uint length);
		private unsafe delegate void SetMemoryDelegate(void* destination, byte value, uint length);

		private static DynamicMethod copyMemoryMethod = CreateCopyMemoryMethod();
		private static DynamicMethod setMemoryMethod = CreateSetMemoryMethod();

		private static CopyMemoryDelegate copyMemoryDelegate = (CopyMemoryDelegate)copyMemoryMethod.CreateDelegate(typeof(CopyMemoryDelegate));
		private static SetMemoryDelegate setMemoryDelegate = (SetMemoryDelegate)setMemoryMethod.CreateDelegate(typeof(SetMemoryDelegate));

		private static DynamicMethod CreateCopyMemoryMethod()
		{
			DynamicMethod copyMemoryMethod = new DynamicMethod("Copy", typeof(void), new Type[] { typeof(void*), typeof(void*), typeof(uint) }, typeof(Memory).Module, false);
			ILGenerator ilGenerator = copyMemoryMethod.GetILGenerator();

			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldarg_1);
			ilGenerator.Emit(OpCodes.Ldarg_2);
			ilGenerator.Emit(OpCodes.Cpblk);
			ilGenerator.Emit(OpCodes.Ret);

			return copyMemoryMethod;
		}

		private static DynamicMethod CreateSetMemoryMethod()
		{
			DynamicMethod setMemoryMethod = new DynamicMethod("Set", typeof(void), new Type[] { typeof(void*), typeof(byte), typeof(uint) }, typeof(Memory).Module, false);
			ILGenerator ilGenerator = setMemoryMethod.GetILGenerator();

			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldarg_1);
			ilGenerator.Emit(OpCodes.Ldarg_2);
			ilGenerator.Emit(OpCodes.Initblk);
			ilGenerator.Emit(OpCodes.Ret);

			return setMemoryMethod;
		}

		public static unsafe void Copy(void* destination, void* source, int length) { copyMemoryDelegate(destination, source, checked((uint)length)); }

		public static unsafe void Copy(void* destination, void* source, uint length) { copyMemoryDelegate(destination, source, length); }

		public static unsafe void Set(void* destination, byte value, int length) { setMemoryDelegate(destination, value, checked((uint)length)); }

		public static unsafe void Set(void* destination, byte value, uint length) { setMemoryDelegate(destination, value, length); }
	}
}