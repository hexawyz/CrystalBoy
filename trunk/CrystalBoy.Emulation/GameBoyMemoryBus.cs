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
	public sealed partial class GameBoyMemoryBus : IDisposable
	{
		#region Variables

		private HardwareType hardwareType;
		private bool colorHardware;
		private bool colorMode;
		private bool useBootRom;

		#endregion

		#region Constructors

		public GameBoyMemoryBus()
		{
			Initialize();
		}

		public GameBoyMemoryBus(MemoryBlock externalRom)
		{
			try
			{
				Initialize();
				LoadRom(externalRom);
			}
			catch
			{
				Dispose();
			}
		}

		#endregion

		#region Destructors

		~GameBoyMemoryBus()
		{
			Dispose(false);
		}

		#region Dispose

		// Define a Dispose function for each module
		partial void DisposeProcessor();
		partial void DisposeTiming();
		partial void DisposeTimer();
		partial void DisposeInterrupt();
		partial void DisposeMemory();
		partial void DisposeBootRom();
		partial void DisposeRom();
		partial void DisposeVideo();
		partial void DisposeJoypad();
		partial void DisposePorts();
		partial void DisposeRendering();
		partial void DisposeDebug();
		partial void DisposeSnapshot();

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				GC.SuppressFinalize(this);
				DisposeProcessor();
				DisposeTiming();
				DisposeTimer();
				DisposeInterrupt();
				DisposeMemory();
				DisposeBootRom();
				DisposeRom();
				DisposeVideo();
				DisposeJoypad();
				DisposePorts();
				DisposeRendering();
				DisposeDebug();
				DisposeSnapshot();
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

		private void Initialize()
		{
			HardwareType = HardwareType.GameBoyColor;

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
		}

		#endregion

		#region Reset

		// Define a Reset function for each module
		partial void ResetProcessor();
		partial void ResetTiming();
		partial void ResetTimer();
		partial void ResetInterrupt();
		partial void ResetMemory();
		partial void ResetBootRom();
		partial void ResetRom();
		partial void ResetVideo();
		partial void ResetJoypad();
		partial void ResetPorts();
		partial void ResetRendering();
		partial void ResetDebug();
		partial void ResetSnapshot();

		// The main reset function calls all module reset functions
		public void Reset() { Reset(hardwareType, true); }
		public void Reset(HardwareType hardwareType) { Reset(hardwareType, true); }
		public void Reset(bool useBootRom) { Reset(hardwareType, useBootRom); }
		public void Reset(HardwareType hardwareType, bool useBootRom)
		{
			// From now on, the hardware type can only be changed after a "hard" reset of the emulated machine
			HardwareType = hardwareType;

			// Determine whether to use the boot ROM.
			// We only choose to use the boot ROM if it has been loaded for the corresponding hardware…
			if (useBootRom)
				switch (HardwareType)
				{
					case HardwareType.GameBoy:
						this.useBootRom = dmgBootRomLoaded;
						break;
					case HardwareType.SuperGameBoy:
						this.useBootRom = sgbBootRomLoaded;
						break;
					case HardwareType.GameBoyColor:
						if (this.useBootRom = cgbBootRomLoaded)
							this.colorMode = true; // Always start in color mode if emulating a GBC with bootstrap ROM
						break;
					default:
						this.useBootRom = false;
						break;
				}
			else this.useBootRom = false;
			
			ResetProcessor();
			ResetTiming();
			ResetTimer();
			ResetInterrupt();
			ResetMemory();
			ResetBootRom();
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

		public bool ColorHardware { get { return colorHardware; } }

		public bool ColorMode { get { return colorMode; } }

		public bool UseBootRom { get { return useBootRom; } }

		#endregion
	}
}
