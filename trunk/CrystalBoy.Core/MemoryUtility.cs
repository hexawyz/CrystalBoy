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
using System.IO;

namespace CrystalBoy.Core
{
	public static class MemoryUtility
	{
		const int bufferLength = 16384;

		[ThreadStatic]
		static byte[] buffer;

		private static byte[] Buffer
		{
			get
			{
				if (buffer != null) return buffer;
				else return buffer = new byte[bufferLength];
			}
		}

		#region Reading

		public static MemoryBlock ReadFile(FileInfo fileInfo)
		{
			FileStream fileStream;

			// Open the file in exclusive mode
			using (fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
				return ReadStream(fileStream, checked((int)fileStream.Length));
		}

		public static MemoryBlock ReadStream(Stream stream, int length)
		{
			MemoryBlock memoryBlock;

			// Initialize variables
			memoryBlock = null;

			try
			{
				// Create the memory block once the file has been successfully opened
				memoryBlock = new MemoryBlock((int)length);

				// Read the file using the extension method defined below
				stream.Read(memoryBlock, 0, memoryBlock.Length);
			}
			catch
			{
				// Dispose the memory if needed
				if (memoryBlock != null)
					memoryBlock.Dispose();

				throw;
			}
			finally
			{
				// Close the file
				stream.Close();
			}

			return memoryBlock;
		}

		public static unsafe int Read(this Stream stream, MemoryBlock memoryBlock, int offset, int length)
		{
			byte* pMemory;
			int bytesRead, bytesToRead, totalBytesRead;
			byte[] buffer;

			if (memoryBlock == null)
				throw new ArgumentNullException();
			if (offset >= memoryBlock.Length && length != 0)
				throw new ArgumentOutOfRangeException("offset");
			if (length < 0 || offset + length > memoryBlock.Length)
				throw new ArgumentOutOfRangeException("length");

			// Get a pointer to the memory block
			pMemory = (byte*)memoryBlock.Pointer + offset;

			// Obtain a reference to the buffer (lazy allocation)
			buffer = Buffer;

			totalBytesRead = 0;
			bytesToRead = Math.Min(bufferLength, length);

			// Read the file in chunks
			fixed (byte* pBuffer = buffer)
			{
				while ((bytesRead = stream.Read(buffer, 0, bytesToRead)) > 0)
				{
					MemoryBlock.Copy(pMemory, pBuffer, bytesRead);
					totalBytesRead += bytesRead;
					pMemory += bytesRead;
					length -= bytesRead;
					if (length < bytesToRead)
						bytesToRead = length;
				}
			}

			return totalBytesRead;
		}

		public static unsafe int Read(this BinaryReader reader, MemoryBlock memoryBlock, int offset, int length)
		{
			byte* pMemory;
			int bytesRead, bytesToRead, totalBytesRead;
			byte[] buffer;

			if (memoryBlock == null)
				throw new ArgumentNullException();
			if (offset >= memoryBlock.Length && length != 0)
				throw new ArgumentOutOfRangeException("offset");
			if (length < 0 || offset + length > memoryBlock.Length)
				throw new ArgumentOutOfRangeException("length");

			// Get a pointer to the memory block
			pMemory = (byte*)memoryBlock.Pointer;

			// Obtain a reference to the buffer (lazy allocation)
			buffer = Buffer;

			totalBytesRead = 0;
			bytesToRead = Math.Min(bufferLength, length);

			// Read the file in chunks
			fixed (byte* pBuffer = buffer)
			{
				while ((bytesRead = reader.Read(buffer, 0, bytesToRead)) > 0)
				{
					Memory.Copy(pMemory, pBuffer, (uint)bytesRead);
					totalBytesRead += bytesRead;
					pMemory += bytesRead;
					length -= bytesRead;
					if (length < bytesToRead)
						bytesToRead = length;
				}
			}

			return totalBytesRead;
		}

		#endregion

		#region Writing

		public static unsafe void WriteFile(FileInfo fileInfo, MemoryBlock memoryBlock)
		{
			FileStream fileStream;

			if (fileInfo == null)
				throw new ArgumentNullException("fileInfo");
			if (memoryBlock == null)
				throw new ArgumentNullException("memoryBlock");

			// Open the file in exclusive mode
			using (fileStream = fileInfo.Open(FileMode.Open, FileAccess.Write, FileShare.Read))
				fileStream.Write(memoryBlock, 0, memoryBlock.Length);
		}

		public static unsafe void Write(this Stream stream, MemoryBlock memoryBlock, int offset, int length)
		{
			byte* pMemory;
			int bytesLeft,
				bytesToWrite;
			byte[] buffer;

			if (memoryBlock == null)
				throw new ArgumentNullException("memoryBlock");
			if (offset >= memoryBlock.Length && length != 0)
				throw new ArgumentOutOfRangeException("offset");
			if (length < 0 || offset + length > memoryBlock.Length)
				throw new ArgumentOutOfRangeException("length");

			// Initialize variables
			bytesLeft = length;
			bytesToWrite = bufferLength;

			// Get a pointer to the memory block
			pMemory = (byte*)memoryBlock.Pointer + offset;

			// Obtain a reference to the buffer (lazy allocation)
			buffer = Buffer;

			// Write the file in chunks
			fixed (byte* pBuffer = buffer)
			{
				while (bytesLeft > 0)
				{
					if (bytesLeft < bytesToWrite)
						bytesToWrite = bytesLeft;
					Memory.Copy(pBuffer, pMemory, (uint)bytesToWrite);
					stream.Write(buffer, 0, bytesToWrite);
					pMemory += bytesToWrite;
					bytesLeft -= bytesToWrite;
				}
			}
		}

		public static unsafe void Write(this BinaryWriter writer, MemoryBlock memoryBlock, int offset, int length)
		{
			byte* pMemory;
			int bytesLeft,
				bytesToWrite;
			byte[] buffer;

			if (memoryBlock == null)
				throw new ArgumentNullException("memoryBlock");
			if (offset >= memoryBlock.Length && length != 0)
				throw new ArgumentOutOfRangeException("offset");
			if (length < 0 || offset + length > memoryBlock.Length)
				throw new ArgumentOutOfRangeException("length");

			// Initialize variables
			bytesLeft = length;
			bytesToWrite = bufferLength;

			// Get a pointer to the memory block
			pMemory = (byte*)memoryBlock.Pointer + offset;

			// Obtain a reference to the buffer (lazy allocation)
			buffer = Buffer;

			// Write the file in chunks
			fixed (byte* pBuffer = buffer)
			{
				while (bytesLeft > 0)
				{
					if (bytesLeft < bytesToWrite)
						bytesToWrite = bytesLeft;
					Memory.Copy(pBuffer, pMemory, (uint)bytesToWrite);
					writer.Write(buffer, 0, bytesToWrite);
					pMemory += bytesToWrite;
					bytesLeft -= bytesToWrite;
				}
			}
		}

		#endregion
	}
}
