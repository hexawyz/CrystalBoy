using System;
using System.ComponentModel;

namespace CrystalBoy.Emulator
{
	internal struct PluginInformation
	{
		public readonly PluginKind Kind;
		public readonly Type Type;
		public readonly string DisplayName;
		public readonly string Description;

		public PluginInformation(PluginKind kind, Type type)
		{
			var displayNameAttributes = type.GetCustomAttributes(typeof(DisplayNameAttribute), false) as DisplayNameAttribute[];
			var descriptionAttributes = type.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

			Kind = kind;
			Type = type;
			DisplayName = string.Intern(displayNameAttributes.Length > 0 ? displayNameAttributes[0].DisplayName : type.Name);
			Description = descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : null;
		}

		public PluginInformation(PluginKind kind, Type type, string displayName, string description)
		{
			Kind = kind;
			Type = type;
			DisplayName = displayName;
			Description = description;
		}
	}
}
