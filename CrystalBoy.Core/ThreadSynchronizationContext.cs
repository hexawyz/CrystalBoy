using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalBoy.Core
{
	public sealed class ThreadSynchronizationContext : SynchronizationContext, IDisposable
	{
		private struct Message
		{
			public readonly SendOrPostCallback Callback;
			public readonly object State;
			public readonly object SyncRoot;

			public Message(SendOrPostCallback callback, object state)
				: this(callback, state, null)
			{ }

			public Message(SendOrPostCallback callback, object state, object syncRoot)
			{
				Callback = callback;
				State = state;
				SyncRoot = syncRoot;
			}

			public void ExecuteCallback() { Callback(State); }
		}

		private static readonly ConcurrentStack<object> objectPool = new ConcurrentStack<object>();

		private static object GetSynchronizationObjectFromPool()
		{
			object obj;

			return objectPool.TryPop(out obj) ? obj : new object();
		}

		private static void ReturnObjectToPool(object obj)
		{
			objectPool.Push(obj);
		}
		
		private readonly BlockingCollection<Message> queuedMessages = new BlockingCollection<Message>();
		private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		private readonly Thread thread;

		/// <summary>Initializes a new instance of the class <see cref="ThreadSynchronizationContext"/>.</summary>
		/// <param name="action">The async action to initially run on the thread.</param>
		/// <param name="threadName">The name to assign to the thread which will run the synchronization context.</param>
		public ThreadSynchronizationContext(Func<Task> action, string threadName)
		{
			thread = new Thread(Run) { Name = threadName };
			thread.Start(action);
		}

		/// <summary>Disposes of any resources still owned by the class.</summary>
		/// <remarks>This method will block until the underlying thread successfully stopped.</remarks>
		public void Dispose()
		{
			cancellationTokenSource.Cancel();
			thread.Join();
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			queuedMessages.Add(new Message(d, state));
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			if (Thread.CurrentThread == thread) d(state);
			else CrossThreadSend(queuedMessages, d, state);
		}

		private static void CrossThreadSend(BlockingCollection<Message> queuedMessages, SendOrPostCallback d, object state)
		{
			object syncRoot;

			syncRoot = GetSynchronizationObjectFromPool();

			try
			{
				lock (syncRoot)
				{
					queuedMessages.Add(new Message(d, state, syncRoot));
					Monitor.Wait(syncRoot);
				}
			}
			finally { ReturnObjectToPool(syncRoot); }
		}

		private void Run(object state)
		{
			var oldSynchronizationContext = Current;

			SetSynchronizationContext(this);

			try
			{
				((Func<Task>)state)();

				state = null;

				do
				{
					var message = queuedMessages.Take(cancellationTokenSource.Token);

					message.ExecuteCallback();

					// If the message was synchronous, notify the caller that its request has been processed
					if (message.SyncRoot != null)
						lock (message.SyncRoot)
							Monitor.Pulse(message.SyncRoot);
				}
				while (true);
			}
			catch (OperationCanceledException) { }
			//catch (Exception ex) { }
			finally { SetSynchronizationContext(oldSynchronizationContext); }
		}
	}
}
