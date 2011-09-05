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
using SlimDX.Multimedia;
using SlimDX.XAudio2;
using System.IO;

namespace CrystalBoy.Emulator.Rendering.SlimDX
{
	public sealed class XAudio2Renderer : IDisposable
	{
		private byte[] rawBuffer;
		private WaveFormat waveFormat;
		private XAudio2 xAudio;
		private MasteringVoice masteringVoice;
		private SourceVoice sourceVoice;
		private AudioBuffer audioBuffer;

		public XAudio2Renderer()
		{
			waveFormat = new WaveFormat();
			waveFormat.AverageBytesPerSecond = (waveFormat.BitsPerSample = 16) / 8 * (waveFormat.SamplesPerSecond = (waveFormat.Channels = 2) * 44100);
			rawBuffer = new byte[waveFormat.BitsPerSample / 8 * waveFormat.Channels * (2 * 44100 / 60)];
			xAudio = new XAudio2(XAudio2Flags.None, ProcessorSpecifier.AnyProcessor);
			masteringVoice = new MasteringVoice(xAudio, 2, 44100);
			sourceVoice = new SourceVoice(xAudio, waveFormat);
			audioBuffer = new AudioBuffer()
			{
				AudioData = new MemoryStream(rawBuffer, false),
				AudioBytes = rawBuffer.Length,
				LoopBegin = 0,
				LoopLength = 2 * 44100 / 60,
				LoopCount = XAudio2.LoopInfinite
			};
		}

		public void Dispose()
		{
			xAudio.StopEngine();
		}
	}
}
