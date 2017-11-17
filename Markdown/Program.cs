using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Markdown
{
	class Program
	{
		static void Main(string[] args)
		{
			var source = args[0];
			var content = File.ReadAllText(source);
			var paragraphs = content.Split(new[] {"\r\n\r\n"}, StringSplitOptions.RemoveEmptyEntries);
			var markdownParser = new Md(new IFormattingUnit[]
				{
					new SingleUnderscore(),
					new DoubleUnderscore(),
					new Header1Tag(),
					new Header2Tag(), 
					new CodeTag()
				},
				new PairFinder());
			var save = args[1];
			var html = new List<string>();
			for (var index = 0; index < paragraphs.Length; index++)
			{
				var paragraph = paragraphs[index];
				html.Add(markdownParser.RenderToHtml(paragraph));
			}
			File.WriteAllLines(save, html);
		}
	}
}
