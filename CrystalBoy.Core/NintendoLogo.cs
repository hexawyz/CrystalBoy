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
using System.Collections.Generic;
using System.Text;

namespace CrystalBoy.Core
{
	public static class NintendoLogo
	{
		internal static readonly byte[] Data = Load();

		private static byte[] Load()
		{
			using (var stream = typeof(NintendoLogo).Assembly.GetManifestResourceStream(typeof(NintendoLogo), "NintendoLogo"))
			{
				byte[] data = new byte[stream.Length];

				stream.Read(data, 0, data.Length);

				return data;
			}
		}

		// A static indexer would do a better job, but there is no such thing in C#… :(
		public static byte Get(int index) { return Data[index]; }
	}
}
