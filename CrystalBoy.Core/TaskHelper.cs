using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrystalBoy.Core
{
	public static class TaskHelper
	{
		public static readonly Task<bool> TrueTask = Task.FromResult(true);
		public static readonly Task<bool> FalseTask = Task.FromResult(false);
		public static readonly Task CanceledTask = CreateCanceledTask();

		private static Task CreateCanceledTask()
		{
			var tcs = new TaskCompletionSource<bool>();

			tcs.SetCanceled();

			return tcs.Task;
		}
	}
}
