using CrystalBoy.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrystalBoy.Emulator.ViewModels
{
	internal sealed class RomInformationViewModel : BindableObject
	{
		private ApplicationViewModel ApplicationViewModel { get; }

		public RomInformationViewModel(ApplicationViewModel applicationViewModel)
		{
			ApplicationViewModel = applicationViewModel;

			applicationViewModel.PropertyChanged += OnApplicationViewModelPropertyChanged;
		}

		private void OnApplicationViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ApplicationViewModel.RomInformation))
			{
				NotifyPropertyChanged(nameof(RomName));
				NotifyPropertyChanged(nameof(MakerCode));
				NotifyPropertyChanged(nameof(MakerName));
				NotifyPropertyChanged(nameof(IsRegularGameBoyCompatible));
				NotifyPropertyChanged(nameof(IsColorGameBoyCompatible));
				NotifyPropertyChanged(nameof(IsSuperGameBoyCompatible));
				NotifyPropertyChanged(nameof(RomType));
				NotifyPropertyChanged(nameof(RomSize));
				NotifyPropertyChanged(nameof(RamSize));
				NotifyPropertyChanged(nameof(BackgroundColor0));
				NotifyPropertyChanged(nameof(BackgroundColor1));
				NotifyPropertyChanged(nameof(BackgroundColor2));
				NotifyPropertyChanged(nameof(BackgroundColor3));
				NotifyPropertyChanged(nameof(Object0Color0));
				NotifyPropertyChanged(nameof(Object0Color1));
				NotifyPropertyChanged(nameof(Object0Color2));
				NotifyPropertyChanged(nameof(Object0Color3));
				NotifyPropertyChanged(nameof(Object1Color0));
				NotifyPropertyChanged(nameof(Object1Color1));
				NotifyPropertyChanged(nameof(Object1Color2));
				NotifyPropertyChanged(nameof(Object1Color3));
			}
		}

		public string RomName => ApplicationViewModel.RomInformation?.Name;
		public string MakerCode => ApplicationViewModel.RomInformation?.MakerCode;
		public string MakerName => ApplicationViewModel.RomInformation?.MakerName;

		public bool? IsRegularGameBoyCompatible => ApplicationViewModel.RomInformation?.SupportsRegularGameBoy;
		public bool? IsColorGameBoyCompatible => ApplicationViewModel.RomInformation?.SupportsColorGameBoy;
		public bool? IsSuperGameBoyCompatible => ApplicationViewModel.RomInformation?.SupportsSuperGameBoy;

		public RomType? RomType => ApplicationViewModel.RomInformation?.RomType;
		public int? RomSize => ApplicationViewModel.RomInformation?.RomSize;
		public int? RamSize => ApplicationViewModel.RomInformation?.RamSize;

		public short? BackgroundColor0 => GetColor(ApplicationViewModel.RomInformation?.AutomaticColorPalette?.BackgroundPalette, 0);
		public short? BackgroundColor1 => GetColor(ApplicationViewModel.RomInformation?.AutomaticColorPalette?.BackgroundPalette, 1);
		public short? BackgroundColor2 => GetColor(ApplicationViewModel.RomInformation?.AutomaticColorPalette?.BackgroundPalette, 2);
		public short? BackgroundColor3 => GetColor(ApplicationViewModel.RomInformation?.AutomaticColorPalette?.BackgroundPalette, 3);

		public short? Object0Color0 => GetColor(ApplicationViewModel.RomInformation?.AutomaticColorPalette?.ObjectPalette0, 0);
		public short? Object0Color1 => GetColor(ApplicationViewModel.RomInformation?.AutomaticColorPalette?.ObjectPalette0, 1);
		public short? Object0Color2 => GetColor(ApplicationViewModel.RomInformation?.AutomaticColorPalette?.ObjectPalette0, 2);
		public short? Object0Color3 => GetColor(ApplicationViewModel.RomInformation?.AutomaticColorPalette?.ObjectPalette0, 3);

		public short? Object1Color0 => GetColor(ApplicationViewModel.RomInformation?.AutomaticColorPalette?.ObjectPalette1, 0);
		public short? Object1Color1 => GetColor(ApplicationViewModel.RomInformation?.AutomaticColorPalette?.ObjectPalette1, 1);
		public short? Object1Color2 => GetColor(ApplicationViewModel.RomInformation?.AutomaticColorPalette?.ObjectPalette1, 2);
		public short? Object1Color3 => GetColor(ApplicationViewModel.RomInformation?.AutomaticColorPalette?.ObjectPalette1, 3);

		private static short? GetColor(ColorPalette? palette, int colorIndex) => palette != null ?
			palette.GetValueOrDefault()[colorIndex] :
			null as short?;
	}
}
