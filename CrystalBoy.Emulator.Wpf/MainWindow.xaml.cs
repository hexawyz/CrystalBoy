using CrystalBoy.Emulator.Rendering.SharpDX;
using CrystalBoy.Emulator.Services;
using CrystalBoy.Emulator.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrystalBoy.Emulator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	internal partial class MainWindow : Window, IWindowManager, IFileDialogService
	{
		private sealed class WindowWrapper : IWindow
		{
			private Window Window { get; }

			public WindowWrapper(Window window)
			{
				Window = window;
			}

			public void Dispose() => Window.Close();

			public void Show()
			{
				if (Window.IsVisible) Window.Activate();
				else Window.Show();
			}

			public void Hide() => Window.Hide();
		}

		private static readonly Dictionary<string, Type> ViewTypes = new Dictionary<string, Type>
		{
			{ "RomInformation", typeof(RomInformationWindow) },
		};

		public MainWindow()
		{
			DataContext = new ApplicationViewModel(this, this);
			InitializeComponent();
			ViewModel.SetVideoRenderer(new Direct2DRenderer(renderTarget));
		}

		protected override void OnClosed(EventArgs e)
		{
			ViewModel.Dispose();
			DataContext = null;
			base.OnClosed(e);
		}

		public ApplicationViewModel ViewModel => (ApplicationViewModel)DataContext;

		private void OnCanExecuteAlways(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
			e.Handled = true;
		}

		private void OnOpenExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			e.Handled = true;

			ViewModel.LoadRom();
		}

		private void OnShowRomInformationCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
			e.Handled = true;
		}

		private void OnShowRomInformationExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			e.Handled = true;
			ViewModel.ShowRomInformation();
		}

		private void OnExitExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			e.Handled = true;
			Close();
		}

		private void OnZoomLevelMenuItemClick(object sender, RoutedEventArgs e)
		{
			ViewModel.CurrentZoomLevel = ((ApplicationViewModel.ZoomLevelViewModel)((MenuItem)sender).DataContext).Value;
		}

		#region IFileDialogService

		string IFileDialogService.ShowOpenFileDialog(string filter)
		{
			var dialog = new OpenFileDialog
			{
				Filter = filter
			};

			return dialog.ShowDialog(this).GetValueOrDefault() ? dialog.FileName : null;
		}

		string IFileDialogService.ShowSaveFileDialog(string filter)
		{
			var dialog = new SaveFileDialog
			{
				Filter = filter
			};

			return dialog.ShowDialog(this).GetValueOrDefault() ? dialog.FileName : null;
		}

		#endregion

		#region IWindowManager

		IWindow IWindowManager.CreateWindow(string viewName, object viewModel)
		{
			var window = (Window)Activator.CreateInstance(ViewTypes[viewName]);

			window.DataContext = viewModel;
			window.Owner = this;

			return new WindowWrapper(window);
		}

		#endregion
	}
}
