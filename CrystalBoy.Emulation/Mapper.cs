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

namespace CrystalBoy.Emulation
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class Mapper
	{
		GameBoyMemoryBus bus;

		unsafe byte* ram;

		bool handlesRamWrites;

		public Mapper(GameBoyMemoryBus bus)
		{
			if (bus == null)
				throw new ArgumentNullException();

			// Initializations to default values are left as comments for reference, but the code is not needed
			this.bus = bus;
		}

		/// <summary>
		/// Resets the Mapper
		/// </summary>
		/// <remarks>
		/// The default Reset implementation do the following operations
		/// <list type="bullet">
		/// <item>
		/// <description>Caches the RAM pointer, which can change on each reset (only if the requested RAM grows)</description>
		/// </item>
		/// <item>
		/// <description>Maps ROM bank 0 in the lower ROM area</description>
		/// </item>
		/// <item>
		/// <description>Maps ROM bank 1 in the upper ROM area</description>
		/// </item>
		/// <item>
		/// <description>Unmaps RAM from the RAM area</description>
		/// </item>
		/// <item>
		/// <description>Sets the custom port value to 0</description>
		/// </item>
		/// </list>
		/// If you override this methode, try to either call the base implementation, or repeat these (possibly customized) steps in your own method.
		/// </remarks>
		public unsafe virtual void Reset()
		{
			ram = (byte*)Bus.ExternalRam.Pointer;
			MapRomBank(false, 0);
			MapRomBank(true, 1);
			UnmapRam();
			SetPortValue(0);
		}

		/// <summary>
		/// Gets the GameBoyMemoryBus associated with this Mapper
		/// </summary>
		protected GameBoyMemoryBus Bus
		{
			get
			{
				return bus;
			}
		}

		/// <summary>
		/// Gets the RAM Size requested by the mapper.
		/// </summary>
		/// <remarks>
		/// The default implementation returns the ram size provided by the ROM information.
		/// If need to allocate more RAM for other purposes (memory mapped IO for example), override this propertie to request the proper amount of memory.
		/// </remarks>
		public virtual int RamSize
		{
			get
			{
				return Bus.RomInformation.RamSize;
			}
		}

		/// <summary>
		/// Gets the size of the RAM to save.
		/// </summary>
		/// <remarks>
		/// The default implementation returns the value returned by RamSize.
		/// If you requested more memory than what need to be saved, if battery is supported, just return the correct amount here.
		/// The first RAM banks will always be saved first, so it is better to put your custom data in the last RAM banks.
		/// </remarks>
		public virtual int SavedRamSize
		{
			get
			{
				return RamSize;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating wether the Mapper handles RAM Writes.
		/// </summary>
		/// <remarks>
		/// The default Mapper implementation does not handle RAM Writes.
		/// If you set this property to true, it is very likely that you need to override the HandleRamWrite method.
		/// </remarks>
		public bool HandlesRamWrites
		{
			get
			{
				return handlesRamWrites;
			}
			set
			{
				if (value != handlesRamWrites)
				{
					handlesRamWrites = value;
					bus.ResetRamWriteHandler();
				}
			}
		}

		/// <summary>
		/// Gets a pointer to the allocated RAM.
		/// </summary>
		[CLSCompliant(false)]
		protected unsafe byte* Ram
		{
			get
			{
				return ram;
			}
		}

		#region Bank Management Functions

		/// <summary>
		/// Maps a ROM bank into the upper or lower ROM area.
		/// </summary>
		/// <param name="upper">True for the upper ROM area, false for the lower one.</param>
		/// <param name="bankIndex">Index of the ROM bank to map.</param>
		protected void MapRomBank(bool upper, int bankIndex)
		{
			bus.MapExternalRomBank(upper, bankIndex);
		}

		/// <summary>
		/// Maps a custom port into the RAM area.
		/// </summary>
		/// <remarks>
		/// The port is handled internally by the GameBoyMemoryBus object associated with the Mapper.
		/// In the current implementation, it uses a 256 byte memory containing only one value, repeated all over the RAM area.
		/// If you wish to change the value assigned to this memory, you can do so by using the SetPortValue method.
		/// </remarks>
		protected void MapPort()
		{
			bus.MapExternalPort();
		}

		/// <summary>
		/// Sets the value assigned to the custom port memory.
		/// </summary>
		/// <param name="value">Value to assign</param>
		/// 
		protected void SetPortValue(byte value)
		{
			bus.SetPortValue(value);
		}

		/// <summary>
		/// Maps a RAM bank into the RAM area.
		/// </summary>
		/// <param name="bankIndex">Index of the RAM bank to map</param>
		protected void MapRamBank(int bankIndex)
		{
			bus.MapExternalRamBank(bankIndex);
		}

		/// <summary>
		/// Unmaps RAM from the RAM area.
		/// </summary>
		/// <remarks>
		/// By unmapping the RAM, you ensure that every unhandled read or write to the area will access an useless memory.
		/// Thus, unless you implement your own version of HandleRamWrite, writes will never affect the real RAM.
		/// This can be used by mappers that supports RAM enabling and disabling.
		/// </remarks>
		protected void UnmapRam()
		{
			bus.UnmapExternalRam();
		}

		#endregion

		/// <summary>
		/// Handles a ROM write.
		/// </summary>
		/// <param name="offsetLow">Less significant byte of the write offset</param>
		/// <param name="offsetHigh">Most significant byte of the write offset</param>
		/// <param name="value">Value written</param>
		public abstract void HandleRomWrite(byte offsetLow, byte offsetHigh, byte value);

		/// <summary>
		/// Handles a RAM write.
		/// </summary>
		/// <param name="offsetLow">Less significant byte of the write offset</param>
		/// <param name="offsetHigh">Most significant byte of the write offset</param>
		/// <param name="value">Value written</param>
		/// <remarks>
		/// The default implementation of HandleRamWrite does nothing.
		/// If you impelement your own version, do not bother calling the base implementation (the one in Mapper) because it is useless.
		/// </remarks>
		public virtual void HandleRamWrite(byte offsetLow, byte offsetHigh, byte value)
		{
		}
	}
}
