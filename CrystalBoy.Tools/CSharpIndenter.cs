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
using System.Collections.Generic;
using System.Text;

namespace CrystalBoy.Tools
{
	public static class CSharpIndenter
	{
		/// <summary>Indent simple C# Source Code</summary>
		/// <param name="input">C# Source Code to indent</param>
		/// <returns>Indented C# Source Code</returns>
		public static string IndentSource(string input)
		{
			var output = new StringBuilder();

			IndentSource(input, output);

			return output.ToString();
		}

		/// <summary>Indent simple C# Source Code</summary>
		/// <param name="input">C# Source Code to indent</param>
		/// <returns>Indented C# Source Code</returns>
		public static void IndentSource(string input, StringBuilder output)
		{
			var lines = input.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
			var indentationLevelStack = new Stack<int>();
			int currentIndentationLevel = 0;
			int	nextIndentationLevel = 0;
			bool unindentAfter = false;

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];

				if (line != null && line.Length > 0) line = line.Trim();

				currentIndentationLevel = nextIndentationLevel;

				if (line != null && line.Length > 0)
				{
					char c1 = line[0];
					char c2 = line[line.Length - 1];

					if (c1 == '#') currentIndentationLevel = 0;
					else if (c2 == '{')
					{
						if (unindentAfter)
						{
							unindentAfter = false;
							currentIndentationLevel--;
						}
						indentationLevelStack.Push(currentIndentationLevel);
						nextIndentationLevel = currentIndentationLevel + 1;
					}
					else if (c1 == '}' || c2 == '}' || line.EndsWith("};", StringComparison.Ordinal) && !line.Contains("{"))
					{
						currentIndentationLevel = indentationLevelStack.Pop();
						nextIndentationLevel = currentIndentationLevel;
					}
					else if (line.StartsWith("case", StringComparison.Ordinal) || line.StartsWith("default:", StringComparison.Ordinal))
					{
						currentIndentationLevel = indentationLevelStack.Peek() + 1;
						nextIndentationLevel = currentIndentationLevel + 1;
					}
					else if (c2 == ':')
					{
						if (currentIndentationLevel > 0)
							currentIndentationLevel -= 1;
					}
					else if (line.StartsWith("if", StringComparison.Ordinal) || line.StartsWith("else", StringComparison.Ordinal))
					{
						nextIndentationLevel = currentIndentationLevel + 1;
						unindentAfter = true;
					}
					else if (unindentAfter)
					{
						nextIndentationLevel = currentIndentationLevel - 1;
						unindentAfter = false;
					}
					else nextIndentationLevel = currentIndentationLevel;
					output.Append('\t', currentIndentationLevel);
					output.AppendLine(line);
				}
				else output.AppendLine();
			}
		}
	}
}
