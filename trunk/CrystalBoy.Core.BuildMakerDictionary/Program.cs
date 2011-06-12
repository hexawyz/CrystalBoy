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
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace CrystalBoy.Core.BuildMakerDictionary
{
	class Program
	{
		static readonly Regex parsingRegex = new Regex("^(?<code>.{2}) = (?<name>([^ \t]+ ){0,3}[^ \t]+)[ \t]?", RegexOptions.Compiled | RegexOptions.Multiline);
		static readonly string sourceUrl = "http://www.ndsretro.com/download/Nintendo_Companys.txt";

		static void Main(string[] args)
		{
			Dictionary<string, string> makerDictionary = new Dictionary<string, string>();
			XmlWriter xmlWriter;
			WebClient webClient = new WebClient();
			string text;

			// This code produce an XML file containing maker entries found in the file
			// Since the file is mainly for human reading, additional *manual* post-processing must be done for the file to be really usable
			// Entries named ??? and such must be removed from the file, and many uncertain entries (see the original file for reference) must be manually checked
			try
			{
				text = webClient.DownloadString(sourceUrl);

				xmlWriter = XmlWriter.Create("makers.xml",
					new XmlWriterSettings()
					{
						Indent = true,
						CloseOutput = true,
						ConformanceLevel = ConformanceLevel.Document,
						IndentChars = "\t",
						NewLineHandling = NewLineHandling.Entitize,
						Encoding = Encoding.UTF8,
						CheckCharacters = true,
						NewLineOnAttributes = false,
						OmitXmlDeclaration = false
					});

				xmlWriter.WriteStartDocument();
				xmlWriter.WriteStartElement("Makers");

				foreach (Match match in parsingRegex.Matches(text, 0))
				{
					xmlWriter.WriteStartElement("Maker");
					xmlWriter.WriteStartAttribute("Code");
					xmlWriter.WriteString(match.Groups["code"].Value);
					xmlWriter.WriteEndAttribute();
					xmlWriter.WriteString(match.Groups["name"].Value);
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndDocument();

				xmlWriter.Close();
			}
			catch
			{
				throw;
			}
		}
	}
}
