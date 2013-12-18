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
		#region ErrorStream Class

		private sealed class ErrorStream : Stream
		{
			private Stream standardErrorStream;
			private Stream fileStream;
			private string logFileName;
			private bool fileStreamCreationFailed;

			public ErrorStream(Stream standardErrorStream, string logFileName)
			{
				this.standardErrorStream = standardErrorStream;
				this.logFileName = Path.IsPathRooted(logFileName) ? logFileName : Path.Combine(Environment.CurrentDirectory, logFileName);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (standardErrorStream != null) standardErrorStream.Dispose();
					standardErrorStream = null;
					if (fileStream != null) fileStream.Close();
					fileStream = null;
				}
				base.Dispose(disposing);
			}

			public override bool CanRead { get { return false; } }
			public override bool CanSeek { get { return false; } }
			public override bool CanWrite { get { return true; } }
			public override void Flush()
			{
				if (standardErrorStream == null) throw new ObjectDisposedException(GetType().FullName);
				standardErrorStream.Flush();
				if (fileStream != null) fileStream.Flush();
			}

			public override long Length
			{
				get
				{
					if (fileStreamCreationFailed) throw new NotSupportedException();
					if (standardErrorStream == null) throw new ObjectDisposedException(GetType().FullName);
					return fileStream != null ? fileStream.Length : 0;
				}
			}

			public override long Position
			{
				get { throw new NotSupportedException(); }
				set { throw new NotSupportedException(); }
			}

			public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) { throw new NotSupportedException(); }
			public override int Read(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
			public override int ReadByte() { throw new NotSupportedException(); }

			public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
			public override void SetLength(long value) { throw new NotSupportedException(); }

			public override void Write(byte[] buffer, int offset, int count)
			{
				if (standardErrorStream == null) throw new ObjectDisposedException(GetType().FullName);

				// First write to the standard error stream (which may already have been redirected, but we don't care, really)
				standardErrorStream.Write(buffer, offset, count);

				// Then attempt to write to the error log file (which may fail *once* and print an error on its own)
				if (fileStream == null) TryCreateFileStream();

				if (fileStream != null)
					try { fileStream.Write(buffer, offset, count); }
					catch (IOException ex)
					{
						fileStreamCreationFailed = true;
						try { fileStream.Close(); }
						finally { fileStream = null; }
						// Writes the exception to the error stream (the Write method will be re-entered…)
						Console.Error.WriteLine(ex);
					}
			}

			private void TryCreateFileStream()
			{
				// Only try to create the stream once.
				if (fileStreamCreationFailed) return;

				try { fileStream = File.Create(logFileName, 4096, FileOptions.SequentialScan); }
				catch (Exception ex)
				{
					// Set the fail flag first
					fileStreamCreationFailed = true;
					// Then write to the error stream (the Write method wil be re-entered, but that shouldn't be a problem…)
					Console.Error.WriteLine(ex);
				}
			}
		}

		#endregion

		private static readonly Type[] supportedSampleTypes = { typeof(short) };
		private static readonly Type[] supportedRenderObjectTypes = { typeof(Control), typeof(IWin32Window) };

		private static readonly List<Assembly> pluginAssemblyList = new List<Assembly>();
		private static readonly List<PluginInformation> pluginList = new List<PluginInformation>();

		public static readonly ReadOnlyCollection<Assembly> PluginAssemblyCollection = new ReadOnlyCollection<Assembly>(pluginAssemblyList);
		public static readonly ReadOnlyCollection<PluginInformation> PluginCollection = new ReadOnlyCollection<PluginInformation>(pluginList);

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
				catch (FileNotFoundException ex)
				{
					Console.Error.WriteLine(ex);
					MessageBox.Show(string.Format(Resources.Culture, Resources.AssemblyNotFoundErrorMessage, pluginAssembly), Resources.AssemblyLoadErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					updateNeeded = true;
				}
				catch (BadImageFormatException ex)
				{
					Console.Error.WriteLine(ex);
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
			try
			{
				foreach (var type in assembly.GetTypes())
				{
					try
					{
						// Ignore any abstract type, as we want only real renderers here…
						if (type.IsAbstract) continue;

						// We are going to do generic argument resolution here.
						// For each generic type we encounter, the rules are quite simple.
						// When it comes to unbound generic arguments, the only ones supported are the one defined by the base classes.
						// The generic parameters when there are some (mandatory for AudioRenderer, optional for VideoRenderer) must be either unbound or of a type supported by the application.
						// Typically, the application will use integral types for audio samples, and Control or IWin32Window for render objects.
						var genericArguments = type.IsGenericType ? type.GetGenericArguments() : Type.EmptyTypes;

						// We only allow for one generic argument, as of now…
						if (genericArguments.Length > 1) continue;

						var parentType = type;

						var genericRendererType = !type.IsSubclassOf(typeof(AudioRenderer)) ? !type.IsSubclassOf(typeof(VideoRenderer)) ? null : typeof(VideoRenderer<>) : typeof(AudioRenderer<>);

						if (genericRendererType != null)
						{
							// A renderer can come in two flavors:
							// A generic version with a constructor taking the render object, where the generic argument matches the rules mentioned before.
							// A non-generic version with an empty constructor.
							do
							{
								parentType = type.BaseType;

								if (parentType.IsGenericType)
								{
									var parentTypeDefinition = parentType.GetGenericTypeDefinition();
									var parentTypeGenericArguments = parentType.GetGenericArguments();

									if (parentTypeDefinition == genericRendererType)
									{
										if (IsGenericArgumentTypeSupported(parentTypeGenericArguments[0], supportedRenderObjectTypes, type.IsGenericType, !type.IsGenericType, GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint, GenericParameterAttributes.ReferenceTypeConstraint)
											&& type.GetConstructor(new[] { parentTypeGenericArguments[0] }) != null)
										{
											pluginList.Add(new PluginInformation(type));
											break;
										}
									}
								}
								else if (parentType == genericRendererType.BaseType)
								{
									if (!type.IsGenericType && type.GetConstructor(Type.EmptyTypes) != null) pluginList.Add(new PluginInformation(type));
									break;
								}
							}
							while (true);
						}
					}
					catch (TypeLoadException ex)
					{
						Console.Error.WriteLine(ex);
						MessageBox.Show(ex.ToString(), Resources.TypeLoadingErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
				}
			}
			catch (ReflectionTypeLoadException ex)
			{
				Console.Error.WriteLine(ex);
				foreach (Exception lex in ex.LoaderExceptions)
					Console.Error.WriteLine(lex);
				MessageBox.Show(ex.ToString(), Resources.TypeLoadingErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private static bool IsGenericArgumentTypeSupported
		(
			Type argumentType,
			Type[] supportedTypes,
			bool allowOpen = false,
			bool allowClosed = true,
			GenericParameterAttributes allowedParameterAttributes = GenericParameterAttributes.SpecialConstraintMask | GenericParameterAttributes.DefaultConstructorConstraint | GenericParameterAttributes.VarianceMask,
			GenericParameterAttributes requiredParameterAttributes = GenericParameterAttributes.None
		)
		{
			if (argumentType.IsGenericParameter)
				return allowOpen
					&& (argumentType.GenericParameterAttributes & ~allowedParameterAttributes) == GenericParameterAttributes.None
					&& (argumentType.GenericParameterAttributes & requiredParameterAttributes) == requiredParameterAttributes;
			else if (!allowClosed) return false;

			foreach (var supportedType in supportedTypes)
				if (argumentType == supportedType) return true;

			return false;
		}

		#endregion

#if DEBUG
		[System.Runtime.InteropServices.DllImport("user32", SetLastError = true)]
		private static extern bool SetProcessDPIAware();
#endif

		/// <summary>Application entry point.</summary>
		[STAThread]
		private static void Main()
		{
			Console.SetError(new StreamWriter(new ErrorStream(Console.OpenStandardError(), "CrystalBoy.Emulator.log")) { AutoFlush = true });

#if DEBUG // When debugging in Visual Studio, this will enable DPI-awareness for our application
			SetProcessDPIAware();
#endif
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			ToolStripManager.VisualStylesEnabled = true;
			ToolStripManager.RenderMode = ToolStripManagerRenderMode.Professional;

			RuntimeHelpers.RunClassConstructor(typeof(LookupTables).TypeHandle);

			// Check for embedded plugins
			// This will be useful for providing an all-in-one assembly using ILMerge
			FindPlugins(typeof(Program).Assembly);

			// Load plugin assembles and check for external plugins
			LoadPluginAssemblies();

			// Attempt to clean the garbage left by the plugin loading process
			GC.Collect();

			Application.Run(new MainForm());
		}
	}
}
