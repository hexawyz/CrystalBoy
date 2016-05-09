using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CrystalBoy.Emulator.Controls
{
	internal sealed class UncheckableCheckBox : CheckBox
	{
		protected override void OnClick()
		{
			RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, this));
		}
	}
}
