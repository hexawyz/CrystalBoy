using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CrystalBoy.Core;
using System.Reflection;
using CrystalBoy.Emulation;

namespace CrystalBoy.Emulator
{
	partial class RomInformationForm : EmulatorForm
	{
		static Dictionary<RomType, string> _descriptionCache = BuildDescriptionChache();

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

		private static string GetDescription(RomType romType)
		{
			string description;

			if (_descriptionCache.TryGetValue(romType, out description))
				return description;
			else
				return romType.ToString();
		}

		private static void CopyPalette(Color[] destination, ColorPalette source)
		{
			for (int i = 0; i < 4; i++)
			{
				destination[i] = Color.FromArgb(unchecked((int)LookupTables.StandardColorLookupTable32[source[i]]));
			}
		}

		private static void CopyPalette(Color[] destination, uint[] source)
		{
			for (int i = 0; i < 4; i++)
			{
				destination[i] = Color.FromArgb(unchecked((int)source[i]));
			}
		}

		private readonly Color[] _backgroundColors = new Color[4];
		private readonly Color[] _object0Colors = new Color[4];
		private readonly Color[] _object1Colors = new Color[4];

		public RomInformationForm(EmulatedGameBoy emulatedGameBoy)
			: base(emulatedGameBoy)
		{
			InitializeComponent();
			backgroundColorPaletteTableLayoutPanel.Tag = _backgroundColors;
			object0ColorPaletteTableLayoutPanel.Tag = _object0Colors;
			object1ColorPaletteTableLayoutPanel.Tag = _object1Colors;
		}

		private void UpdateRomInformation()
		{
			if (EmulatedGameBoy.IsRomLoaded)
			{
				nameValueLabel.Text = EmulatedGameBoy.RomInformation.Name;
				makerCodeValueLabel.Text = EmulatedGameBoy.RomInformation.MakerCode;
				makerNameValueLabel.Text = EmulatedGameBoy.RomInformation.MakerName;
				romTypeValueLabel.Text = GetDescription(EmulatedGameBoy.RomInformation.RomType);
				romSizeValueLabel.Text = Common.FormatSize(EmulatedGameBoy.RomInformation.RomSize);
				ramSizeValueLabel.Text = Common.FormatSize(EmulatedGameBoy.RomInformation.RamSize);
				sgbCheckBox.Checked = EmulatedGameBoy.RomInformation.SupportsSuperGameBoy;
				cgbCheckBox.Checked = EmulatedGameBoy.RomInformation.SupportsColorGameBoy;
				var colorPalette = EmulatedGameBoy.RomInformation.AutomaticColorPalette;
				colorPaletteGroupBox.Enabled = colorPalette != null;
				if (colorPalette != null) CopyPalettes(colorPalette.GetValueOrDefault());
				else ResetPalettes();
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
				colorPaletteGroupBox.Enabled = false;
				ResetPalettes();
			}

			backgroundColorPaletteTableLayoutPanel.Refresh();
			object0ColorPaletteTableLayoutPanel.Refresh();
			object1ColorPaletteTableLayoutPanel.Refresh();
		}

		private void CopyPalettes(FixedColorPalette colorPalette)
		{
			CopyPalette(_backgroundColors, colorPalette.BackgroundPalette);
			CopyPalette(_object0Colors, colorPalette.ObjectPalette0);
			CopyPalette(_object1Colors, colorPalette.ObjectPalette1);
		}

		private void ResetPalettes()
		{
			CopyPalette(_backgroundColors, LookupTables.GrayPalette);
			CopyPalette(_object0Colors, LookupTables.GrayPalette);
			CopyPalette(_object1Colors, LookupTables.GrayPalette);
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

		private void OnPaletteCellPaint(object sender, TableLayoutCellPaintEventArgs e)
		{
			using (var brush = new SolidBrush(((Color[])((Control)sender).Tag)[e.Column]))
				e.Graphics.FillRectangle(brush, e.CellBounds);
		}
	}
}
