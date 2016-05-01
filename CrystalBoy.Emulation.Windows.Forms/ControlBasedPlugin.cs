using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrystalBoy.Emulation.Windows.Forms
{
	public abstract class ControlBasedPlugin : IDisposable
	{
		protected Control RenderControl { get; }
		protected SynchronizationContext SynchronizationContext { get; }

		internal ControlBasedPlugin(Control renderControl)
		{
			if (renderControl == null) throw new ArgumentNullException(nameof(renderControl));
			
			RenderControl = renderControl;
			SynchronizationContext = renderControl.InvokeRequired ?
				(SynchronizationContext)renderControl.Invoke((Func<SynchronizationContext>)(() => SynchronizationContext.Current)) :
				SynchronizationContext.Current;
		}

		public virtual void Dispose() { }
	}
}
