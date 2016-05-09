using CrystalBoy.Core;
using CrystalBoy.Emulation;
using CrystalBoy.Emulator.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalBoy.Emulator.ViewModels
{
	internal sealed class ApplicationViewModel : BindableObject, IDisposable
	{
		public class ZoomLevelViewModel : BindableObject
		{
			private ApplicationViewModel _applicationViewModel;

			public double Value { get; }

			public bool IsSelected => Value == _applicationViewModel._currentZoomLevel;

			internal ZoomLevelViewModel(ApplicationViewModel applicationViewModel, double value)
			{
				_applicationViewModel = applicationViewModel;
				Value = value;
			}

			internal void NotifyIsSelectedChanged() => NotifyPropertyChanged(nameof(IsSelected));
		}

		private static readonly long RelativeSpeedUpdateTicks = Stopwatch.Frequency / 10;

		private EmulatedGameBoy EmulatedGameBoy { get; }
		private IWindowManager WindowService { get; }
		private IFileDialogService FileDialogService { get; }

		private readonly ReadOnlyCollection<ZoomLevelViewModel> _zoomLevels;
		private double _currentZoomLevel;
		private readonly Stopwatch _relativeSpeedUpdateStopwatch;
		private double _relativeSpeed;
		private IWindow _romInformationWindow;

		public ApplicationViewModel(IWindowManager windowService, IFileDialogService fileDialogService)
		{
			_zoomLevels = Array.AsReadOnly(Enumerable.Range(1, 8).Select(n => new ZoomLevelViewModel(this, n)).ToArray());
			WindowService = windowService;
			FileDialogService = fileDialogService;
			EmulatedGameBoy = new EmulatedGameBoy
			{
				EnableFramerateLimiter = true
			};

			_relativeSpeedUpdateStopwatch = Stopwatch.StartNew();

			EmulatedGameBoy.NewFrame += OnNewFrame;
		}

		private void OnNewFrame(object sender, EventArgs e)
		{
			if (_relativeSpeedUpdateStopwatch.ElapsedTicks > RelativeSpeedUpdateTicks)
			{
				_relativeSpeedUpdateStopwatch.Restart();

				// We can just assign the property from the current thread here, and WPF will do the required marshalling.
				RelativeSpeed = EmulatedGameBoy.EmulatedSpeed;
			}
		}

		public void Dispose()
		{
			EmulatedGameBoy.Pause();
			EmulatedGameBoy.Dispose();
			if (_romInformationWindow != null) _romInformationWindow.Dispose();
		}

		public double RelativeSpeed
		{
			get { return Volatile.Read(ref _relativeSpeed); }
			set { InterlockedSetValue(ref _relativeSpeed, value); }
		}

		public ReadOnlyCollection<ZoomLevelViewModel> ZoomLevels => _zoomLevels;

		public double CurrentZoomLevel
		{
			get { return _currentZoomLevel; }
			set
			{
				double oldValue = _currentZoomLevel;

				if (value != _currentZoomLevel)
				{
					_currentZoomLevel = value;

					foreach (var zoomLevel in _zoomLevels)
					{
						if (zoomLevel.Value == oldValue || zoomLevel.Value == value) zoomLevel.NotifyIsSelectedChanged();
					}
				}
			}
		}

		public RomInformation RomInformation => EmulatedGameBoy.RomInformation;

		public void SetVideoRenderer(IVideoRenderer videoRenderer)
		{
			EmulatedGameBoy.Bus.VideoRenderer = videoRenderer;
		}

		public void LoadRom()
		{
			string fileName = FileDialogService.ShowOpenFileDialog("GameBoy ROMs (*.gb;*.gbc;*.cgb)|*.gb;*.gbc;*.cgb|All Files (*.*)|*.*");

			if (fileName != null) LoadRom(fileName);
		}

		public void LoadRom(string fileName)
		{
			var romFileInfo = new FileInfo(fileName);

			// Open only existing rom files
			if (!romFileInfo.Exists)
				throw new FileNotFoundException();
			if (romFileInfo.Length < 512)
				throw new InvalidOperationException("ROM files must be at least 512 bytes.");
			if (romFileInfo.Length > 8 * 1024 * 1024)
				throw new InvalidOperationException("ROM files cannot exceed 8MB.");

			EmulatedGameBoy.LoadRom(MemoryUtility.ReadFile(romFileInfo, true));

			//if (EmulatedGameBoy.RomInformation.HasRam && EmulatedGameBoy.RomInformation.HasBattery)
			//{
			//	var ramFileInfo = new FileInfo(Path.Combine(romFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(romFileInfo.Name)) + ".sav");

			//	var ramSaveStream = ramFileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
			//	ramSaveStream.SetLength(EmulatedGameBoy.Mapper.SavedRamSize + (EmulatedGameBoy.RomInformation.HasTimer ? 48 : 0));
			//	ramSaveStream.Read(EmulatedGameBoy.ExternalRam, 0, EmulatedGameBoy.Mapper.SavedRamSize);
			//	ramSaveWriter = new BinaryWriter(ramSaveStream);

			//	if (EmulatedGameBoy.RomInformation.HasTimer)
			//	{
			//		var mbc3 = EmulatedGameBoy.Mapper as CrystalBoy.Emulation.Mappers.MemoryBankController3;

			//		if (mbc3 != null)
			//		{
			//			var rtcState = mbc3.RtcState;
			//			ramSaveReader = new BinaryReader(ramSaveStream);

			//			rtcState.Frozen = true;

			//			rtcState.Seconds = (byte)ramSaveReader.ReadInt32();
			//			rtcState.Minutes = (byte)ramSaveReader.ReadInt32();
			//			rtcState.Hours = (byte)ramSaveReader.ReadInt32();
			//			rtcState.Days = (short)((byte)ramSaveReader.ReadInt32() + ((byte)ramSaveReader.ReadInt32() << 8));

			//			rtcState.LatchedSeconds = (byte)ramSaveReader.ReadInt32();
			//			rtcState.LatchedMinutes = (byte)ramSaveReader.ReadInt32();
			//			rtcState.LatchedHours = (byte)ramSaveReader.ReadInt32();
			//			rtcState.LatchedDays = (short)((byte)ramSaveReader.ReadInt32() + ((byte)ramSaveReader.ReadInt32() << 8));

			//			rtcState.DateTime = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(ramSaveReader.ReadInt64());

			//			rtcState.Frozen = false;
			//		}
			//	}

			//	EmulatedGameBoy.Mapper.RamUpdated += Mapper_RamUpdated;
			//}

			NotifyPropertyChanged(nameof(RomInformation));

			EmulatedGameBoy.Run();
		}

		private IWindow RomInformationWindow => _romInformationWindow ?? (_romInformationWindow = WindowService.CreateWindow("RomInformation", new RomInformationViewModel(this)));

		public void ShowRomInformation() => RomInformationWindow.Show();
	}
}
