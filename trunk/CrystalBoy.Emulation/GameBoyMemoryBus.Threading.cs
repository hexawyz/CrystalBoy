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

namespace CrystalBoy.Emulation
{
	partial class GameBoyMemoryBus
	{
		private RunFrameResult runFrameResult;
#if WITH_THREADING
		private Thread processorThread;
		private Thread videoFrameThread;
		private Thread audioFrameThread;
		private volatile bool isRendering;

		private bool threadingEnabled;
#endif

#if WITH_THREADING
		partial void InitializeThreading()
		{
			if (threadingEnabled = !(Environment.ProcessorCount < 2))
			{
				processorThread = new Thread(RunProcessor);
				videoFrameThread = new Thread(RunVideoFrame);
				//audioFrameThread = new Thread(RunAudioFrame);

				processorThread.Start();
				videoFrameThread.Start();
			}
			runFrameResult = new RunFrameResult();
		}

		partial void DisposeThreading()
		{
			threadingEnabled = false;
			DisposeThread(ref processorThread);
			DisposeThread(ref videoFrameThread);
			DisposeThread(ref audioFrameThread);
		}

		private void DisposeThread(ref Thread thread)
		{
			if (thread != null)
			{
				lock (thread)
					Monitor.Pulse(thread);
				thread.Join();

				thread = null;
			}
		}
#endif

		public RunFrameResult RunFrame()
		{
#if WITH_THREADING
			if (threadingEnabled)
			{
				lock (processorThread)
				{
					runFrameResult.Finished = false;
					runFrameResult.Value = false;
					Monitor.Pulse(processorThread);
				}
			}
			else
#endif
			{
				runFrameResult.Value = processor.Emulate(true);
				runFrameResult.Finished = true;
			}

			return runFrameResult;
		}

#if WITH_THREADING
		private void ThreadedRender()
		{
			// This method will (should) never be called when a rendering operation is already active.
			// The code below should thus never lock the thread for a long time.
			lock (videoFrameThread)
				Monitor.Pulse(videoFrameThread);
		}

		private void RunProcessor()
		{
			lock (processorThread)
			{
				while (true)
				{
					Monitor.Wait(processorThread);

					if (!threadingEnabled) return;

					lock (runFrameResult)
					{
						runFrameResult.Value = processor.Emulate(true);
						runFrameResult.Finished = true;
						Monitor.Pulse(runFrameResult);
					}
				}
			}
		}

		private void RunVideoFrame()
		{
			lock (videoFrameThread)
			{
				while (true)
				{
					Monitor.Wait(videoFrameThread);

					if (!threadingEnabled) return;

					// The method will clear the isRendering field by itself
					Render();

					OnAfterRendering(EventArgs.Empty);
				}
			}
		}

		private void RunAudioFrame()
		{
			lock (audioFrameThread)
			{
				while (true)
				{
					Monitor.Wait(audioFrameThread);

					if (!threadingEnabled) return;
				}
			}
		}

		public bool ThreadingEnabled { get { return threadingEnabled; } }
#else
		public bool ThreadingEnabled { get { return false; } }
#endif
	}
}
