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
using System.Collections.Generic;
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	/// <summary>Represents the memory bus of a Game Boy system, with all its attached devices.</summary>
	public sealed partial class GameBoyMemoryBus : IDisposable
	{
		#region Variables

		private HardwareType hardwareType;
		private bool colorHardware;
		private bool colorMode;
		private bool disposed;

		#endregion

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="GameBoyMemoryBus"/> class.</summary>
		public GameBoyMemoryBus()
		{
			Initialize();
		}

		/// <summary>Initializes a new instance of the <see cref="GameBoyMemoryBus"/> class, preloaded with an external ROM.</summary>
		/// <param name="externalRom">The external ROM to load.</param>
		public GameBoyMemoryBus(MemoryBlock externalRom)
		{
			try
			{
				Initialize();
				LoadRom(externalRom);
			}
			catch { Dispose(); }
		}

		#endregion

		#region Destructors

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="GameBoyMemoryBus"/> is reclaimed by garbage collection.
		/// </summary>
		~GameBoyMemoryBus() { Dispose(false); }

		#region Dispose

		// Define a Dispose function for each module
		partial void DisposeBootRom();
		partial void DisposeProcessor();
		partial void DisposeTiming();
		partial void DisposeTimer();
		partial void DisposeInterrupt();
		partial void DisposeMemory();
		partial void DisposeRom();
		partial void DisposeVideo();
		partial void DisposeJoypad();
		partial void DisposePorts();
		partial void DisposeRendering();
		partial void DisposeDebug();
		partial void DisposeSnapshot();

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public void Dispose() { Dispose(true); }

		/// <summary>Releases unmanaged and - optionally - managed resources.</summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (disposing && !disposed)
			{
				disposed = true;
				DisposeBootRom();
				DisposeProcessor();
				DisposeTiming();
				DisposeTimer();
				DisposeInterrupt();
				DisposeMemory();
				DisposeRom();
				DisposeVideo();
				DisposeJoypad();
				DisposePorts();
				DisposeRendering();
				DisposeDebug();
				DisposeSnapshot();
				GC.SuppressFinalize(this);
			}
		}

		#endregion

		#endregion

		#region Initialize

		// Define an Initialize function for each module
		partial void InitializeProcessor();
		partial void InitializeTiming();
		partial void InitializeTimer();
		partial void InitializeInterrupt();
		partial void InitializeMemory();
		partial void InitializeBootRom();
		partial void InitializeRom();
		partial void InitializeVideo();
		partial void InitializeJoypad();
		partial void InitializePorts();
		partial void InitializeRendering();
		partial void InitializeDebug();
		partial void InitializeSnapshot();

		/// <summary>Initializes the emulated system.</summary>
		/// <remarks>This will call the initialization methods for all modules.</remarks>
		private void Initialize()
		{
			HardwareType = HardwareType.GameBoyColor;
			tryUsingBootRom = true;

			InitializeProcessor();
			InitializeTiming();
			InitializeTimer();
			InitializeInterrupt();
			InitializeMemory();
			InitializeBootRom();
			InitializeRom();
			InitializeVideo();
			InitializeJoypad();
			InitializePorts();
			InitializeRendering();
			InitializeDebug();
			InitializeSnapshot();

			Reset();
		}

		#endregion

		#region Reset

		// Define a Reset function for each module
		partial void ResetBootRom();
		partial void ResetProcessor();
		partial void ResetTiming();
		partial void ResetTimer();
		partial void ResetInterrupt();
		partial void ResetMemory();
		partial void ResetRom();
		partial void ResetVideo();
		partial void ResetJoypad();
		partial void ResetPorts();
		partial void ResetRendering();
		partial void ResetDebug();
		partial void ResetSnapshot();

		// The main reset function calls all module reset functions

		/// <summary>Resets the emulated system.</summary>
		public void Reset() { Reset(hardwareType); }

		/// <summary>Resets the emulated system and emulate a specific hardware.</summary>
		/// <remarks>In order to chaneg the emulated hardware, the emulation has to be reset.</remarks>
		/// <param name="hardwareType">Hardware to emulate.</param>
		public void Reset(HardwareType hardwareType)
		{
			// From now on, the hardware type can only be changed after a "hard" reset of the emulated machine
			HardwareType = hardwareType;

			ResetBootRom();
			ResetProcessor();
			ResetTiming();
			ResetTimer();
			ResetInterrupt();
			ResetMemory();
			ResetRom();
			ResetVideo();
			ResetJoypad();
			ResetPorts();
			ResetRendering();
			ResetDebug();
			ResetSnapshot();
		}

		#endregion

		#region Properties

		/// <summary>Gets the emulated hardware.</summary>
		/// <value>The emulated hardware.</value>
		public HardwareType HardwareType
		{
			get { return hardwareType; }
			private set
			{
				// We don't really care about the exact hardware type here, as long as it's a value defined in the code.
				if (!Enum.IsDefined(typeof(HardwareType), hardwareType))
					throw new ArgumentOutOfRangeException("value");

				// Once the value has been checked, we can safely update the internal information
				this.hardwareType = value;
				// This information should be more convenient provided as a flag
				this.colorHardware = value >= HardwareType.GameBoyColor;
				// Don't forget to reset the color mode flag too…
				this.colorMode = ColorHardware && RomInformation != null ? RomInformation.ColorGameBoySupport : false;
			}
		}

		/// <summary>Gets a value indicating whether the emulated hardware supports Game Boy Color functions.</summary>
		/// <remarks>
		/// The only harware supporting Game Boy Color functions are:
		/// <list type="bullet">
		/// <item><description>Game Boy Color</description></item>
		/// <item><description>Game Boy Advance</description></item>
		/// </list>
		/// </remarks>
		/// <value><c>true</c> if the emulated harware supports color functions; otherwise, <c>false</c>.</value>
		public bool ColorHardware { get { return colorHardware; } }

		/// <summary>Gets a value indicating whether the emulated system is running in color mode.</summary>
		/// <remarks>
		/// When using the Game Boy Color bootstrap ROM, the emulation always starts in color mode.
		/// However, the bootstrap ROM will switch the system to a pseudo-black-and-white mode if the inserted game doesn't support Game Boy Color functions.
		/// </remarks>
		/// <value><c>true</c> if the emulated system is running in color mode; otherwise, <c>false</c>.</value>
		public bool ColorMode { get { return colorMode; } }

		#endregion
	}
}
