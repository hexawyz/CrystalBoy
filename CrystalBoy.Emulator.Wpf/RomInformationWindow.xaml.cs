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
using System.Windows.Shapes;

namespace CrystalBoy.Emulator
{
	/// <summary>
	/// Interaction logic for RomInformationWindow.xaml
	/// </summary>
	public partial class RomInformationWindow : Window
	{
		public RomInformationWindow()
		{
			InitializeComponent();
		}

		private void OnOkButtonClick(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			Hide();
		}
	}
}
