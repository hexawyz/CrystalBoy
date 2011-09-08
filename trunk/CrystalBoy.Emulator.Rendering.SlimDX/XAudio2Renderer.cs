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

namespace CrystalBoy.Emulator.Rendering.SlimDX
{
	[DisplayName("XAudio 2")]
	[Description("Renders audio using XAudio 2 / SlimDX.")]
	public sealed class XAudio2Renderer<TElement> : CrystalBoy.Emulation.AudioRenderer<TElement>
		where TElement : struct
	{
		private TElement[] rawBuffer;
		private WaveFormat waveFormat;
		private XAudio2 xAudio;
		private MasteringVoice masteringVoice;
		private SourceVoice sourceVoice;
		private AudioBuffer audioBuffer;

		public unsafe XAudio2Renderer()
		{
			waveFormat = new WaveFormat();
			waveFormat.AverageBytesPerSecond = (waveFormat.BitsPerSample = (short)(Marshal.SizeOf(typeof(TElement)) * 8)) / 8 * (waveFormat.SamplesPerSecond = (waveFormat.Channels = 2) * 44100);
			xAudio = new XAudio2(XAudio2Flags.None, ProcessorSpecifier.AnyProcessor);
			masteringVoice = new MasteringVoice(xAudio, 2, 44100);
			sourceVoice = new SourceVoice(xAudio, waveFormat);
			ResetBuffer(null);
		}

		public override void Dispose()
		{
			sourceVoice.FlushSourceBuffers();
			audioBuffer.Dispose();
			sourceVoice.Dispose();
			xAudio.StopEngine();
		}

		private static TElement[] CreateDefaultBuffer() { return new TElement[2 * (2 * 44100 / 60)]; }

		private void ResetBuffer(TElement[] value)
		{
			if (audioBuffer == null) audioBuffer = new AudioBuffer();
			else if (audioBuffer.AudioData != null)
			{
				sourceVoice.FlushSourceBuffers();
				audioBuffer.AudioData.Dispose();
				audioBuffer.AudioData = null;
			}

			rawBuffer = value ?? CreateDefaultBuffer();

			audioBuffer.AudioData = new DataStream(rawBuffer, true, true);
			audioBuffer.AudioBytes = (int)audioBuffer.AudioData.Length;
			audioBuffer.LoopLength = rawBuffer.Length / 2;
			audioBuffer.LoopCount = XAudio2.LoopInfinite;
			sourceVoice.SubmitSourceBuffer(audioBuffer);
		}

		public override TElement[] RawBuffer
		{
			get { return rawBuffer; }
			set { ResetBuffer(value); }
		}

		public override bool CanSetRawBuffer { get { return true; } }
	}
}
