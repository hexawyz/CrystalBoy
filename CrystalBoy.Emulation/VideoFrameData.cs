using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalBoy.Emulation
{
	/// <summary>Instances of this type contain data useful to render a single video frame.</summary>
	public sealed class VideoFrameData
	{
		private static int InstanceCounter;

		private readonly int InstanceIndex = Interlocked.Increment(ref InstanceCounter);

		/// <summary>Gets the snapshot of the video memory.</summary>
		internal VideoMemorySnapshot VideoMemorySnapshot { get; }
		/// <summary>Gets the list of recorded video port accesses.</summary>
		internal List<PortAccess> VideoPortAccessList { get; }
		/// <summary>Gets the list of recorded palette accesses, for CGB mode only.</summary>
		internal List<PaletteAccess> PaletteAccessList { get; }
		/// <summary>Gets or sets a value indicating whether the emulation is running in CGB color mode.</summary>
		/// <value><see langword="true" /> if the emulation is running in CGB color mode; otherwise, <see langword="false" />.</value>
		internal bool IsRunningInColorMode { get; set; }
		/// <summary>Gets or sets a value indicating whether the grey palette for B&amp;W games running in CGB mode has been updated.</summary>
		/// <value><see langword="true" /> if the grey palette for B&amp;W games running in CGB mode has been updated; otherwise, <see langword="false" />.</value>
		internal bool GreyPaletteUpdated { get; set; }

		internal VideoFrameData(GameBoyMemoryBus bus)
		{
			VideoMemorySnapshot = new VideoMemorySnapshot(bus);
			VideoPortAccessList = new List<PortAccess>(1000);
			PaletteAccessList = new List<PaletteAccess>(1000);
		}
	}
}
