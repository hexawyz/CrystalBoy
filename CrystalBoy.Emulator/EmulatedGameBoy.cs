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
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using CrystalBoy.Core;
using CrystalBoy.Emulation;
using System.ComponentModel;

namespace CrystalBoy.Emulator
{
	sealed class EmulatedGameBoy : IComponent, IDisposable
	{
		private GameBoyMemoryBus bus;
		private EmulationStatus emulationStatus;
		private Stopwatch frameStopwatch;
		private Stopwatch frameRateStopwatch;
		private double lastFrameTime;
		private double currentFrameTime;
		private bool enableFramerateLimiter;

		public event EventHandler AfterReset;
		public event EventHandler RomChanged;
		public event EventHandler Paused;
		public event EventHandler Break;
		public event EventHandler EmulationStatusChanged;

		public event EventHandler NewFrame
		{
			add { bus.FrameDone += value; }
			remove { bus.FrameDone -= value; }
		}

		public event EventHandler BorderChanged
		{
			add { bus.BorderChanged += value; }
			remove { bus.BorderChanged -= value; }
		}

		public EmulatedGameBoy()
		{
			bus = new GameBoyMemoryBus();
			frameStopwatch = new Stopwatch();
			frameRateStopwatch = new Stopwatch();
#if WITH_THREADING
			bus.EmulationStopped += OnEmulationStopped;
#else
			Application.Idle += OnApplicationIdle;
#endif
			bus.ReadKeys += new EventHandler<ReadKeysEventArgs>(OnReadKeys);
			emulationStatus = bus.UseBootRom ? EmulationStatus.Paused : EmulationStatus.Stopped;
		}

		#region IComponent Members

		public event EventHandler Disposed;

		public ISite Site { get; set; }

		#endregion

		private void OnReadKeys(object sender, ReadKeysEventArgs e) { if (e.JoypadIndex == 0) bus.PressedKeys = ReadKeys(); }

#if WITH_THREADING
		private void  OnEmulationStopped(object sender, EventArgs e) { Pause(Processor.Status == ProcessorStatus.Running); }
#endif

		public void Dispose()
		{
			if (bus != null)
			{
				bus.Dispose();
				bus = null;
				if (Disposed != null) Disposed(this, EventArgs.Empty);
			}
		}

		public void Reset() { Reset(bus.HardwareType); }

		public void Reset(HardwareType hardwareType)
		{
			bus.Reset(hardwareType);
			if (emulationStatus == EmulationStatus.Stopped && bus.UseBootRom)
				emulationStatus = EmulationStatus.Paused;
			OnAfterReset(EventArgs.Empty);
		}

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
			emulationStatus = bus.UseBootRom ? EmulationStatus.Paused : EmulationStatus.Stopped;
		}

		public bool TryUsingBootRom
		{
			get { return bus.TryUsingBootRom; }
			set { bus.TryUsingBootRom = value; }
		}

		public HardwareType HardwareType { get { return bus.HardwareType; } }

		public RomInformation RomInformation { get { return bus.RomInformation; } }

		public bool RomLoaded { get { return bus.RomLoaded; } }

		public bool HasCustomBorder { get { return bus.HasCustomBorder; } }

		public GameBoyMemoryBus Bus { get { return bus; } }

		public Mapper Mapper { get { return bus.Mapper; } }

		public Processor Processor { get { return bus.Processor; } }

		public MemoryBlock ExternalRam { get { return bus.ExternalRam; } }

		public EmulationStatus EmulationStatus
		{
			get { return emulationStatus; }
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
					return 1000d / (currentFrameTime - lastFrameTime);
				else return 0;
			}
		}

		public int FrameRate
		{
			get
			{
				if (emulationStatus == EmulationStatus.Running)
					return (int)Math.Round(1000 / (currentFrameTime - lastFrameTime), 0);
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
			if (EmulationStatus == EmulationStatus.Paused)
			{
				ResetCounter();
				EmulationStatus = EmulationStatus.Running;
#if WITH_THREADING
				bus.Run();
#endif
			}
		}

		public void Pause()
		{
			if (emulationStatus == EmulationStatus.Running)
				Pause(false);
		}

		private void ResetCounter()
		{
			lastFrameTime = 0;
			currentFrameTime = 0;

			frameRateStopwatch.Reset();
			frameRateStopwatch.Start();
			frameStopwatch.Reset();
			frameStopwatch.Start();
		}

		private void Pause(bool breakpoint)
		{
#if WITH_THREADING
			bus.Stop();
#endif
			EmulationStatus = EmulationStatus.Paused;

			frameRateStopwatch.Stop();

			if (breakpoint) OnBreak(EventArgs.Empty);
			else OnPause(EventArgs.Empty);
		}

		public GameBoyKeys PressedKeys
		{
			get { return bus.PressedKeys; }
			set { bus.PressedKeys = value; }
		}

		//public void NotifyPressedKeys(GameBoyKeys pressedKeys) { bus.NotifyPressedKeys(pressedKeys); }

		//public void NotifyReleasedKeys(GameBoyKeys releasedKeys) { bus.NotifyReleasedKeys(releasedKeys); }

		private void RunFrameInternal() { bus.RunFrame(); }

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

#if !WITH_THREADING
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

				lastFrameTime = currentFrameTime;
				currentFrameTime = frameRateStopwatch.Elapsed.TotalMilliseconds;

				frameStopwatch.Reset();
				frameStopwatch.Start();
				RunFrameInternal();
			}
#else
#error Render loop cannot work without P/Invoke
#endif
		}
#endif

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

		private void OnAfterReset(EventArgs e)
		{
			if (AfterReset != null)
				AfterReset(this, e);
		}

		private void OnEmulationStatusChanged(EventArgs e)
		{
			if (EmulationStatusChanged != null)
				EmulationStatusChanged(this, e);
		}
	}
}
