using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalBoy.Core
{
	/// <summary>Abstracts a triple-buffering engine.</summary>
	/// <remarks>All members of this class are thread-safe.</remarks>
	/// <typeparam name="T"></typeparam>
	public sealed class TripleBufferingSystem<T> : IDisposable
		where T : class
	{
		private readonly object syncRoot = new object();
		private T consumerBuffer;
		private T sharedBuffer;
		private T producerBuffer;
		private bool isNewConsumerBufferAvailable;

		/// <summary>Initializes a new instance of the <see cref="AsyncTripleBufferingSystem{T}"/> class.</summary>
		/// <param name="bufferInitializer">A delegate which will be called for initializing new buffers.</param>
		public TripleBufferingSystem(Func<T> bufferInitializer)
		{
			consumerBuffer = bufferInitializer();
			sharedBuffer = bufferInitializer();
			producerBuffer = bufferInitializer();
		}

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public void Dispose()
		{
			Dispose(ref consumerBuffer);
			Dispose(ref sharedBuffer);
			Dispose(ref producerBuffer);
		}

		private void Dispose(ref T buffer)
		{
			if (buffer is IDisposable)
			{
				((IDisposable)buffer).Dispose();
			}
		}

		/// <summary>Gets the current valid consumer buffer.</summary>
		/// <returns>The current valid consumer buffer.</returns>
		public T GetCurrentConsumerBuffer() => Volatile.Read(ref consumerBuffer);

		/// <summary>Gets the current valid producer buffer.</summary>
		/// <returns>The current valid producer buffer.</returns>
		public T GetCurrentProducerBuffer() => Volatile.Read(ref producerBuffer);

		/// <summary>Swaps the specified buffer with the shared buffer.</summary>
		/// <remarks>This method MUST be called from inside the common lock represented by <see cref="syncRoot"/>.</remarks>
		/// <param name="buffer">The buffer to swap with the shared buffer.</param>
		/// <returns>The previous shared buffer.</returns>
		private T SwapWithSharedBuffer(ref T buffer)
		{
			var sharedBuffer = Volatile.Read(ref this.sharedBuffer);
			var otherBuffer = Interlocked.Exchange(ref buffer, sharedBuffer);
			Volatile.Write(ref this.sharedBuffer, otherBuffer);
			return sharedBuffer;
		}

		/// <summary>Tries to swap buffers, and gets the current valid consumer buffer.</summary>
		/// <param name="buffer">This will be set to a valid consumer buffer.</param>
		/// <returns>true if the buffers were swapped; otherwise false.</returns>
		public bool TryGetNextConsumerBuffer(out T buffer)
		{
			bool success;

			lock (syncRoot)
			{
				if (success = isNewConsumerBufferAvailable)
				{
					isNewConsumerBufferAvailable = false;
					buffer = SwapWithSharedBuffer(ref consumerBuffer);
				}
				else
				{
					buffer = GetCurrentConsumerBuffer();
				}
			}

			return success;
		}

		/// <summary>Gets a ready to use producer buffer.</summary>
		/// <returns>A ready to use buffer.</returns>
		public T GetNextProducerBuffer()
		{
			lock (syncRoot)
			{
				isNewConsumerBufferAvailable = true;
				return SwapWithSharedBuffer(ref producerBuffer);
			}
		}
	}
}
