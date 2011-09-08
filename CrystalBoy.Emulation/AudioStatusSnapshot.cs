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
using CrystalBoy.Core;

namespace CrystalBoy.Emulation
{
	internal sealed class AudioStatusSnapshot
	{
		private GameBoyMemoryBus bus;
		private MemoryBlock wavePatternMemoryBlock;

		public unsafe AudioStatusSnapshot(GameBoyMemoryBus bus)
		{
			this.bus = bus;
			this.wavePatternMemoryBlock = new MemoryBlock(16);
			this.WavePatternMemory = (byte*)this.wavePatternMemoryBlock.Pointer;
		}

		public void Capture()
		{
			SoundMasterEnable = bus.SoundEnabled;
		}

		public byte NR10;
		public byte NR11;
		public ushort Channel1_SweepInternalFrequency;
		public ushort Channel1_Frequency;
		public ushort Channel1_Enveloppe;
		public ushort Channel2_Frequency;
		public byte NR50;
		public byte NR51;
		public bool SoundMasterEnable;

		public unsafe readonly byte* WavePatternMemory;
	}
}
