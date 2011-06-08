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
using System.Diagnostics;
using System.Windows.Forms;
using CrystalBoy.Core;
using CrystalBoy.Emulation;

namespace CrystalBoy.Emulator
{
	sealed class EmulatedGameBoy : IDisposable
	{
		GameBoyMemoryBus bus;
		EmulationStatus emulationStatus;
		Stopwatch frameStopwatch;
		Stopwatch frameRateStopwatch;
		long[] tickCounts;
		int tickIndex;
		bool enableFramerateLimiter;

		public event EventHandler RomChanged;
		public event EventHandler Paused;
		public event EventHandler Break;
		public event EventHandler EmulationStatusChanged;
		public event EventHandler NewFrame;

		public EmulatedGameBoy()
		{
			bus = new GameBoyMemoryBus();
			tickCounts = new long[60];
			frameStopwatch = new Stopwatch();
			frameRateStopwatch = new Stopwatch();
			Application.Idle += OnApplicationIdle;
		}

		public void Dispose() { bus.Dispose(); }

		public void Reset() { bus.Reset(); }

		public void Reset(HardwareType hardwareType) { bus.Reset(hardwareType); }

		public void LoadRom(MemoryBlock rom)
		{
			emulationStatus = EmulationStatus.Stopped;
			bus.LoadRom(rom);
			emulationStatus = EmulationStatus.Paused;
			OnRomChanged(EventArgs.Empty);
		}

		public void UnloadRom()
		{
			emulationStatus = EmulationStatus.Stopped;
			bus.UnloadRom();
		}

		public HardwareType HardwareType { get { return bus.HardwareType; } }

		public RomInformation RomInformation { get { return bus.RomInformation; } }

		public bool RomLoaded { get { return bus.RomLoaded; } }

		public GameBoyMemoryBus Bus { get { return bus; } }

		public Mapper Mapper { get { return bus.Mapper; } }

		public Processor Processor { get { return bus.Processor; } }

		public MemoryBlock ExternalRam { get { return bus.ExternalRam; } }

		public EmulationStatus EmulationStatus
		{
			get
			{
				return emulationStatus;
			}
			private set
			{
				if (value != emulationStatus)
				{
					emulationStatus = value;
					OnEmulationStatusChanged(EventArgs.Empty);
				}
			}
		}

		public double PreciseFrameRate
		{
			get
			{
				if (emulationStatus == EmulationStatus.Running)
				{
					long delta = tickIndex == 0 ?
						 tickCounts[tickCounts.Length - 1] - tickCounts[0] :
						 tickCounts[tickIndex - 1] - tickCounts[tickIndex];

					return delta > 0 ? (double)(1000 * tickCounts.Length) / delta : 0;
				}
				else return 0;
			}
		}

		public int FrameRate
		{
			get
			{
				if (emulationStatus == EmulationStatus.Running)
				{
					long delta = tickIndex == 0 ?
						tickCounts[tickCounts.Length - 1] - tickCounts[0] :
						tickCounts[tickIndex - 1] - tickCounts[tickIndex];

					return delta > 0 ? (int)(1000 * tickCounts.Length / delta) : 0;
				}
				else return 0;
			}
		}

		public bool EnableFramerateLimiter { get { return enableFramerateLimiter; } set { enableFramerateLimiter = value; } }

		public void Step()
		{
			if (EmulationStatus == EmulationStatus.Paused)
			{
				Processor.Emulate(false);
				OnBreak(EventArgs.Empty);
			}
		}

		public void RunFrame()
		{
			if (EmulationStatus == EmulationStatus.Paused)
				RunFrameInternal();
		}

		public void Run()
		{
			if (EmulationStatus != EmulationStatus.Running)
			{
				ResetCounter();
				EmulationStatus = EmulationStatus.Running;
			}
		}

		public void Pause()
		{
			if (emulationStatus == EmulationStatus.Running)
				Pause(false);
		}

		private void ResetCounter()
		{
			for (int i = 0; i < tickCounts.Length; i++)
				tickCounts[i] = 0;

			tickIndex = 0;

			frameRateStopwatch.Reset();
			frameRateStopwatch.Start();
			frameStopwatch.Reset();
			frameStopwatch.Start();
		}

		private void Pause(bool breakpoint)
		{
			EmulationStatus = EmulationStatus.Paused;

			frameRateStopwatch.Stop();

			if (breakpoint)
				OnBreak(EventArgs.Empty);
			else
				OnPause(EventArgs.Empty);
		}

		public GameBoyKeys PressedKeys
		{
			get { return bus.PressedKeys; }
			set { bus.PressedKeys = value; }
		}

		public void NotifyPressedKeys(GameBoyKeys pressedKeys) { bus.NotifyPressedKeys(pressedKeys); }

		public void NotifyReleasedKeys(GameBoyKeys releasedKeys) { bus.NotifyReleasedKeys(releasedKeys); }

		private void RunFrameInternal()
		{
			bus.PressedKeys = ReadKeys();
			if (Processor.Emulate(true)) OnNewFrame(EventArgs.Empty);
			else Pause(true);
		}

		private bool IsKeyDown(Keys vKey) { return (NativeMethods.GetAsyncKeyState(vKey) & 0x8000) != 0; }

		private GameBoyKeys ReadKeys()
		{
#if PINVOKE
			GameBoyKeys keys = GameBoyKeys.None;

			if (IsKeyDown(Keys.Right))
				keys |= GameBoyKeys.Right;
			if (IsKeyDown(Keys.Left))
				keys |= GameBoyKeys.Left;
			if (IsKeyDown(Keys.Up))
				keys |= GameBoyKeys.Up;
			if (IsKeyDown(Keys.Down))
				keys |= GameBoyKeys.Down;
			if (IsKeyDown(Keys.X))
				keys |= GameBoyKeys.A;
			if (IsKeyDown(Keys.Z))
				keys |= GameBoyKeys.B;
			if (IsKeyDown(Keys.RShiftKey))
				keys |= GameBoyKeys.Select;
			if (IsKeyDown(Keys.Return))
				keys |= GameBoyKeys.Start;

			return keys;
#endif
		}

		private void OnApplicationIdle(object sender, EventArgs e)
		{
#if PINVOKE
			NativeMethods.Message msg;

			while (emulationStatus == EmulationStatus.Running &&
				!NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0))
			{
				if (enableFramerateLimiter)
				{
					long timer = frameStopwatch.ElapsedMilliseconds;

					if (timer < 17) // Exact timing for one frame at 60fps is 16⅔ ms
					{
						if (timer < 16)
						{
							// Conversion from long to int is safe since the value is less than 17.
							// Sleep is a really bad tool for precise timing, but it will play its role when needed.
							System.Threading.Thread.Sleep(16 - (int)timer);
						}

						// Do some active wait, even though this is bad…
						while (frameStopwatch.Elapsed.TotalMilliseconds < (1000d / 60d)) ;
					}
				}
				
				tickCounts[tickIndex++] = frameRateStopwatch.ElapsedMilliseconds;
				if (tickIndex >= tickCounts.Length) tickIndex = 0;

				frameStopwatch.Reset();
				frameStopwatch.Start();
				RunFrameInternal();
			}
#else
#error Render loop cannot work without P/Invoke
#endif
		}

		private void OnRomChanged(EventArgs e)
		{
			if (RomChanged != null)
				RomChanged(this, e);
		}

		private void OnPause(EventArgs e)
		{
			if (Paused != null)
				Paused(this, e);
		}

		private void OnBreak(EventArgs e)
		{
			if (Break != null)
				Break(this, e);
		}

		private void OnEmulationStatusChanged(EventArgs e)
		{
			if (EmulationStatusChanged != null)
				EmulationStatusChanged(this, e);
		}

		private void OnNewFrame(EventArgs e)
		{
			if (NewFrame != null)
				NewFrame(this, e);
		}
	}
}
