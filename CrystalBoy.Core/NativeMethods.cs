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
using System.Security;
using System.Runtime.InteropServices;

namespace CrystalBoy.Core
{
#if WIN32 && PINVOKE
	static class NativeMethods
	{
		#region Heap Functions

		public const uint HEAP_NO_SERIALIZE = 0x00000001;
		public const uint HEAP_GROWABLE = 0x00000002;
		public const uint HEAP_GENERATE_EXCEPTIONS = 0x00000004;
		public const uint HEAP_ZERO_MEMORY = 0x00000008;
		public const uint HEAP_REALLOC_IN_PLACE_ONLY = 0x00000010;
		public const uint HEAP_TAIL_CHECKING_ENABLED = 0x00000020;
		public const uint HEAP_FREE_CHECKING_ENABLED = 0x00000040;
		public const uint HEAP_DISABLE_COALESCE_ON_FREE = 0x00000080;
		public const uint HEAP_CREATE_ALIGN_16 = 0x00010000;
		public const uint HEAP_CREATE_ENABLE_TRACING = 0x00020000;
		public const uint HEAP_CREATE_ENABLE_EXECUTE = 0x00040000;

		public const uint STATUS_NO_MEMORY = 0xC0000017;
		public const uint STATUS_ACCESS_VIOLATION = 0xC0000005;

		[DllImport("kernel32")]
		[SuppressUnmanagedCodeSecurity]
		public static extern IntPtr GetProcessHeap();

		[DllImport("kernel32")]
		[SuppressUnmanagedCodeSecurity]
		public static unsafe extern void* HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwBytes);

		[DllImport("kernel32")]
		[SuppressUnmanagedCodeSecurity]
		public static unsafe extern void* HeapReAlloc(IntPtr hHeap, uint dwFlags, void* lpMem, UIntPtr dwBytes);

		[DllImport("kernel32")]
		[SuppressUnmanagedCodeSecurity]
		[return:MarshalAs(UnmanagedType.Bool)]
		public static unsafe extern bool HeapFree(IntPtr hHeap, uint dwFlags, void* lpMem);

		[DllImport("kernel32", SetLastError = true)]
		[SuppressUnmanagedCodeSecurity]
		public static unsafe extern uint HeapSize(IntPtr hHeap, uint dwFlags, void* lpMem);

		#endregion
	}
#endif
}
