using CrystalBoy.Emulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace CrystalBoy.Emulation.Windows.Forms
{
	public abstract class ControlVideoRenderer : IVideoRenderer
	{
		protected Control RenderControl { get; }
		protected SynchronizationContext SynchronizationContext { get; }

		public ControlVideoRenderer(Control renderControl)
		{
			if (renderControl == null) throw new ArgumentNullException(nameof(renderControl));

			RenderControl = renderControl;
			SynchronizationContext = renderControl.InvokeRequired ?
				(SynchronizationContext)renderControl.Invoke((Func<SynchronizationContext>)(() => SynchronizationContext.Current)) :
				SynchronizationContext.Current;
		}

		public virtual void Dispose()
		{
		}

		public abstract void Refresh();

		public abstract Task RenderBorderAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken);

		public abstract Task RenderFrameAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken);
	}
}
