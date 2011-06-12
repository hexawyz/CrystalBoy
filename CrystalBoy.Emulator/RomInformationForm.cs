#region Copyright Notice
// This file is part of CrystalBoy.
// Copyright © 2008-2011 Fabien Barbier
// 
// CrystalBoy is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// CrystalBoy is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CrystalBoy.Core;
using System.Reflection;

namespace CrystalBoy.Emulator
{
	partial class RomInformationForm : EmulatorForm
	{
		static Dictionary<RomType, string> descriptionCache = BuildDescriptionChache();

		private static Dictionary<RomType, string> BuildDescriptionChache()
		{
			Dictionary<RomType, string> descriptionCache = new Dictionary<RomType, string>();

			foreach (FieldInfo fieldInfo in typeof(RomType).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.GetField))
			{
				DescriptionAttribute[] descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
				RomType value = (RomType)fieldInfo.GetRawConstantValue();
				string description = string.Intern(descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : fieldInfo.Name);

				descriptionCache.Add(value, description);
			}

			return descriptionCache;
		}

		private string GetDescription(RomType romType)
		{
			string description;

			if (descriptionCache.TryGetValue(romType, out description))
				return description;
			else
				return romType.ToString();
		}

		public RomInformationForm(EmulatedGameBoy emulatedGameBoy)
			: base(emulatedGameBoy)
		{
			InitializeComponent();
		}

		private void UpdateRomInformation()
		{
			if (EmulatedGameBoy.RomLoaded)
			{
				nameValueLabel.Text = EmulatedGameBoy.RomInformation.Name;
				makerCodeValueLabel.Text = EmulatedGameBoy.RomInformation.MakerCode;
				makerNameValueLabel.Text = EmulatedGameBoy.RomInformation.MakerName;
				romTypeValueLabel.Text = GetDescription(EmulatedGameBoy.RomInformation.RomType);
				romSizeValueLabel.Text = Common.FormatSize(EmulatedGameBoy.RomInformation.RomSize);
				ramSizeValueLabel.Text = Common.FormatSize(EmulatedGameBoy.RomInformation.RamSize);
				sgbCheckBox.Checked = EmulatedGameBoy.RomInformation.SuperGameBoySupport;
				cgbCheckBox.Checked = EmulatedGameBoy.RomInformation.ColorGameBoySupport;
			}
			else
			{
				nameValueLabel.Text = "-";
				makerCodeValueLabel.Text = "-";
				makerNameValueLabel.Text = "-";
				romTypeValueLabel.Text = "-";
				romSizeValueLabel.Text = "-";
				ramSizeValueLabel.Text = "-";
				sgbCheckBox.Checked = false;
				cgbCheckBox.Checked = false;
			}
		}

		protected override void OnShown(EventArgs e)
		{
			UpdateRomInformation();
			base.OnShown(e);
		}

		protected override void OnRomChanged(EventArgs e)
		{
			UpdateRomInformation();
			base.OnRomChanged(e);
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
