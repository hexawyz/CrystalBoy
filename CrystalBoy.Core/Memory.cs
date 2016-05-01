using System;

namespace CrystalBoy.Core
{
	internal sealed class Memory
	{
		public static unsafe void Copy(void* destination, void* source, int length) { Copy(destination, source, checked((uint)length)); }

		public static unsafe void Copy(void* destination, void* source, uint length) { throw new NotImplementedException(); }

		public static unsafe void Set(void* destination, byte value, int length) { Set(destination, value, checked((uint)length)); }

		public static unsafe void Set(void* destination, byte value, uint length) { throw new NotImplementedException(); }
	}
}