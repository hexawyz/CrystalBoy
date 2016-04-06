using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalBoy.Core
{
	/// <summary>An asynchronous version of <see cref="AutoResetEvent"/>.</summary>
	public sealed class AsyncAutoResetEvent : IDisposable
	{
		private readonly static Task completedTask = Task.FromResult(true);

		private readonly Queue<TaskCompletionSource<bool>> pendingTaskCompletions = new Queue<TaskCompletionSource<bool>>();
		private bool isSignaled;

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public void Dispose()
		{
			isSignaled = false;

			foreach (var tcs in pendingTaskCompletions)
			{
				tcs.TrySetCanceled();
			}
		}

		private void TryCancelWait(object state)
		{
			((TaskCompletionSource<bool>)state).TrySetCanceled();
		}

		private void DisposeCancellationTokenRegistration(Task task, object state)
		{
			try { ((CancellationTokenRegistration)state).Dispose(); }
			catch { }
		}

		/// <summary>Asynchronously waits for the event to be set.</summary>
		/// <returns>A task representing the wait operation.</returns>
		public Task WaitAsync(CancellationToken cancellationToken)
		{
			lock (pendingTaskCompletions)
			{
				if (isSignaled)
				{
					isSignaled = false;
					return completedTask;
				}
				else
				{
					var tcs = new TaskCompletionSource<bool>();
					pendingTaskCompletions.Enqueue(tcs);
					
					// Registers the cancellation and the disposal of the registration.
					tcs.Task.ContinueWith(DisposeCancellationTokenRegistration, cancellationToken.Register(TryCancelWait, tcs, false), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

					return tcs.Task;
				}
			}
		}

		/// <summary>Sets the event.</summary>
		public void Set()
		{
			while (true)
			{
				TaskCompletionSource<bool> pendingTaskCompletion = null;
				lock (pendingTaskCompletions)
				{
					if (pendingTaskCompletions.Count > 0) pendingTaskCompletion = pendingTaskCompletions.Dequeue();
					else if (!isSignaled) isSignaled = true;
				}
				if (pendingTaskCompletion != null)
				{
					// If we can't set the result, we need to ignore the task and restart the method.
					if (!pendingTaskCompletion.TrySetResult(true)) continue;
				}

				break;
			}
		}
	}
}
