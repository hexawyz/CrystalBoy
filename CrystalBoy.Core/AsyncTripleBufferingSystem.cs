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
	public sealed class AsyncTripleBufferingSystem<T> : IDisposable
		where T : class
	{
		/// <summary>Provides buffer from the triple-buffer system for the consumer-side.</summary>
		/// <remarks>
		/// Members of this structure are <c>NOT</c> thread-safe.
		/// For correct operation, the <see cref="SwapBuffersAsync"/> method should only ever be called by one thread at a time.
		/// </remarks>
		public struct ConsumerSideBufferProvider
		{
			private readonly AsyncTripleBufferingSystem<T> owner;

			internal ConsumerSideBufferProvider(AsyncTripleBufferingSystem<T> owner)
			{
				this.owner = owner;
			}

			/// <summary>Swaps the current buffer with the new one when it is ready, and returns it.</summary>
			/// <returns>The next buffer to use, as a consumer.</returns>
			/// <exception cref="OperationCanceledException">The triple-buffering system was canceled.</exception>
			public Task<T> SwapBuffersAsync(CancellationToken cancellationToken)
			{
				return owner.GetConsumerBufferAsync(cancellationToken);
            }

            /// <summary>Summary gets the buffer that is currently available to the consumer side.</summary>
            /// <returns>The current buffer to use, as a consumer.</returns>
            public T GetCurrentBuffer()
            {
                return owner.GetCurrentConsumerBuffer();
            }
		}

		/// <summary>Provides buffer from the triple-buffer system for the producer-side.</summary>
		/// <remarks>
		/// Members of this structure are <c>NOT</c> thread-safe.
		/// For correct operation, the <see cref="SwapBuffers"/> method should only ever be called by one thread at a time.
		/// </remarks>
		public struct ProducerSideBufferProvider
		{
			private readonly AsyncTripleBufferingSystem<T> owner;

			internal ProducerSideBufferProvider(AsyncTripleBufferingSystem<T> owner)
			{
				this.owner = owner;
			}

			/// <summary>Swaps the current buffer with the next one, and returns it.</summary>
			/// <returns>The next buffer to use, as a producer.</returns>
			public T SwapBuffers()
			{
				return owner.GetProducerBuffer();
			}
		}

		private readonly AsyncAutoResetEvent synchronizationEvent;
		private T consumerBuffer;
		private T sharedBuffer;
		private T producerBuffer;

		/// <summary>Initializes a new instance of the <see cref="AsyncTripleBufferingSystem{T}"/> class.</summary>
		/// <param name="bufferInitializer">A delegate which will be called for initializing new buffers.</param>
		public AsyncTripleBufferingSystem(Func<T> bufferInitializer)
		{
			synchronizationEvent = new AsyncAutoResetEvent();
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
			synchronizationEvent.Dispose();
		}

		private void Dispose(ref T item)
		{
			if (item is IDisposable)
			{
				((IDisposable)item).Dispose();
			}
			Volatile.Write(ref item, null);
		}

		private T GetCurrentConsumerBuffer() => Volatile.Read(ref consumerBuffer);

        private async Task<T> GetConsumerBufferAsync(CancellationToken cancellationToken)
		{
			await synchronizationEvent.WaitAsync(cancellationToken).ConfigureAwait(false);

			var newConsumerBuffer = Interlocked.Exchange(ref sharedBuffer, Volatile.Read(ref consumerBuffer));
			Volatile.Write(ref consumerBuffer, newConsumerBuffer);
			return newConsumerBuffer;
		}

		private T GetProducerBuffer()
		{
			// NB: This method is NOT re-entrant, and NOT thread-safe when called by multiple threads at the same time.
			// We could easily make it re-entrant and thread-safe, but there is no reason right now.
			
			// Updates to sharedBuffer need to be atomic.
            var newProducerBuffer = Interlocked.Exchange(ref sharedBuffer, Volatile.Read(ref producerBuffer));
            Volatile.Write(ref producerBuffer, newProducerBuffer);

			synchronizationEvent.Set();

			return newProducerBuffer;
		}

		/// <summary>Gets the buffer provider for the consumer-side of the triple-buffering system.</summary>
		/// <value>The buffer provider for the consumer-side of the triple-buffering system.</value>
		public ConsumerSideBufferProvider ConsumerBufferProvider { get { return new ConsumerSideBufferProvider(this); } }
		/// <summary>Gets the buffer provider for the producer-side of the triple-buffering system.</summary>
		/// <value>The buffer provider for the producer-side of the triple-buffering system.</value>
		public ProducerSideBufferProvider ProducerBufferProvider { get { return new ProducerSideBufferProvider(this); } }
	}
}
