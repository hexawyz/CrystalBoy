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
using System.Reflection;
using System.Xml;
using System.Collections.Generic;
using System.Text;

namespace CrystalBoy.Core
{
	public static class MakerDictionary
	{
		private static readonly Dictionary<string, string> makerDictionary = ReadDictionary();

		private static Dictionary<string, string> ReadDictionary()
		{
			Dictionary<string, string> makerDictionary = new Dictionary<string, string>();
			XmlReader xmlReader = XmlReader.Create(
				typeof(MakerDictionary).Assembly.GetManifestResourceStream(typeof(MakerDictionary), "makers.xml"),
				new XmlReaderSettings()
				{
					IgnoreWhitespace = true,
					IgnoreComments = true,
					CloseInput = true,
					ConformanceLevel = ConformanceLevel.Document
				});

			xmlReader.Read();
			xmlReader.ReadStartElement("Makers");

			if (xmlReader.IsEmptyElement)
				goto Return;

			xmlReader.MoveToContent();

			while (xmlReader.NodeType == XmlNodeType.Element)
			{
				string code, name;

				if (xmlReader.Name != "Maker")
					if (!xmlReader.ReadToNextSibling("Maker"))
						continue;

				xmlReader.MoveToAttribute("Code");
				xmlReader.ReadAttributeValue();
				code = string.Intern(xmlReader.Value);
				xmlReader.MoveToElement();
				xmlReader.ReadStartElement("Maker");
				xmlReader.MoveToContent();
				name = string.Intern(xmlReader.ReadContentAsString());
				xmlReader.ReadEndElement();

				makerDictionary.Add(code, name);
			}

			xmlReader.ReadEndElement();

		Return:
			xmlReader.Close();
			return makerDictionary;
		}

		public static string GetMakerName(string makerCode)
		{
			string result;

			if (makerDictionary.TryGetValue(makerCode, out result))
				return result;
			else
				return "?";
		}
	}
}
