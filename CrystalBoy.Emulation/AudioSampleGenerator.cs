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
using System.ComponentModel;

namespace CrystalBoy.Emulation
{
	public abstract class AudioSampleGenerator
	{
		internal AudioSampleGenerator()
		{
			var displayNameAttributes = GetType().GetCustomAttributes(typeof(DisplayNameAttribute), false) as DisplayNameAttribute[];

			DisplayName = string.Intern(displayNameAttributes.Length > 0 ? displayNameAttributes[0].DisplayName : GetType().Name);
		}

		public abstract void FillBuffer();

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal abstract AudioBuffer GetAudioBuffer();

		public string DisplayName { get; private set; }
	}

	public abstract class AudioSampleGenerator<TSample> : AudioSampleGenerator
		where TSample : struct
	{
		public AudioSampleGenerator(AudioBuffer<TSample> audioBuffer) { AudioBuffer = audioBuffer; }

		public AudioBuffer<TSample> AudioBuffer { get; private set; }

		internal sealed override AudioBuffer GetAudioBuffer() { return AudioBuffer; }
	}
}
