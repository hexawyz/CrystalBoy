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
	[DesignerCategory("Component")]
	sealed class EmulatedGameBoy : IComponent, IClockManager, IDisposable
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

		public EmulatedGameBoy() : this(null) { }
		public EmulatedGameBoy(IContainer container)
		{
			bus = new GameBoyMemoryBus();
			frameStopwatch = new Stopwatch();
			frameRateStopwatch = new Stopwatch();
#if WITH_THREADING
			bus.EmulationStarted += OnEmulationStarted;	
			bus.EmulationStopped += OnEmulationStopped;
			bus.ClockManager = this;
#else
			Application.Idle += OnApplicationIdle;
#endif
			bus.ReadKeys += new EventHandler<ReadKeysEventArgs>(OnReadKeys);
			emulationStatus = bus.UseBootRom ? EmulationStatus.Paused : EmulationStatus.Stopped;
			if (container != null) container.Add(this);
		}

		#region IComponent Implementation

		public event EventHandler Disposed;

		public ISite Site { get; set; }

		#endregion

		#region IClockManager Implementation

		void IClockManager.Reset()
		{
			lastFrameTime = 0;
			currentFrameTime = 0;

			frameRateStopwatch.Reset();
			frameRateStopwatch.Start();
			frameStopwatch.Reset();
			frameStopwatch.Start();
		}

		void IClockManager.Wait()
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
		}

		#endregion

		private void OnReadKeys(object sender, ReadKeysEventArgs e) { if (e.JoypadIndex == 0) bus.PressedKeys = ReadKeys(); }

#if WITH_THREADING
		private void OnEmulationStarted(object sender, EventArgs e) { EmulationStatus = EmulationStatus.Running; }
		private void OnEmulationStopped(object sender, EventArgs e) { Pause(!IsDisposed && Processor.Status == ProcessorStatus.Running); }
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

		public bool IsDisposed { get { return bus == null; } }

		public void Reset() { Reset(bus.HardwareType); }

		public void Reset(HardwareType hardwareType)
		{
			bus.Reset(hardwareType);
			if (emulationStatus == EmulationStatus.Stopped && bus.UseBootRom) EmulationStatus = EmulationStatus.Paused;
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

		public bool EnableFramerateLimiter
		{
			get { return enableFramerateLimiter; }
#if false
			set { bus.ClockManager = (enableFramerateLimiter = value) ? this : null; }
#else
			set { enableFramerateLimiter = value; }
#endif
		}

		public void Step()
		{
			if (EmulationStatus == EmulationStatus.Paused)
			{
				Bus.Step();
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
#if WITH_THREADING
			bus.Run();
#else
			if (EmulationStatus == EmulationStatus.Paused)
			{
				(this as IClockManager).Reset();
				EmulationStatus = EmulationStatus.Running;
			}
#endif
		}

#if WITH_THREADING
		public void Pause() { if (!IsDisposed) bus.Stop(); }
#else
		public void Pause() { if (emulationStatus == EmulationStatus.Running) Pause(false); }
#endif

		private void Pause(bool breakpoint)
		{
			EmulationStatus = EmulationStatus.Paused;

			if (breakpoint) OnBreak(EventArgs.Empty);
			else OnPause(EventArgs.Empty);
		}

		public GameBoyKeys PressedKeys
		{
			get { return bus.PressedKeys; }
			set { bus.PressedKeys = value; }
		}

		public void NotifyPressedKeys(GameBoyKeys pressedKeys) { bus.Joypads.NotifyPressedKeys(pressedKeys); }

		public void NotifyReleasedKeys(GameBoyKeys releasedKeys) { bus.Joypads.NotifyReleasedKeys(releasedKeys); }

		private void RunFrameInternal() { bus.RunFrame(); }

#if PINVOKE
		private bool IsKeyDown(Keys vKey) { return (NativeMethods.GetAsyncKeyState(vKey) & 0x8000) != 0; }
#endif

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
				(this as IClockManager).Wait();
				RunFrameInternal();
			}
#else
#error Render loop cannot work without P/Invoke
#endif
		}
#endif

		private void OnRomChanged(EventArgs e)
		{
			var handler = RomChanged;

			if (handler != null)
				handler(this, e);
		}

		private void OnPause(EventArgs e)
		{
			var handler = Paused;

			if (handler != null)
				handler(this, e);
		}

		private void OnBreak(EventArgs e)
		{
			var handler = Break;

			if (handler != null)
				handler(this, e);
		}

		private void OnAfterReset(EventArgs e)
		{
			var handler = AfterReset;

			if (handler != null)
				handler(this, e);
		}

		private void OnEmulationStatusChanged(EventArgs e)
		{
			var handler = EmulationStatusChanged;

			if (handler != null)
				handler(this, e);
		}
	}
}
