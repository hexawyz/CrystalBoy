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
using System.Security.Cryptography;
using CrystalBoy.Core;
using System.ComponentModel;

namespace CrystalBoy.Emulation
{
	partial class GameBoyMemoryBus
	{
		private AudioBuffer audioBuffer;
		private AudioSampleGenerator audioSampleGenerator;
		private AudioRenderer audioRenderer;
		private bool audioRenderingEnabled;

		internal ushort Channel1_SweepInternalFrequency;
		internal ushort Channel1_Frequency;
		internal ushort Channel1_Enveloppe;
		internal ushort Channel2_Frequency;

		partial void InitializeAudio()
		{
			audioRenderingEnabled = true;
			audioBuffer = new AudioBuffer<short>(new short[2 * 2 * 44100 / 60]);
			audioSampleGenerator = new Int16AudioSampleGenerator(audioBuffer as AudioBuffer<short>);
		}

		public AudioBuffer AudioBuffer { get { return audioBuffer; } }

		public AudioRenderer AudioRenderer
		{
			get { return audioRenderer; }
			set
			{
				if (value != audioRenderer)
				{
					if (audioRenderer != null) audioRenderer.AudioBuffer = null;

					if ((audioRenderer = value) != null)
					{
						audioRenderer.AudioBuffer = audioBuffer;
					}
				}
			}
		}

		public bool AudioRenderingEnabled
		{
			get { return audioRenderingEnabled; }
			set { audioRenderingEnabled = value; }
		}

		public bool SoundEnabled { get { return (ReadPort(Port.NR52) & 0x80) != 0; } }

		private void RenderAudioFrame()
		{
			if (!audioRenderingEnabled) return;

			if (audioSampleGenerator != null) audioSampleGenerator.FillBuffer();
		}

		[DisplayName("16-bit Sound")]
		private sealed class Int16AudioSampleGenerator : AudioSampleGenerator<Int16>
		{
			Random random = new Random();

			public Int16AudioSampleGenerator(AudioBuffer<short> audioBuffer) : base(audioBuffer) { }

			public override void FillBuffer()
			{
				bool soundEnabled = false;
				byte length1, length2, length3, length4;
				ushort frequency1, frequency2, frequency3;

				int audioFrameDuration = 44100 / 60;

				for (int i = 0; i < audioFrameDuration; i++)
				{
					int maxCycle = i * FrameDuration / audioFrameDuration;

					if (!soundEnabled)
					{
						short stupid = (short)random.Next(-8188, 8189);
						AudioBuffer.PutSample(stupid, stupid);
						continue;
					}

					// For easy mixing, the amplitude for any of those channels must be constrained between -8188 and 8188
					short sample1 = 0, sample2 = 0, sample3 = 0, sample4 = 0; // Fill the buffer with dummy sound for testing the plugin…

					AudioBuffer.PutSample((short)(sample1 + sample2 + sample3 + sample4), (short)(sample1 + sample2 + sample3 + sample4));
				}
			}
		}
	}
}
