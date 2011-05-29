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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CrystalBoy.Core;
using CrystalBoy.Emulation;
using CrystalBoy.Emulator.Properties;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace CrystalBoy.Emulator
{
	partial class MainForm : Form
	{
		private static Assembly[] pluginAssemblies = LoadPluginAssemblies();
		private static Type[] availableRenderMethods = DetectRenderMethods();
		private static Dictionary<Type, string> renderMethodDitionary = BuildRenderMethodDictionary();

		private DebuggerForm debuggerForm;
		private TileViewerForm tileViewerForm;
		private MapViewerForm mapViewerForm;
		private RomInformationForm romInformationForm;
		private EmulatedGameBoy emulatedGameBoy;
		private RenderMethod renderMethod;
		private Dictionary<Type, ToolStripMenuItem> renderMethodMenuItemDictionary;
		private bool pausedForResizing;

		#region Constructor and Initialization

		#region Plugin Management

		private static Assembly[] LoadPluginAssemblies()
		{
			List<Assembly> assemblyList = new List<Assembly>();

			foreach (string pluginAssembly in Settings.Default.PluginAssemblies)
			{
				try { assemblyList.Add(Assembly.LoadFrom(pluginAssembly)); }
				catch (BadImageFormatException) { }
			}

			return assemblyList.ToArray();
		}

		private static Type[] DetectRenderMethods()
		{
			List<Type> typeList = new List<Type>();
			Type[] defaultTypeArray = new Type[] { typeof(Control) };

			// Check for embedded render methods
			// This will be useful for providing an al-in-one assembly using ILMerge
			foreach (Type type in typeof(MainForm).Assembly.GetTypes())
				if (typeof(RenderMethod<Control>).IsAssignableFrom(type) && type.GetConstructor(defaultTypeArray) != null)
					typeList.Add(type);

			// Check for external render methods
			foreach (Assembly pluginAssembly in pluginAssemblies)
				foreach (Type type in pluginAssembly.GetExportedTypes())
					if (typeof(RenderMethod<Control>).IsAssignableFrom(type) && type.GetConstructor(defaultTypeArray) != null)
						typeList.Add(type);

			return typeList.ToArray();
		}

		private static Dictionary<Type, string> BuildRenderMethodDictionary()
		{
			Dictionary<Type, string> renderMethodDictionary = new Dictionary<Type,string>();

			foreach (Type renderMethodType in availableRenderMethods)
			{
				DisplayNameAttribute[] displayNameAttributes = (DisplayNameAttribute[])renderMethodType.GetCustomAttributes(typeof(DisplayNameAttribute), false);
				string name = string.Intern(displayNameAttributes.Length > 0 ? displayNameAttributes[0].DisplayName : renderMethodType.Name);

				renderMethodDictionary.Add(renderMethodType, name);
			}

			return renderMethodDictionary;
		}

		#endregion

		public MainForm()
		{
			InitializeComponent();
			emulatedGameBoy = new EmulatedGameBoy();
			emulatedGameBoy.EnableFramerateLimiter = Settings.Default.LimitSpeed;
			emulatedGameBoy.RomChanged += OnRomChanged;
			emulatedGameBoy.EmulationStatusChanged += OnEmulationStatusChanged;
			emulatedGameBoy.NewFrame += OnNewFrame;
			AdjustSize(Settings.Default.RenderSize);
			ResetEmulationMenuItems(false);
			UpdateEmulationStatus();
			UpdateFrameRate();
			SetStatusTextHandler();
			CreateRenderMethodMenuItems();
		}

		private void CreateRenderMethodMenuItems()
		{
			renderMethodMenuItemDictionary = new Dictionary<Type, ToolStripMenuItem>();

			foreach (Type renderMethodType in availableRenderMethods)
			{
				ToolStripMenuItem renderMethodMenuItem = new ToolStripMenuItem(renderMethodDitionary[renderMethodType]);

				renderMethodMenuItem.Click += new EventHandler(renderMethodMenuItem_Click);
				renderMethodMenuItem.Tag = renderMethodType;

				renderMethodMenuItemDictionary.Add(renderMethodType, renderMethodMenuItem);

				renderMethodToolStripMenuItem.DropDownItems.Add(renderMethodMenuItem);
			}
		}

		private void SetStatusTextHandler()
		{
			foreach (ToolStripItem item in mainMenuStrip.Items)
				SetStatusTextHandler(item);
		}

		private void SetStatusTextHandler(ToolStripItem toolStripItem)
		{
			toolStripItem.MouseEnter += new EventHandler(toolStripItem_MouseEnter);
			toolStripItem.MouseLeave += new EventHandler(toolStripItem_MouseLeave);

			if (toolStripItem is ToolStripDropDownItem)
			{
				ToolStripDropDownItem dropDownItem = (ToolStripDropDownItem)toolStripItem;

				foreach (ToolStripItem item in dropDownItem.DropDownItems)
					SetStatusTextHandler(item);
			}
		}

		private RenderMethod<Control> CreateRenderMethod(Type renderMethodType)
		{
			ConstructorInfo constructor = renderMethodType.GetConstructor(new Type[] { typeof(Control) });

			return (RenderMethod<Control>)constructor.Invoke(new object[] { toolStripContainer.ContentPanel });
		}

		private void SwitchRenderMethod(Type renderMethodType)
		{
			if (renderMethod != null)
			{
				emulatedGameBoy.Bus.RenderMethod = null;
				renderMethod.Dispose();
				renderMethod = null;
			}

			renderMethod = CreateRenderMethod(renderMethodType);

			ToolStripMenuItem selectedMethodMenuItem = renderMethodMenuItemDictionary[renderMethodType];

			foreach (ToolStripMenuItem renderMethodMenuItem in renderMethodMenuItemDictionary.Values)
				renderMethodMenuItem.Checked = renderMethodMenuItem == selectedMethodMenuItem;

			// Store the FullName once we know the type of render method to use
			Settings.Default.RenderMethod = renderMethodType.FullName; // Don't use AssemblyQualifiedName for easing updates, though it should be a better choice

			emulatedGameBoy.Bus.RenderMethod = renderMethod;
		}

		private void InitializeRenderMethod()
		{
			string renderMethodName = Settings.Default.RenderMethod;
			Type renderMethodType = Type.GetType(renderMethodName); // Try to find the type by simple reflexion (will work for embedded types and AssemblyQualifiedNames)

			if (renderMethodType != null)
				SwitchRenderMethod(renderMethodType);
			else // If simple reflexion failed, try using Name and FullName matches (there may be multiple matches in that case but it is better to avoid writing the full AssemblyQualifiedName in initial configurtaion files)
			{
				for (int i = 0; i < availableRenderMethods.Length; i++)
				{
					renderMethodType = availableRenderMethods[i];

					if (renderMethodType.Name == renderMethodName || renderMethodType.FullName == renderMethodName)
					{
						SwitchRenderMethod(renderMethodType);
						return;
					}
				}

				// If nothing was found, try using the first render method found, or throw an exception if nothing can be done
				if (availableRenderMethods.Length > 0) SwitchRenderMethod(availableRenderMethods[0]);
				else throw new InvalidOperationException();
			}
		}

		#endregion

		#region Utility Forms

		private DebuggerForm DebuggerForm
		{
			get
			{
				if (debuggerForm == null)
					debuggerForm = new DebuggerForm(emulatedGameBoy);
				return debuggerForm;
			}
		}

		private TileViewerForm TileViewerForm
		{
			get
			{
				if (tileViewerForm == null)
					tileViewerForm = new TileViewerForm(emulatedGameBoy);
				return tileViewerForm;
			}
		}

		private MapViewerForm MapViewerForm
		{
			get
			{
				if (mapViewerForm == null)
					mapViewerForm = new MapViewerForm(emulatedGameBoy);
				return mapViewerForm;
			}
		}

		private RomInformationForm RomInformationForm
		{
			get
			{
				if (romInformationForm == null)
					romInformationForm = new RomInformationForm(emulatedGameBoy);
				return romInformationForm;
			}
		}

		#endregion

		private void LoadRom(string fileName)
		{
			FileInfo fileInfo = new FileInfo(fileName);

			// Open only existing rom files
			if (!fileInfo.Exists)
				throw new FileNotFoundException();
			// Limit the rom size to 4 Mb
			if (fileInfo.Length > 4 * 1024 * 1024)
				throw new InvalidOperationException();
			emulatedGameBoy.LoadRom(MemoryUtility.ReadFile(fileInfo));
			emulatedGameBoy.Run();
		}

		#region Size Management

		private void SetZoomFactor(int factor)
		{
			if (factor <= 0)
				throw new ArgumentOutOfRangeException("factor");
			Settings.Default.ZoomFactor = factor;
			AdjustSize(factor * 160, factor * 144);
		}

		private void AdjustSize(Size requestedSize)
		{
			AdjustSize(requestedSize.Width, requestedSize.Height);
		}

		private void AdjustSize(int requestedWidth, int requestedHeight)
		{
			Size newSize, panelSize;

			newSize = ClientSize;
			panelSize = toolStripContainer.ContentPanel.ClientSize;
			newSize.Width = newSize.Width - panelSize.Width + requestedWidth;
			newSize.Height = newSize.Height - panelSize.Height + requestedHeight;

			ClientSize = newSize;
		}

		protected override void OnResizeBegin(EventArgs e) { if (pausedForResizing = emulatedGameBoy.EmulationStatus == EmulationStatus.Running) emulatedGameBoy.Pause(); }

		protected override void OnResizeEnd(EventArgs e) { if (pausedForResizing) emulatedGameBoy.Run(); }

		#endregion

		#region Status Updates

		private void UpdateZoomMenuItems()
		{
			ToolStripMenuItem zoomItem = null;

			foreach (ToolStripMenuItem menuItem in zoomToolStripMenuItem.DropDownItems)
				menuItem.Checked = (menuItem == zoomItem);
		}

		private void ResetEmulationMenuItems(bool enable)
		{
			pauseToolStripMenuItem.Enabled = enable;
			resetToolStripMenuItem.Enabled = enable;
		}

		private void UpdateEmulationStatus() { emulationStatusToolStripStatusLabel.Text = emulatedGameBoy.EmulationStatus.ToString(); }

		private void UpdateFrameRate()
		{
			double frameRate = emulatedGameBoy.FrameRate;

			if (frameRate > 0) frameRateToolStripStatusLabel.Text = "FPS: " + frameRate.ToString();
			else frameRateToolStripStatusLabel.Text = "FPS: -";
		}

		#endregion

		#region Event Handling

		protected override void OnShown(EventArgs e)
		{
			InitializeRenderMethod();
			emulatedGameBoy.Bus.RenderMethod = renderMethod;
			base.OnShown(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			Settings.Default.Save();
			base.OnClosed(e);
		}

		private void OnRomChanged(object sender, EventArgs e) { ResetEmulationMenuItems(emulatedGameBoy.RomLoaded); }

		private void OnEmulationStatusChanged(object sender, EventArgs e) { UpdateEmulationStatus(); }

		private unsafe void OnNewFrame(object sender, EventArgs e) { UpdateFrameRate(); }

		private void toolStripContainer_ContentPanel_Paint(object sender, PaintEventArgs e) { renderMethod.Render(); }

		#region Menus

		#region General

		void toolStripItem_MouseLeave(object sender, EventArgs e)
		{
			toolStripStatusLabel.Text = "";
		}

		private void toolStripItem_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel.Text = ((ToolStripItem)sender).ToolTipText;
		}

		#endregion

		#region File

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
				LoadRom(openFileDialog.FileName);
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Emulation

		private void emulationToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			pauseToolStripMenuItem.Checked = emulatedGameBoy.EmulationStatus == EmulationStatus.Paused;
			limitSpeedToolStripMenuItem.Checked = emulatedGameBoy.EnableFramerateLimiter;
		}

		private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (emulatedGameBoy.EmulationStatus == EmulationStatus.Paused) emulatedGameBoy.Run();
			else emulatedGameBoy.Pause();
		}

		private void resetToolStripMenuItem_Click(object sender, EventArgs e) { emulatedGameBoy.Reset(); }

		#region Video

		private void videoToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			interpolationToolStripMenuItem.Enabled = renderMethod.SupportsInterpolation;
			interpolationToolStripMenuItem.Checked = renderMethod.Interpolation;
		}

		private void renderMethodMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem renderMethodMenuItem = (ToolStripMenuItem)sender;

			SwitchRenderMethod((Type)renderMethodMenuItem.Tag);
		}

		#region Zoom

		private void zoomToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			Size renderSize = toolStripContainer.ContentPanel.ClientSize;
			ToolStripMenuItem checkItem = null;

			if (renderSize == new Size(160, 144))
				checkItem = zoom100toolStripMenuItem;
			else if (renderSize == new Size(320, 288))
				checkItem = zoom200toolStripMenuItem;
			else if (renderSize == new Size(480, 432))
				checkItem = zoom300toolStripMenuItem;
			else if (renderSize == new Size(649, 567))
				checkItem = zoom400toolStripMenuItem;

			foreach (ToolStripMenuItem zoomMenuItem in zoomToolStripMenuItem.DropDownItems)
				zoomMenuItem.Checked = zoomMenuItem == checkItem;
		}

		private void zoom100toolStripMenuItem_Click(object sender, EventArgs e) { SetZoomFactor(1); }

		private void zoom200toolStripMenuItem_Click(object sender, EventArgs e) { SetZoomFactor(2); }

		private void zoom300toolStripMenuItem_Click(object sender, EventArgs e) { SetZoomFactor(3); }

		private void zoom400toolStripMenuItem_Click(object sender, EventArgs e) { SetZoomFactor(4); }

		#endregion

		private void interpolationToolStripMenuItem_Click(object sender, EventArgs e) { renderMethod.Interpolation = !renderMethod.Interpolation; }

		#endregion

		#endregion

		#region Tools

		private void romInformationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!RomInformationForm.Visible)
				RomInformationForm.Show(this);
		}

		private void debuggerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!DebuggerForm.Visible)
				DebuggerForm.Show(this);
		}

		private void tileViewerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!TileViewerForm.Visible)
				TileViewerForm.Show(this);
		}

		private void mapViewerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!MapViewerForm.Visible)
				MapViewerForm.Show(this);
		}

		#endregion

		#endregion

		private void limitSpeedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Settings.Default.LimitSpeed = 
				emulatedGameBoy.EnableFramerateLimiter = limitSpeedToolStripMenuItem.Checked;
		}

		#endregion
	}
}
