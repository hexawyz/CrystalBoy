using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CrystalBoy.Emulator
{
	internal abstract class BindableObject : INotifyPropertyChanged
	{
		private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> _eventArgsCache = new ConcurrentDictionary<string, PropertyChangedEventArgs>(StringComparer.Ordinal);

		private static PropertyChangedEventArgs GetEventHandler(string propertyName) => _eventArgsCache.GetOrAdd(propertyName, p => new PropertyChangedEventArgs(p));

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, GetEventHandler(propertyName));
		}

		protected bool InterlockedSetValue(ref double storage, double value, [CallerMemberName] string propertyName = null)
		{
			if (Interlocked.Exchange(ref storage, value) != value)
			{
				NotifyPropertyChanged(propertyName);
				return true;
			}
			return false;
		}

		protected bool SetValue<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{
			if (!EqualityComparer<T>.Default.Equals(value, storage))
			{
				storage = value;
				NotifyPropertyChanged(propertyName);
				return true;
			}
			return false;
		}

		protected bool SetValue<T>(ref T storage, T value, IEqualityComparer<T> equalityComparer, [CallerMemberName] string propertyName = null)
		{
			if (!(equalityComparer ?? EqualityComparer<T>.Default).Equals(value, storage))
			{
				storage = value;
				NotifyPropertyChanged(propertyName);
				return true;
			}
			return false;
		}
	}
}
