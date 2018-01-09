﻿using System;
using System.IO;
using System.Linq;

namespace Markdown
{
	static class Program
	{
		private static (string openingTag, string closingTag) GetTagFromName(string name)
		{
			if (name is null)
				throw new ArgumentNullException();
			return ($"<{name}>", $"</{name}>");
		}
		public static void Main(string[] args)
		{
			var source = args[0];
			var content = File.ReadAllText(source);
			var paragraphs = content.Split(new[] {"\r\n\r\n"}, StringSplitOptions.RemoveEmptyEntries);
			var markdownParser = new Md(new IFormattingUnit[]
				{
					new Italic(GetTagFromName),
					new Bold(GetTagFromName),
					new CodeTag(GetTagFromName)
				},
				new PairFinder(),
				("<p>", "</p>"));
			var save = args[1];
			File.WriteAllLines(save, 
				paragraphs.Select(paragraph => markdownParser.RenderToHtml(paragraph)).ToList());
		}
	}
}
