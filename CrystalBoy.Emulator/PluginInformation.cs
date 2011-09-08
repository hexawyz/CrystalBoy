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

namespace CrystalBoy.Emulator
{
	internal struct PluginInformation
	{
		public readonly Type Type;
		public readonly string DisplayName;
		public readonly string Description;

		internal PluginInformation(Type type)
		{
			var displayNameAttributes = type.GetCustomAttributes(typeof(DisplayNameAttribute), false) as DisplayNameAttribute[];
			var descriptionAttributes = type.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

			Type = type;
			DisplayName = string.Intern(displayNameAttributes.Length > 0 ? displayNameAttributes[0].DisplayName : type.Name);
			Description = descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : null;
		}

		internal PluginInformation(Type type, string displayName, string description)
		{
			Type = type;
			DisplayName = displayName;
			Description = description;
		}
	}
}
