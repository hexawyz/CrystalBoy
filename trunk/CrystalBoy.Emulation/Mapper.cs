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

namespace CrystalBoy.Emulation
{
	/// <summary>Represents a rom/ram/whatever mapper for Game Boy games cartidges.</summary>
	public abstract class Mapper
	{
		GameBoyMemoryBus bus;
		MemoryWriteHandler ramWriteInternalHandler, ramWriteHandler;

		unsafe byte* ram;
		bool ramWriteDetected;

		bool interceptsRamWrites;

		public Mapper(GameBoyMemoryBus bus)
		{
			if (bus == null) throw new ArgumentNullException();

			this.bus = bus;
			this.ramWriteInternalHandler = HandleRamWriteInternal;
			this.ramWriteHandler = HandleRamWrite;
		}

		/// <summary>Occurs when the external RAM has been updated.</summary>
		/// <remarks>
		/// Subclasses handling raw writes by themselves must carefully call the <see cref="RamWritten"/> method when needed in order for this event to get triggered.
		/// Other subclasses will get the default automatic detection behavior, which should work fine in most, if not all cases.
		/// </remarks>
		public event EventHandler RamUpdated;

		/// <summary>Resets the Mapper</summary>
		/// <remarks>
		/// The default Reset implementation performs the following operations
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
			InterceptsRamWrites = false;
			UnmapRam();
			SetPortValue(0);
		}

		/// <summary>Gets the GameBoyMemoryBus associated with this Mapper</summary>
		protected GameBoyMemoryBus Bus { get { return bus; } }

		/// <summary>Gets the RAM Size requested by the mapper.</summary>
		/// <remarks>
		/// The default implementation returns the ram size provided by the ROM information.
		/// If need to allocate more RAM for other purposes (memory mapped IO for example), override this propertie to request the proper amount of memory.
		/// </remarks>
		public virtual int RamSize { get { return Bus.RomInformation.RamSize; } }

		/// <summary>Gets the size of the RAM to save.</summary>
		/// <remarks>
		/// The default implementation returns the value returned by RamSize.
		/// If you requested more memory than what need to be saved, if battery is supported, just return the correct amount here.
		/// The first RAM banks will always be saved first, so it is better to put your custom data in the last RAM banks.
		/// </remarks>
		public virtual int SavedRamSize { get { return RamSize; } }

		/// <summary>Gets or sets a value indicating wether the Mapper handles RAM Writes.</summary>
		/// <remarks>
		/// The default Mapper implementation does not handle RAM Writes.
		/// If you set this property to true, it is very likely that you need to override the HandleRamWrite method.
		/// </remarks>
		protected bool InterceptsRamWrites
		{
			get { return interceptsRamWrites; }
			set
			{
				if (value != interceptsRamWrites)
				{
					interceptsRamWrites = value;
					bus.ResetRamWriteHandler();
				}
			}
		}

		internal MemoryWriteHandler RamWriteHandler
		{
			get
			{
				// Determines the RAM write handler to use given the current emulation state.
				// We want to intercept ram writes internally if no particular handling was specifically requested.
				// Also, we don't care about RAM writes if the ROM has no battery (because there is no need to save it somewhere…),
				// or if a RAM write was already detected during this RAM mapping session.
				return interceptsRamWrites ?
					ramWriteHandler :
					bus.ExternalRamBank >= 0 && bus.RomInformation.HasBattery && !ramWriteDetected ?
							ramWriteInternalHandler :
							null;
			}
		}

		/// <summary>Gets a pointer to the allocated RAM.</summary>
		[CLSCompliant(false)]
		protected unsafe byte* Ram { get { return ram; } }

		#region Bank Management Functions

		/// <summary>Maps a ROM bank into the upper or lower ROM area.</summary>
		/// <param name="upper">True for the upper ROM area, false for the lower one.</param>
		/// <param name="bankIndex">Index of the ROM bank to map.</param>
		protected void MapRomBank(bool upper, int bankIndex) { bus.MapExternalRomBank(upper, bankIndex); }

		/// <summary>Maps a custom port into the RAM area.</summary>
		/// <remarks>
		/// The port is handled internally by the GameBoyMemoryBus object associated with the Mapper.
		/// In the current implementation, it uses a 256 byte memory containing only one value, repeated all over the RAM area.
		/// If you wish to change the value assigned to this memory, you can do so by using the SetPortValue method.
		/// </remarks>
		protected void MapPort() { bus.MapExternalPort(); }

		/// <summary>Sets the value assigned to the custom port memory.</summary>
		/// <param name="value">Value to assign</param>
		protected void SetPortValue(byte value) { bus.SetPortValue(value); }

		/// <summary>Maps a RAM bank into the RAM area.</summary>
		/// <param name="bankIndex">Index of the RAM bank to map</param>
		protected void MapRamBank(int bankIndex)
		{
			// We do not access the RAM write detection flag here…
			// And this should stay like this if possible.
			// (Write detection need only to happen once. Thus, the detection function will only ever be called once per ram mapping/unmapping session)
			bus.MapExternalRamBank(bankIndex);
			bus.ResetRamWriteHandler();
		}

		/// <summary>Unmaps RAM from the RAM area.</summary>
		/// <remarks>
		/// By unmapping the RAM, you ensure that every unhandled read or write to the area will access an useless memory.
		/// Thus, unless you implement your own version of HandleRamWrite, writes will never affect the real RAM.
		/// This can be used by mappers that supports RAM enabling and disabling.
		/// </remarks>
		protected void UnmapRam()
		{
			bus.UnmapExternalRam();
			if (ramWriteDetected && RamUpdated != null)
				RamUpdated(this, EventArgs.Empty);
			ramWriteDetected = false;
			bus.ResetRamWriteHandler();
		}

		#endregion

		/// <summary>Handles a ROM write.</summary>
		/// <param name="offsetLow">Less significant byte of the write offset</param>
		/// <param name="offsetHigh">Most significant byte of the write offset</param>
		/// <param name="value">Value written</param>
		public abstract void HandleRomWrite(byte offsetLow, byte offsetHigh, byte value);

		/// <summary>Notifies of a write to external RAM.</summary>
		/// <remarks>
		/// Use this method while implementing RAM writes in a non-standard fashion.
		/// By default, ram writes will be detected by the base Mapper implementation.
		/// Detecting RAM writes allows for on-demand battery file writes.
		/// The RamUpdated event will be triggered when unmapping the RAM, only if (real) ram writes were detected.
		/// </remarks>
		protected void RamWritten() { ramWriteDetected = true; }

		/// <summary>Intercepts all RAM writes.</summary>
		/// <param name="offsetLow">Less significant byte of the write offset</param>
		/// <param name="offsetHigh">Most significant byte of the write offset</param>
		/// <param name="value">Value written</param>
		/// <remarks>This method will be used only for detecting ram updates transparently.</remarks>
		private void HandleRamWriteInternal(byte offsetLow, byte offsetHigh, byte value)
		{
			ramWriteDetected = true;
			// Process as if the method was never there…
			bus.RamWritePassthrough(offsetLow, offsetHigh, value);
			// Once a RAM write is detected, this method is no longer useful, so we can reset the write handler to a more performant state (null)
			bus.ResetRamWriteHandler();
		}

		/// <summary>Handles a RAM write.</summary>
		/// <param name="offsetLow">Less significant byte of the write offset</param>
		/// <param name="offsetHigh">Most significant byte of the write offset</param>
		/// <param name="value">Value written</param>
		/// <remarks>
		/// The default implementation of HandleRamWrite does nothing.
		/// If you impelement your own version, do not bother calling the base implementation (the one in Mapper) because it is useless.
		/// However, please call the <see cref="RamWriten"/> method if you write directly to the external RAM.
		/// </remarks>
		public virtual void HandleRamWrite(byte offsetLow, byte offsetHigh, byte value) { }
	}
}
