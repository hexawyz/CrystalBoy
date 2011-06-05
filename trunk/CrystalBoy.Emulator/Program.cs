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
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CrystalBoy.Emulation;
using CrystalBoy.Emulator.Properties;

namespace CrystalBoy.Emulator
{
	internal static class Program
	{
		private static readonly List<Assembly> pluginAssemblyList = new List<Assembly>();
		private static readonly Dictionary<Type, string> renderMethodDictionary = new Dictionary<Type, string>();

		public static readonly ReadOnlyCollection<Assembly> pluginAssemblyCollection = new ReadOnlyCollection<Assembly>(pluginAssemblyList);
		public static readonly Dictionary<Type, string> RenderMethodDictionary = renderMethodDictionary; // Need to implement a read only dictionary later

		#region Plugin Management

		private static void LoadPluginAssemblies()
		{
			var pluginAssemblies = new System.Collections.Specialized.StringCollection();
			bool updateNeeded = false;

			foreach (string pluginAssembly in Settings.Default.PluginAssemblies)
			{
				try
				{
					var assembly = Assembly.LoadFrom(pluginAssembly);

					FindPlugins(assembly);
					pluginAssemblyList.Add(assembly);

					pluginAssemblies.Add(pluginAssembly);
				}
				catch (FileNotFoundException)
				{
					MessageBox.Show(string.Format(Resources.Culture, Resources.AssemblyNotFoundErrorMessage, pluginAssembly), Resources.AssemblyLoadErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					updateNeeded = true;
				}
				catch (BadImageFormatException)
				{
					MessageBox.Show(string.Format(Resources.Culture, Resources.AssemblyArchitectureErrorMessage, pluginAssembly), Resources.AssemblyLoadErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					updateNeeded = true;
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine(ex);
					MessageBox.Show(string.Format(Resources.Culture, Resources.AssemblyLoadErrorMessage, pluginAssembly, ex), Resources.AssemblyLoadErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					updateNeeded = true;
				}
			}

			if (updateNeeded)
			{
				Settings.Default.PluginAssemblies = pluginAssemblies;
				Settings.Default.Save();
			}
		}
		
		private static void FindPlugins(Assembly assembly)
		{
			Type[] defaultTypeArray = new Type[] { typeof(Control) };

			try
			{
				foreach (Type type in assembly.GetTypes())
				{
					try
					{
						if (typeof(RenderMethod<Control>).IsAssignableFrom(type) && type.GetConstructor(defaultTypeArray) != null)
							AddRenderMethod(type);
					}
					catch (TypeLoadException ex)
					{
						MessageBox.Show(ex.ToString(), Resources.TypeLoadingErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						Console.Error.WriteLine(ex);
					}
				}
			}
			catch (ReflectionTypeLoadException ex)
			{
				MessageBox.Show(ex.ToString(), Resources.TypeLoadingErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				Console.Error.WriteLine(ex);
			}
		}

		private static void AddRenderMethod(Type renderMethodType)
		{
			DisplayNameAttribute[] displayNameAttributes = (DisplayNameAttribute[])renderMethodType.GetCustomAttributes(typeof(DisplayNameAttribute), false);
			string name = string.Intern(displayNameAttributes.Length > 0 ? displayNameAttributes[0].DisplayName : renderMethodType.Name);

			renderMethodDictionary.Add(renderMethodType, name);
		}

		#endregion

		/// <summary>Application entry point.</summary>
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			RuntimeHelpers.RunClassConstructor(typeof(LookupTables).TypeHandle);

			// Check for embedded plugins
			// This will be useful for providing an all-in-one assembly using ILMerge
			FindPlugins(typeof(Program).Assembly);

			// Load plugin assembles and check for external plugins
			LoadPluginAssemblies();

			Application.Run(new MainForm());
		}
	}
}
