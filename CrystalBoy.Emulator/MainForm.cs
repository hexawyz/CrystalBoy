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
		private DebuggerForm debuggerForm;
		private TileViewerForm tileViewerForm;
		private MapViewerForm mapViewerForm;
		private RomInformationForm romInformationForm;
		private EmulatedGameBoy emulatedGameBoy;
		private VideoRenderer videoRenderer;
		private AudioRenderer audioRenderer;
		private Dictionary<Type, ToolStripMenuItem> videoRendererMenuItemDictionary;
		private Dictionary<Type, ToolStripMenuItem> audioRendererMenuItemDictionary;
		private BinaryWriter ramSaveWriter;
		private BinaryReader ramSaveReader;
		private bool pausedTemporarily;

		#region Constructor and Initialization

		public MainForm()
		{
			InitializeComponent();
			if (components == null) components = new System.ComponentModel.Container();
			emulatedGameBoy = new EmulatedGameBoy(components);
			emulatedGameBoy.TryUsingBootRom = Settings.Default.UseBootstrapRom;
			emulatedGameBoy.EnableFramerateLimiter = Settings.Default.LimitSpeed;
			emulatedGameBoy.RomChanged += OnRomChanged;
			emulatedGameBoy.AfterReset += OnAfterReset;
			emulatedGameBoy.EmulationStatusChanged += OnEmulationStatusChanged;
			emulatedGameBoy.NewFrame += OnNewFrame;
			emulatedGameBoy.BorderChanged += OnBorderChanged;
			try { emulatedGameBoy.Reset(Settings.Default.HardwareType); }
			catch (ArgumentOutOfRangeException) { Settings.Default.HardwareType = emulatedGameBoy.HardwareType; }
			AdjustSize(Settings.Default.ContentSize);
			UpdateEmulationStatus();
			UpdateFrameRate();
			SetStatusTextHandler();
			CreateRendererMenuItems();
		}

		private void CreateRendererMenuItems()
		{
			videoRendererMenuItemDictionary = new Dictionary<Type, ToolStripMenuItem>();
			audioRendererMenuItemDictionary = new Dictionary<Type, ToolStripMenuItem>();

			foreach (var plugin in Program.PluginCollection)
			{
				bool isAudioRenderer = plugin.Type.IsSubclassOf(typeof(AudioRenderer));

				// Skip the plugins which are neither AudioRenderer nor VideoRenderer, for future-proofing the code a little bit.
				if (!(isAudioRenderer || plugin.Type.IsSubclassOf(typeof(VideoRenderer)))) continue;

				var rendererMenuItem = new ToolStripMenuItem(plugin.DisplayName);

				rendererMenuItem.Click += isAudioRenderer ? new EventHandler(audioRendererMenuItem_Click) : new EventHandler(videoRendererMenuItem_Click);
				rendererMenuItem.ToolTipText = plugin.Description;
				rendererMenuItem.Tag = plugin.Type;

				(isAudioRenderer ? audioRendererMenuItemDictionary : videoRendererMenuItemDictionary).Add(plugin.Type, rendererMenuItem);

				(isAudioRenderer ? audioRendererToolStripMenuItem : videoRendererToolStripMenuItem).DropDownItems.Add(rendererMenuItem);
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

		#region Audio Renderer Management

		private AudioRenderer CreateAudioRenderer(Type rendererType)
		{
			Type[] constructorParameterTypes;

			// Assume the plugin filter has been passed at loading (it should have !)
			if (rendererType.IsGenericType)
				rendererType = rendererType.MakeGenericType(constructorParameterTypes = new[] { typeof(Control) });
			else if (rendererType.IsSubclassOf(typeof(AudioRenderer<Control>)))
				constructorParameterTypes = new[] { typeof(Control) };
			else if (rendererType.IsSubclassOf(typeof(AudioRenderer<IWin32Window>)))
				constructorParameterTypes = new[] { typeof(IWin32Window) };
			else constructorParameterTypes = Type.EmptyTypes;

			// Not using Activator.CreateInstance here because it doesn't allow to check the exact signature… (Does it really matters ? Somehow…)
			ConstructorInfo constructor = rendererType.GetConstructor(constructorParameterTypes);

			return (AudioRenderer)constructor.Invoke(constructorParameterTypes.Length > 0 ? new[] { toolStripContainer.ContentPanel } : new object[0]);
		}

		private void SwitchAudioRenderer(Type rendererType)
		{
			if (audioRenderer != null)
			{
				emulatedGameBoy.Bus.AudioRenderer = null;
				audioRenderer.Dispose();
				audioRenderer = null;
			}

			audioRenderer = CreateAudioRenderer(rendererType);

			ToolStripMenuItem selectedRendererMenuItem = audioRendererMenuItemDictionary[rendererType];

			foreach (ToolStripMenuItem renderMethodMenuItem in audioRendererMenuItemDictionary.Values)
				renderMethodMenuItem.Checked = renderMethodMenuItem == selectedRendererMenuItem;

			// Store the FullName once we know the type of render method to use
			Settings.Default.AudioRenderer = rendererType.FullName; // Don't use AssemblyQualifiedName for easing updates, though it should be a better choice

			emulatedGameBoy.Bus.AudioRenderer = audioRenderer;
		}

		#endregion

		#region Video Renderer Management

		private VideoRenderer CreateVideoRenderer(Type rendererType)
		{
			Type[] constructorParameterTypes;

			// Assume the plugin filter has been passed at loading (it should have !)
			if (rendererType.IsGenericType)
				rendererType = rendererType.MakeGenericType(constructorParameterTypes = new[] { typeof(Control) });
			else if (rendererType.IsSubclassOf(typeof(VideoRenderer<Control>)))
				constructorParameterTypes = new[] { typeof(Control) };
			else if (rendererType.IsSubclassOf(typeof(VideoRenderer<IWin32Window>)))
				constructorParameterTypes = new[] { typeof(IWin32Window) };
			else constructorParameterTypes = Type.EmptyTypes;

			// Not using Activator.CreateInstance here because it doesn't allow to check the exact signature… (Does it really matters ? Somehow…)
			ConstructorInfo constructor = rendererType.GetConstructor(constructorParameterTypes);

			return (VideoRenderer)constructor.Invoke(constructorParameterTypes.Length > 0 ? new[] { toolStripContainer.ContentPanel } : new object[0]);
		}

		private void SwitchVideoRenderer(Type rendererType)
		{
			if (videoRenderer != null)
			{
				emulatedGameBoy.Bus.VideoRenderer = null;
				videoRenderer.Dispose();
				videoRenderer = null;
			}

			videoRenderer = CreateVideoRenderer(rendererType);
			videoRenderer.Interpolation = false;
			videoRenderer.BorderVisible = Settings.Default.BorderVisibility == BorderVisibility.On || Settings.Default.BorderVisibility == BorderVisibility.Auto && emulatedGameBoy.HasCustomBorder;

			ToolStripMenuItem selectedRendererMenuItem = videoRendererMenuItemDictionary[rendererType];

			foreach (ToolStripMenuItem renderMethodMenuItem in videoRendererMenuItemDictionary.Values)
				renderMethodMenuItem.Checked = renderMethodMenuItem == selectedRendererMenuItem;

			// Store the FullName once we know the type of render method to use
			Settings.Default.VideoRenderer = rendererType.FullName; // Don't use AssemblyQualifiedName for easing updates, though it should be a better choice

			emulatedGameBoy.Bus.VideoRenderer = videoRenderer;
		}

		#endregion

		private void InitializePlugins()
		{
			string audioRendererName = Settings.Default.AudioRenderer;
			string videoRendererName = Settings.Default.VideoRenderer;

			// Try to find the type by simple reflexion (will work for embedded types and AssemblyQualifiedNames)
			Type audioRendererType = Type.GetType(audioRendererName);
			Type videoRendererType = Type.GetType(videoRendererName);

			Type firstAudioRendererType = null;
			Type firstVideoRendererType = null;

			// If simple reflexion fails, try using Name and FullName matches (there may be multiple matches in that case but it is better to avoid writing the full AssemblyQualifiedName in initial configurtaion files)
			if (audioRendererType == null && videoRendererType == null)
				foreach (var plugin in Program.PluginCollection)
				{
					bool isAudioRenderer = plugin.Type.IsSubclassOf(typeof(AudioRenderer));
					bool isVideoRenderer = !isAudioRenderer && plugin.Type.IsSubclassOf(typeof(VideoRenderer));

					if (audioRendererType == null && isAudioRenderer) audioRendererType = plugin.Type;
					else if (firstVideoRendererType == null && isVideoRenderer) firstVideoRendererType = plugin.Type;

					if (audioRendererType == null && (plugin.Type.Name == audioRendererName || plugin.Type.FullName == audioRendererName))
						audioRendererType = plugin.Type;
					if (videoRendererType == null && (plugin.Type.Name == videoRendererName || plugin.Type.FullName == videoRendererName))
						videoRendererType = plugin.Type;
				}

			audioRendererType = audioRendererType ?? firstAudioRendererType;
			videoRendererType = videoRendererType ?? firstVideoRendererType;

			if (audioRendererType != null) SwitchAudioRenderer(audioRendererType);
			if (videoRendererType != null) SwitchVideoRenderer(videoRendererType);
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

		#region ROM Loading & RAM Saving

		private void UnloadRom()
		{
			emulatedGameBoy.Pause();

			if (ramSaveWriter != null)
			{
				WriteRam();

				ramSaveWriter.Close();
				ramSaveWriter = null;

				if (ramSaveReader != null) ramSaveReader.Close();
				ramSaveReader = null;
			}

			emulatedGameBoy.UnloadRom();
		}

		private void LoadRom(string fileName)
		{
			var romFileInfo = new FileInfo(fileName);

			// Open only existing rom files
			if (!romFileInfo.Exists)
				throw new FileNotFoundException();
			// Limit the rom size to 4 Mb
			if (romFileInfo.Length > 4 * 1024 * 1024)
				throw new InvalidOperationException();

			emulatedGameBoy.LoadRom(MemoryUtility.ReadFile(romFileInfo));

			if (emulatedGameBoy.RomInformation.HasRam && emulatedGameBoy.RomInformation.HasBattery)
			{
				var ramFileInfo = new FileInfo(Path.Combine(romFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(romFileInfo.Name)) + ".sav");

				var ramSaveStream = ramFileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
				ramSaveStream.SetLength(emulatedGameBoy.Mapper.SavedRamSize + (emulatedGameBoy.RomInformation.HasTimer ? 48 : 0));
				ramSaveStream.Read(emulatedGameBoy.ExternalRam, 0, emulatedGameBoy.Mapper.SavedRamSize);
				ramSaveWriter = new BinaryWriter(ramSaveStream);

				if (emulatedGameBoy.RomInformation.HasTimer)
				{
					var mbc3 = emulatedGameBoy.Mapper as CrystalBoy.Emulation.Mappers.MemoryBankController3;

					if (mbc3 != null)
					{
						var rtcState = mbc3.RtcState;
						ramSaveReader = new BinaryReader(ramSaveStream);

						rtcState.Frozen = true;

						rtcState.Seconds = (byte)ramSaveReader.ReadInt32();
						rtcState.Minutes = (byte)ramSaveReader.ReadInt32();
						rtcState.Hours = (byte)ramSaveReader.ReadInt32();
						rtcState.Days = (short)((byte)ramSaveReader.ReadInt32() + ((byte)ramSaveReader.ReadInt32() << 8));

						rtcState.LatchedSeconds = (byte)ramSaveReader.ReadInt32();
						rtcState.LatchedMinutes = (byte)ramSaveReader.ReadInt32();
						rtcState.LatchedHours = (byte)ramSaveReader.ReadInt32();
						rtcState.LatchedDays = (short)((byte)ramSaveReader.ReadInt32() + ((byte)ramSaveReader.ReadInt32() << 8));

						rtcState.DateTime = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(ramSaveReader.ReadInt64());

						rtcState.Frozen = false;
					}
				}

				emulatedGameBoy.Mapper.RamUpdated += Mapper_RamUpdated;
			}

			emulatedGameBoy.Run();
		}

		private void Mapper_RamUpdated(object sender, EventArgs e) { if (ramSaveWriter != null) WriteRam(); }

		private void WriteRam()
		{
			ramSaveWriter.Seek(0, SeekOrigin.Begin);
			ramSaveWriter.Write(emulatedGameBoy.ExternalRam, 0, emulatedGameBoy.Mapper.SavedRamSize);
			if (emulatedGameBoy.RomInformation.HasTimer)
			{
				var mbc3 = emulatedGameBoy.Mapper as CrystalBoy.Emulation.Mappers.MemoryBankController3;

				if (mbc3 != null)
				{
					var rtcState = mbc3.RtcState;

					// I'll save the date using the same format as VBA in order to be more compatible, but i originally planned to store it whithout wasting bytes…
					// Luckily enough, it seems we use the same iternal representation… (But there probably is no other way to do it)

					ramSaveWriter.Write((int)rtcState.Seconds & 0xFF);
					ramSaveWriter.Write((int)rtcState.Minutes & 0xFF);
					ramSaveWriter.Write((int)rtcState.Hours & 0xFF);
					ramSaveWriter.Write((int)rtcState.Days & 0xFF);
					ramSaveWriter.Write((rtcState.Days >> 8) & 0xFF);

					ramSaveWriter.Write((int)rtcState.LatchedSeconds & 0xFF);
					ramSaveWriter.Write((int)rtcState.LatchedMinutes & 0xFF);
					ramSaveWriter.Write((int)rtcState.LatchedHours & 0xFF);
					ramSaveWriter.Write((int)rtcState.LatchedDays & 0xFF);
					ramSaveWriter.Write((rtcState.LatchedDays >> 8) & 0xFF);

					ramSaveWriter.Write((long)((rtcState.DateTime - new DateTime(1970, 1, 1)).TotalSeconds));
				}
			}
		}

		#endregion

		#region Size Management

		#region Border Visibility Management

		private void OnAfterReset(object sender, EventArgs e) { SetBorderVisibility(Settings.Default.BorderVisibility == BorderVisibility.On || Settings.Default.BorderVisibility == BorderVisibility.Auto && emulatedGameBoy.HasCustomBorder); }

		private void OnBorderChanged(object sender, EventArgs e) { if (Settings.Default.BorderVisibility == BorderVisibility.Auto) ShowBorder(); }

		private void SetBorderVisibility(bool visible) { if (visible) ShowBorder(); else HideBorder(); }

		private void ShowBorder()
		{
			if (videoRenderer != null && !videoRenderer.BorderVisible)
			{
				var panelSize = toolStripContainer.ContentPanel.ClientSize;

				videoRenderer.BorderVisible = true;
				AdjustSize(panelSize.Width * 256 / 160, panelSize.Height * 224 / 144);
			}
		}

		private void HideBorder()
		{
			if (videoRenderer != null && videoRenderer.BorderVisible)
			{
				var panelSize = toolStripContainer.ContentPanel.ClientSize;

				videoRenderer.BorderVisible = false;
				AdjustSize(panelSize.Width * 160 / 256, panelSize.Height * 144 / 224);
			}
		}

		#endregion

		private void SetZoomFactor(int factor)
		{
			var referenceSize = videoRenderer.BorderVisible ? new Size(256, 224) : new Size(160, 144);

			if (factor <= 0) throw new ArgumentOutOfRangeException("factor");
			Settings.Default.ZoomFactor = factor;
			AdjustSize(factor * referenceSize.Width, factor * referenceSize.Height);
		}

		private void AdjustSize(Size requestedSize)
		{
			AdjustSize(requestedSize.Width, requestedSize.Height);
		}

		private void AdjustSize(int requestedWidth, int requestedHeight)
		{
			Size newSize, panelSize;

			// First round
			newSize = ClientSize;
			panelSize = toolStripContainer.ContentPanel.ClientSize;
			newSize.Width = newSize.Width - panelSize.Width + requestedWidth;
			newSize.Height = newSize.Height - panelSize.Height + requestedHeight;

			ClientSize = newSize;

			// Second round
			panelSize = toolStripContainer.ContentPanel.ClientSize;
			if (panelSize.Height != requestedHeight)
			{
				newSize.Height = newSize.Height - panelSize.Height + requestedHeight;

				ClientSize = newSize;
			}
		}

		protected override void OnResizeBegin(EventArgs e) { if (pausedTemporarily = emulatedGameBoy.EmulationStatus == EmulationStatus.Running) emulatedGameBoy.Pause(); }

		protected override void OnResizeEnd(EventArgs e) { if (pausedTemporarily) emulatedGameBoy.Run(); }

		#endregion

		#region Status Updates

		private void UpdateEmulationStatus() { emulationStatusToolStripStatusLabel.Text = emulatedGameBoy.EmulationStatus == EmulationStatus.Running ? Resources.RunningText : Resources.PausedText; }

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
			InitializePlugins();
			emulatedGameBoy.Bus.VideoRenderer = videoRenderer;
			base.OnShown(e);
		}

		protected override void OnActivated(EventArgs e)
		{
			if (emulatedGameBoy != null && pausedTemporarily) emulatedGameBoy.Run();
			base.OnActivated(e);
		}

		protected override void OnDeactivate(EventArgs e)
		{
			if (emulatedGameBoy != null && (pausedTemporarily = emulatedGameBoy.EmulationStatus == EmulationStatus.Running)) emulatedGameBoy.Pause();
			base.OnDeactivate(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			// Make sure that the emulated system is stopped before closing
			emulatedGameBoy.Pause();
			UnloadRom();
			// Calling DoEvents here will make sure that everything gets executed in order, but it should work without.
			Application.DoEvents();
			base.OnClosing(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			Size renderSize = toolStripContainer.ContentPanel.ClientSize;

			Settings.Default.ContentSize = Settings.Default.BorderVisibility != BorderVisibility.On && videoRenderer.BorderVisible ? new Size(renderSize.Width * 160 / 256, renderSize.Height * 144 / 224) : renderSize;
			Settings.Default.Save();
			base.OnClosed(e);
		}

		private void OnRomChanged(object sender, EventArgs e) { SetBorderVisibility(Settings.Default.BorderVisibility == BorderVisibility.On || Settings.Default.BorderVisibility == BorderVisibility.Auto && emulatedGameBoy.HasCustomBorder); }

		private void OnEmulationStatusChanged(object sender, EventArgs e) { UpdateEmulationStatus(); }

		private unsafe void OnNewFrame(object sender, EventArgs e) { UpdateFrameRate(); }

		private void toolStripContainer_ContentPanel_Paint(object sender, PaintEventArgs e) { videoRenderer.Render(); }

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
			{
				UnloadRom();
				LoadRom(openFileDialog.FileName);
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e) { Close(); }

		#endregion

		#region Emulation

		private void emulationToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			pauseToolStripMenuItem.Enabled = emulatedGameBoy.EmulationStatus != EmulationStatus.Stopped;
			resetToolStripMenuItem.Enabled = emulatedGameBoy.EmulationStatus != EmulationStatus.Stopped;
			pauseToolStripMenuItem.Checked = emulatedGameBoy.EmulationStatus == EmulationStatus.Paused;
			limitSpeedToolStripMenuItem.Checked = emulatedGameBoy.EnableFramerateLimiter;
		}

		private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (emulatedGameBoy.EmulationStatus == EmulationStatus.Paused) emulatedGameBoy.Run();
			else if (emulatedGameBoy.EmulationStatus == EmulationStatus.Running) emulatedGameBoy.Pause();
		}

		private void runFrameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (emulatedGameBoy.EmulationStatus == EmulationStatus.Running) emulatedGameBoy.Pause();
			if (emulatedGameBoy.EmulationStatus == EmulationStatus.Paused) emulatedGameBoy.RunFrame();
		}

		private void resetToolStripMenuItem_Click(object sender, EventArgs e) { emulatedGameBoy.Reset(); }

		private void limitSpeedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Settings.Default.LimitSpeed =
				emulatedGameBoy.EnableFramerateLimiter = limitSpeedToolStripMenuItem.Checked;
		}

		#region Video

		private void videoToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			interpolationToolStripMenuItem.Enabled = videoRenderer.SupportsInterpolation;
			interpolationToolStripMenuItem.Checked = videoRenderer.Interpolation;
		}

		private void audioRendererMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem renderMethodMenuItem = (ToolStripMenuItem)sender;
		}

		private void videoRendererMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem renderMethodMenuItem = (ToolStripMenuItem)sender;

			SwitchVideoRenderer((Type)renderMethodMenuItem.Tag);
		}

		#region Border

		private void borderToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			var borderVisibility = Settings.Default.BorderVisibility;

			borderAutoToolStripMenuItem.Checked = borderVisibility == BorderVisibility.Auto;
			borderOnToolStripMenuItem.Checked = borderVisibility == BorderVisibility.On;
			borderOffToolStripMenuItem.Checked = borderVisibility == BorderVisibility.Off;
		}
		
		private void borderAutoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Settings.Default.BorderVisibility = BorderVisibility.Auto;
			SetBorderVisibility(emulatedGameBoy.HasCustomBorder);
		}

		private void borderOnToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Settings.Default.BorderVisibility = BorderVisibility.On;
			ShowBorder();
		}

		private void borderOffToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Settings.Default.BorderVisibility = BorderVisibility.Off;
			HideBorder();
		}

		#endregion

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
			else if (renderSize == new Size(640, 576))
				checkItem = zoom400toolStripMenuItem;

			foreach (ToolStripMenuItem zoomMenuItem in zoomToolStripMenuItem.DropDownItems)
				zoomMenuItem.Checked = zoomMenuItem == checkItem;
		}

		private void zoom100toolStripMenuItem_Click(object sender, EventArgs e) { SetZoomFactor(1); }

		private void zoom200toolStripMenuItem_Click(object sender, EventArgs e) { SetZoomFactor(2); }

		private void zoom300toolStripMenuItem_Click(object sender, EventArgs e) { SetZoomFactor(3); }

		private void zoom400toolStripMenuItem_Click(object sender, EventArgs e) { SetZoomFactor(4); }

		#endregion

		private void interpolationToolStripMenuItem_Click(object sender, EventArgs e) { videoRenderer.Interpolation = !videoRenderer.Interpolation; }

		#endregion

		#region Hardware

		private void hardwareToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			useBootstrapRomToolStripMenuItem.Checked = emulatedGameBoy.TryUsingBootRom;
			// The tags have been set by manually editing the designed form code.
			// This should work fine as long as the tag isn't modified in the Windows Forms editor.
			foreach (ToolStripItem item in hardwareToolStripMenuItem.DropDownItems)
				if (item.Tag is HardwareType)
					(item as ToolStripMenuItem).Checked = (HardwareType)item.Tag == emulatedGameBoy.HardwareType;
		}

		private void useBootstrapRomToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Settings.Default.UseBootstrapRom = emulatedGameBoy.TryUsingBootRom = !emulatedGameBoy.TryUsingBootRom;
			if (!emulatedGameBoy.RomLoaded || emulatedGameBoy.EmulationStatus == EmulationStatus.Stopped)
				emulatedGameBoy.Reset();
		}

		private void randomHardwareToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var menuItem = sender as ToolStripMenuItem;

			if (menuItem.Tag is HardwareType) SwitchHardware((HardwareType)menuItem.Tag);
			else throw new InvalidOperationException();
		}

		private void SwitchHardware(HardwareType hardwareType)
		{
			if (!emulatedGameBoy.RomLoaded || MessageBox.Show(this, Resources.EmulationResetMessage, Resources.GenericMessageTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				emulatedGameBoy.Reset(Settings.Default.HardwareType = hardwareType);
		}

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

		#region Help

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e) { AboutForm.Default.ShowDialog(this); }

		#endregion

		#endregion

		#endregion
	}
}
