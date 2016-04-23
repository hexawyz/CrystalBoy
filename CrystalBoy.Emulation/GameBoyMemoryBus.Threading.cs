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
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	partial class GameBoyMemoryBus
	{
		private delegate void NotificationHandler(EventArgs e);

		private const double realFrameDuration = 1000d / 60d;

		private Thread eventsThread;
		private Thread processorThread;
		private Thread audioFrameThread;
		private volatile bool isRunning;
		private volatile IClockManager clockManager;
		
		/// <summary>Notifies that a frame has been executed.</summary>
		public event EventHandler FrameDone;

		/// <summary>Notifies that emulation started.</summary>
		public event EventHandler EmulationStarted;

		/// <summary>Notifies that emulation stopped.</summary>
		public event EventHandler EmulationStopped;

		partial void InitializeThreading()
		{
			clockManager = new GameBoyClockManager();
			processorThread = new Thread(RunProcessor) { IsBackground = true, Name = "GBZ80 Emulation" };
			audioFrameThread = new Thread(RunAudioRenderer) { IsBackground = true };

			processorThread.Start();
			audioFrameThread.Start();
		}

		partial void DisposeThreading()
		{
			Stop();
			DisposeThread(ref processorThread);
			DisposeThread(ref audioFrameThread);
		}

		private void DisposeThread(ref Thread thread)
		{
			if (thread != null)
			{
				lock (thread) Monitor.Pulse(thread);
				thread.Join();

				thread = null;
			}
		}

		/// <summary>Runs the emulation for one frame.</summary>
		public void RunFrame()
		{
			// Prevent dumb deadlocks by checking the value of “isRunning”.
			// However, it is still possible to induce a deadlock in complex threading configurations.
			// In fact, method calls on GameBoyMemoryBus 
			if (!isRunning) lock (processorThread) Monitor.Pulse(processorThread);
		}

		/// <summary>Runs the emulation for one instruction.</summary>
		public void Step()
		{
			if (isRunning) throw new InvalidOperationException();

			lock (processorThread)
				processor.Emulate(false);
		}

		private void OnFrameDone(EventArgs e) => FrameDone?.Invoke(this, e);

		public IClockManager ClockManager
		{
			get { return clockManager; }
			set
			{
				//if (value == null) throw new ArgumentNullException("value");
				SuspendEmulation();
				clockManager = value;
				ResumeEmulation();
			}
		}

		/// <summary>Runs the emulation continuously.</summary>
		public void Run()
		{
			isRunning = true;
			lock (processorThread) Monitor.Pulse(processorThread);
		}

		private void OnEmulationStarted(EventArgs e) => EmulationStarted?.Invoke(this, e);
		private void OnEmulationStopped(EventArgs e) => EmulationStopped?.Invoke(this, e);

		/// <summary>Stops the emulation.</summary>
		public void Stop()
		{
			isRunning = false;
			// Wait for the processor thread to pause…
#pragma warning disable 642
			lock (processorThread) ;
#pragma warning restore 642
		}


		/// <summary>Suspends the emulation.</summary>
		private void SuspendEmulation()
		{
			bool wasRunning = isRunning;

			isRunning = false;
			// Wait for the processor thread to pause, and reset “isRunning”…
			lock (processorThread) isRunning = wasRunning;
		}

		/// <summary>Resumes the emulation.</summary>
		private void ResumeEmulation() { if (isRunning) lock (processorThread) Monitor.Pulse(processorThread); }
		
		private void RunProcessor()
		{
			lock (processorThread)
			{
				while (!isDisposed)
				{
					Monitor.Wait(processorThread);
					
					var clockManager = this.clockManager ?? NullClockManager.Default;

					OnEmulationStarted(EventArgs.Empty);

					clockManager.Reset();

					bool shouldContinueRunning;
					do
					{
						clockManager.Wait();

						// “isRunning” is a volatile variable that can be modified at any time.
						// Reading and writing “isRunning” here is perfectly fine, however, some logic as to be followed.
						// 	- If the processor emulation says to stop running, then we have to stop running, no matter what.
						//	- Because Events may be triggered between the call to “processor.Emulate” and the loop condition evalutaion, altough that is unlikely:
						//    The only value of “isRunning” that matters is the one directly after the call to “processor.Emulate”. (Either the read one or the written one)
						//	- Changes to “isRunning” inside of “processor.Emulate” will be taken into account, for maximum reactivity.
						if (shouldContinueRunning = processor.Emulate(true))
						{
							shouldContinueRunning &= isRunning;
							OnFrameDone(EventArgs.Empty);
						}
						else
						{
							if (processor.Status == ProcessorStatus.Crashed) isRunning = false;
#if WITH_DEBUGGING
							else if (processor.Status == ProcessorStatus.Running)
							{
								isRunning = false;
								OnBreakpoint(EventArgs.Empty);
							}
#endif
							else shouldContinueRunning = true;
						}
					}
					while (shouldContinueRunning);

					OnEmulationStopped(EventArgs.Empty);
				}
			}
		}

		private void OnFrameReady()
		{
			if (sgbPendingTransfer)
			{
				ProcessSuperGameBoyCommand(true);
				if (videoFrameData.SgbBorderChanged)
				{
					sgbCharacterData.CopyTo(videoFrameData.SgbCharacterData, 0);
					sgbBorderMapData.CopyTo(videoFrameData.SgbBorderMapData, 0);
				}
				sgbPendingTransfer = false;
			}

			// For now, keep the audio and video rendering paths separate…
			// It is probably a good idea to merge them, thus avoiding to test threadingEnabled twice…
			
			lock (audioFrameThread)
			{
				// Locking here WILL slow the emulation down… (This might not be a problem for running at 60 fps on modern 1.5GHz+ computers though…)
				// We'll have to wait for the previous frame's audio rendering to be done…
				// By using a real multithreaded sound engine, we could spread the emulation across 3 processors.
				// A good multithreaded sound engine would of course have a slightly different design than the multithreaded video engine:
				// Contrarily to video rendering, we cannot afford to miss a frame for only a few ms of delay…
				// We should however be able to write data to the buffer faster than it can be consumed, as with video data…
				Utility.Swap(ref audioStatusSnapshot, ref savedAudioStatusSnapshot);
				Utility.Swap(ref audioPortAccessList, ref savedAudioPortAccessList);
				Monitor.Pulse(audioFrameThread);
			}

			// Request rendering
			videoFrameData.IsRunningInColorMode = colorMode;
			videoFrameData = videoTripleBufferingSystem.ProducerBufferProvider.SwapBuffers();
		}

		private void RunAudioRenderer()
		{
			lock (audioFrameThread)
			{
				while (!isDisposed)
				{
					Monitor.Wait(audioFrameThread);
					
					RenderAudioFrame();
				}
			}
		}
	}
}
