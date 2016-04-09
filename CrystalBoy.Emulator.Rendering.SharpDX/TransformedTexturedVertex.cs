#if false

using System;
using System.Collections.Generic;
using System.Text;
using SharpDX;

namespace CrystalBoy.Emulator.Rendering.SharpDX
{
	internal struct TransformedTexturedVertex
	{
		public Vector4 Position;
		public Vector2 TextureCoordinates;

		public TransformedTexturedVertex(Vector4 position, Vector2 textureCoordinates)
		{
			Position = position;
			TextureCoordinates = textureCoordinates;
		}

		public const int StrideSize = 4 * sizeof(float) + 2 * sizeof(float);
	}
}

#endif
