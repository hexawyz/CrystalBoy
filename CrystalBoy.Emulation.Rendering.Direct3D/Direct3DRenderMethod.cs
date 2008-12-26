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
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace CrystalBoy.Emulation.Rendering.Direct3D
{
	[DisplayName("Direct3D")]
	public sealed class Direct3DRenderMethod : RenderMethod<Control>
	{
		PresentParameters presentParameters;
		Device device;
		Texture texture;
		VertexBuffer vertexBuffer;
		GraphicsStream graphicsStream;
		TextureFilter textureFilter;

		public Direct3DRenderMethod(Control renderObject)
			: base(renderObject)
		{
			ResetTextureFilter();
			FillPresentParameters();
			InitializeDevice();
			InitializeVertexBuffer();
			InitializeTexture();
		}

		public override void Dispose()
		{
			DisposeDevice();
			DisposeVertexBuffer();
			DisposeTexture();
		}

		private void FillPresentParameters()
		{
			presentParameters = new PresentParameters();

			presentParameters.BackBufferFormat = Format.X8R8G8B8;
			presentParameters.PresentationInterval = PresentInterval.Immediate;
			presentParameters.PresentFlag = PresentFlag.None;
			presentParameters.SwapEffect = SwapEffect.Discard;
			presentParameters.Windowed = true;
			presentParameters.DeviceWindow = RenderObject;
		}

		private void InitializeDevice()
		{
			device = new Device(0, DeviceType.Hardware, RenderObject, CreateFlags.HardwareVertexProcessing, presentParameters);
			device.DeviceReset += new EventHandler(OnDeviceReset);
		}

		private void OnDeviceReset(object sender, EventArgs e)
		{
			InitializeVertexBuffer();
			device.SetTexture(0, texture);
			device.SetSamplerState(0, SamplerStageStates.MagFilter, (int)textureFilter);
			device.SetSamplerState(0, SamplerStageStates.MinFilter, (int)textureFilter);
		}

		private void DisposeDevice()
		{
			if (!device.Disposed)
				device.Dispose();
		}

		private void InitializeVertexBuffer()
		{
			vertexBuffer = new VertexBuffer(device, CustomVertex.TransformedTextured.StrideSize * 4, Usage.None, CustomVertex.TransformedTextured.Format, Pool.Default);
			vertexBuffer.SetData(new CustomVertex.TransformedTextured[] {
				new CustomVertex.TransformedTextured(device.Viewport.X - 0.5f, device.Viewport.Y - 0.5f, 0, 1, 0, 0),
				new CustomVertex.TransformedTextured(device.Viewport.Width - 0.5f, device.Viewport.Y - 0.5f, 0, 1, 1, 0),
				new CustomVertex.TransformedTextured(device.Viewport.X - 0.5f, device.Viewport.Height - 0.5f, 0, 1, 0, 1),
				new CustomVertex.TransformedTextured(device.Viewport.Width - 0.5f, device.Viewport.Height - 0.5f, 0, 1, 1, 1) }, 0, LockFlags.Discard);
			device.SetStreamSource(0, vertexBuffer, 0, CustomVertex.TransformedTextured.StrideSize);
			device.VertexFormat = CustomVertex.TransformedTextured.Format;
		}

		private void DisposeVertexBuffer()
		{
			if (!vertexBuffer.Disposed)
				vertexBuffer.Dispose();
		}

		private void InitializeTexture()
		{
			texture = new Texture(device, 160, 144, 1, Usage.None, Format.X8R8G8B8, Pool.Managed);
			device.SetTexture(0, texture);
			device.SetSamplerState(0, SamplerStageStates.MagFilter, (int)textureFilter);
			device.SetSamplerState(0, SamplerStageStates.MinFilter, (int)textureFilter);
		}

		private void DisposeTexture()
		{
			if (!texture.Disposed)
				texture.Dispose();
		}

		public override unsafe void* LockBuffer(out int stride)
		{
			graphicsStream = texture.LockRectangle(0, LockFlags.Discard, out stride);

			return graphicsStream.InternalDataPointer;
		}

		public override void UnlockBuffer()
		{
			texture.UnlockRectangle(0);
		}

		private void ResetTextureFilter()
		{
			if (Interpolation)
				textureFilter = TextureFilter.Linear;
			else
				textureFilter = TextureFilter.Point;
			if (device != null && device.CheckCooperativeLevel())
			{
				device.SetSamplerState(0, SamplerStageStates.MagFilter, (int)textureFilter);
				device.SetSamplerState(0, SamplerStageStates.MinFilter, (int)textureFilter);
			}
		}

		protected override void OnInterpolationChanged(EventArgs e)
		{
			ResetTextureFilter();
			base.OnInterpolationChanged(e);
		}

		public override void Render()
		{
			device.BeginScene();
			device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
			device.EndScene();
			device.Present();
		}
	}
}
