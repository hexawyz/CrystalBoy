#region Copyright Notice
// This file is part of CrystalBoy.
// Copyright (C) 2008 Fabien Barbier
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
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CrystalBoy.Emulation;

namespace CrystalBoy.Emulator
{
	static class Program
	{
		/// <summary>
		/// Point d'entrée principal de l'application.
		/// </summary>
		[STAThread]
		static void Main()
		{
#if PLUGIN_EXCEPTION_CATCHER
			// Catches plugin loading exception and display an error message
			// Plugins are loaded in the static initializer of MainForm, meaning that this will cause an un-repairable crash if any plugin fails to load...
			try
			{
#endif
				RuntimeHelpers.RunClassConstructor(typeof(LookupTables).TypeHandle);
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm());
#if PLUGIN_EXCEPTION_CATCHER
			}
			catch (TypeInitializationException ex)
			{
				var sb = new System.Text.StringBuilder();

				sb.AppendFormat("{0}:", ex.GetType().Name);
				sb.AppendLine();
				sb.AppendLine();
				sb.AppendLine("* Message:");
				sb.AppendLine(ex.Message);
				sb.AppendLine();
				sb.AppendLine("* StackTrace:");
				sb.AppendLine(ex.StackTrace);
				sb.AppendLine();
				if (ex.InnerException != null)
				{
					sb.AppendFormat("InnerException {0}:", ex.InnerException.GetType().Name);
					sb.AppendLine();
					sb.AppendLine();
					sb.AppendLine("* Message:");
					sb.AppendLine(ex.InnerException.Message);
					sb.AppendLine();
					sb.AppendLine("* StackTrace:");
					sb.AppendLine(ex.InnerException.StackTrace);
					sb.AppendLine();
				}

				if (MessageBox.Show(sb.ToString() + "It would help making the emualtor better if you paste this error message as an issue at http://code.google.com/p/crystalboy." + Environment.NewLine + "Copy this messageto the clipboard ?", "Exception", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
				{
					Clipboard.Clear();
					Clipboard.SetData(DataFormats.UnicodeText, sb.ToString());
				}
			}
#endif
		}
	}
}
