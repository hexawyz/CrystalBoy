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
using System.ComponentModel;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Multimedia;
using SlimDX.XAudio2;
using XAudioBuffer = SlimDX.XAudio2.AudioBuffer; // Avoid confusion between SlimDX.XAudio2.AudioBuffer and CrystalBoy.Emulation.AudioBuffer
using CrystalBoy.Emulation;

namespace CrystalBoy.Emulator.Rendering.SlimDX
{
	[DisplayName("XAudio 2")]
	[Description("Renders audio using XAudio 2 / SlimDX.")]
	public sealed class XAudio2Renderer : CrystalBoy.Emulation.AudioRenderer
	{
		private WaveFormat waveFormat;
		private XAudio2 xAudio;
		private MasteringVoice masteringVoice;
		private SourceVoice sourceVoice;
		private XAudioBuffer audioBuffer;

		public unsafe XAudio2Renderer()
		{
			waveFormat = new WaveFormat();
			waveFormat.FormatTag = WaveFormatTag.Pcm;
			xAudio = new XAudio2(XAudio2Flags.None, ProcessorSpecifier.AnyProcessor);
			masteringVoice = new MasteringVoice(xAudio, 2, 44100);
		}

		public override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (sourceVoice != null)
				{
					sourceVoice.FlushSourceBuffers();
					sourceVoice.Stop();
					sourceVoice.Dispose();
					sourceVoice = null;
				}
				if (audioBuffer != null)
				{
					audioBuffer.AudioData.Dispose();
					audioBuffer.AudioData = null; // Just to be clean…
					audioBuffer.Dispose();
					audioBuffer = null;
				}
				if (xAudio != null)
				{
					xAudio.StopEngine();
					xAudio.Dispose();
					xAudio = null;
				}
			}
		}

		protected override void BeginBufferChange()
		{
			if (audioBuffer != null && audioBuffer.AudioData != null)
			{
				sourceVoice.FlushSourceBuffers();
				sourceVoice.Stop();
				sourceVoice.Dispose();
				sourceVoice = null;
				audioBuffer.AudioData.Dispose();
				audioBuffer.AudioData = null;
			}
		}

		protected override void EndBufferChange()
		{
			if (AudioBuffer != null)
			{
				if (audioBuffer == null) audioBuffer = new XAudioBuffer();

				audioBuffer.AudioData = new DataStream(AudioBuffer.RawBuffer, true, true);
				audioBuffer.AudioBytes = (int)audioBuffer.AudioData.Length;
				audioBuffer.LoopLength = AudioBuffer.RawBuffer.Length / 2;
				audioBuffer.LoopCount = XAudio2.LoopInfinite;
				waveFormat.AverageBytesPerSecond = (waveFormat.BlockAlignment = (short)((waveFormat.BitsPerSample = (short)BitsPerSample) / 8 * (waveFormat.Channels = 2))) * (waveFormat.SamplesPerSecond = Frequency);
				sourceVoice = new SourceVoice(xAudio, waveFormat);
				sourceVoice.SubmitSourceBuffer(audioBuffer);
				sourceVoice.Start();
			}
		}
	}
}
