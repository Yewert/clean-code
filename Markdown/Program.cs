﻿using System;
using System.IO;
using System.Linq;

namespace Markdown
{
	static class Program
	{
		public static void Main(string[] args)
		{
			var source = args[0];
			var content = File.ReadAllText(source);
			var paragraphs = content.Split(new[] {"\r\n\r\n"}, StringSplitOptions.RemoveEmptyEntries);
			var markdownParser = new Md(new IFormattingUnit[]
				{
					new Italic(),
					new Bold(),
					new CodeTag()
				}, new PairFinder());
			var save = args[1];
			File.WriteAllLines(save, 
				paragraphs.Select(paragraph => markdownParser.RenderToHtml(paragraph)).ToList());
		}
	}
}
