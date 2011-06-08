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
using System.IO;
using System.Collections.Generic;
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	public sealed partial class GameBoyMemoryBus : IDisposable
	{
		#region Variables

		HardwareType hardwareType;
		bool colorHardware;
		bool colorMode;

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
		partial void DisposeRom();
		partial void DisposeVideo();
		partial void DisposeJoypad();
		partial void DisposePorts();
		partial void DisposeRendering();
		partial void DisposeDebug();

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
				DisposeRom();
				DisposeVideo();
				DisposeJoypad();
				DisposePorts();
				DisposeRendering();
				DisposeDebug();
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
		partial void InitializeRom();
		partial void InitializeVideo();
		partial void InitializeJoypad();
		partial void InitializePorts();
		partial void InitializeRendering();
		partial void InitializeDebug();

		private void Initialize()
		{
			HardwareType = HardwareType.GameBoyColor;

			InitializeProcessor();
			InitializeTiming();
			InitializeTimer();
			InitializeInterrupt();
			InitializeMemory();
			InitializeRom();
			InitializeVideo();
			InitializeJoypad();
			InitializePorts();
			InitializeRendering();
			InitializeDebug();
		}

		#endregion

		#region Reset

		// Define a Reset function for each module
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

		// The main reset function calls all module reset functions
		public void Reset() { Reset(hardwareType); }

		// The main reset function calls all module reset functions
		public void Reset(HardwareType hardwareType)
		{
			// From now on, the hardware type can only be chanegd after a "hard" reset of the emulated machine
			HardwareType = hardwareType;
			
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

		#endregion
	}
}
