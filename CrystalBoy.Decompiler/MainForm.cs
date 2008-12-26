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
using System.Windows.Forms;
using CrystalBoy.Core;
using CodeMap = CrystalBoy.Decompiler.Analyzer.CodeMap;

namespace CrystalBoy.Decompiler
{
	partial class MainForm : Form
	{
		MemoryBlock rom;
		IMemory memory;
		CodeMap codeMap;

		public MainForm()
		{
			InitializeComponent();
		}

		private void LoadRom(string fileName)
		{
			FileInfo fileInfo = new FileInfo(fileName);

			// Open only existing rom files
			if (!fileInfo.Exists)
				throw new FileNotFoundException();
			// Limit the rom size to 4 Mb
			if (fileInfo.Length > 4 * 1024 * 1024)
				throw new FileTooLongException();
			rom = MemoryUtility.ReadFile(fileInfo);
			memory = new FlexibleGameBoyMemory(rom);
			disassemblyView.Memory = memory;
			codeMap = new CodeMap(rom);
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
				LoadRom(openFileDialog.FileName);
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}
	}
}
