﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
	/* Split by space
	 * Take while not letters
	 * if len == token len -> drop
	 * else work with token (i.e. put in stack if it is known
	 */
	public class Md	
	{
		private readonly string openingParagraph;
		private readonly string closingParagraph;
		private readonly Dictionary<IFormattingUnit, int> tagPriorities;
		private readonly Dictionary<string, IFormattingUnit> tagToFormatter;
		private readonly Func<char, bool> isAllowedInTag;
		

		public Md(IFormattingUnit[] formatters, string paragraphSymbol="p")
		{
			var allowedChar = new HashSet<char>(formatters.SelectMany(f => f.MarkdownTag));
			isAllowedInTag = allowedChar.Contains;
			tagPriorities = formatters.Select((f, i) => (f, i)).ToDictionary(v => v.Item1, v => v.Item2);
			tagToFormatter = formatters.Select(f => (f.MarkdownTag, f)).ToDictionary(v => v.Item1, v => v.Item2);
			(openingParagraph, closingParagraph) = NameToTagConverter.GetTagFromName(paragraphSymbol);
		}

		public string RenderToHtml(string source)
		{
			if (source is null)
				throw new ArgumentNullException();

			source = source.Replace("<", "&lt").Replace(">", "&gt");

			var tagStack = new Stack<(IFormattingUnit formatter, int position)>();

			var replacements = new List<(IFormattingUnit formatter, int position, bool isOpening)>();
			
			var tokenStartIndex = 0;
			foreach (var token in source.Split(' '))
			{
				var openingBackSlashes = token.TakeWhile(c => c == '/');
				var openingPart = openingBackSlashes.Count() % 2 != 0 ? 
					new char[]{} : token.Skip(openingBackSlashes.Count()).TakeWhile(isAllowedInTag);
				if (openingPart.Count() + openingBackSlashes.Count() != token.Length)
				{
					var potentialTagAtStart = string.Join("", openingPart);
					ProcessNewPotentialTag(
						potentialTagAtStart,
						tagStack,
						replacements,
						tokenStartIndex);
					
				}
				var potentialTagAtTail = string.Join("", token.Reverse().TakeWhile(isAllowedInTag));
				ProcessNewPotentialTag(
					potentialTagAtTail,
					tagStack,
					replacements,
					tokenStartIndex + token.Length - potentialTagAtTail.Length);
				
				tokenStartIndex += token.Length + 1;
				
			}
			return $"{openingParagraph}{ReplaceMarkdownTagsWithHtml(source, replacements)}{closingParagraph}";
		}

		private void ProcessNewPotentialTag(
			string potentialTag, Stack<(IFormattingUnit formatter, int position)> tagStack,
			List<(IFormattingUnit formatter, int position, bool isOpening)> replacements,
			int startIndex)
		{
			if (potentialTag.Length == 0 || !tagToFormatter.ContainsKey(potentialTag)) 
				return;
 			var priority = tagPriorities[tagToFormatter[potentialTag]];
			var formatter = tagToFormatter[potentialTag];
//			while (tagStack.Count != 0 && tagPriorities[tagStack.Peek().formatter] > priority)
//			{
//				tagStack.Pop();
//			}

			if (tagStack.Count != 0 && tagStack.Peek().formatter == formatter)
			{
				var (_, position) = tagStack.Pop();
				replacements.Add((formatter, position, true));
				replacements.Add((formatter, startIndex, false));
			}
			else if (tagStack.Count == 0 || tagPriorities[tagStack.Peek().formatter] > priority)
			{
				tagStack.Push((formatter, startIndex));
			}
		}

		private string ReplaceMarkdownTagsWithHtml(
			string source,
			List<(IFormattingUnit formatter, int position, bool isOpening)> replacements)
		{
			var builder = new StringBuilder(source);
			var offset = 0;
			
			foreach (var replacement in replacements.OrderBy(x => x.position))
			{
				var formatter = replacement.formatter;
				if (replacement.isOpening)
				{
					builder.Remove(offset + replacement.position, formatter.MarkdownTag.Length);
					builder.Insert(offset + replacement.position, formatter.HtmlOpeningTag);
					offset += formatter.HtmlOpeningTag.Length - formatter.MarkdownTag.Length;
				}
				else
				{
					builder.Remove(offset + replacement.position, formatter.MarkdownTag.Length);
					builder.Insert(offset + replacement.position, formatter.HtmlClosingTag);
					offset += formatter.HtmlClosingTag.Length - formatter.MarkdownTag.Length;
				}
			}
			return builder.ToString();
		}
	}

	[TestFixture]
	public class Md_Should
	{
		private string openingParagraph;
		private string closingParagraph;
		private string openingItalic;
		private string closingItalic;
		private string openingStrong;
		private string closingStrong;
		private string openingCode;
		private string closingCode;
		private string openingHeader1;
		private string closingHeader1;
		private string openingHeader2;
		private string closingHeader2;
		private Md markdownParser;
		[SetUp]
		public void SetUp()
		{
			(openingParagraph, closingParagraph) = NameToTagConverter.GetTagFromName("p");
			(openingItalic, closingItalic) = NameToTagConverter.GetTagFromName("em");
			(openingStrong, closingStrong) = NameToTagConverter.GetTagFromName("strong");
			(openingCode, closingCode) = NameToTagConverter.GetTagFromName("code");
			(openingHeader1, closingHeader1) = NameToTagConverter.GetTagFromName("h1");
			(openingHeader2, closingHeader2) = NameToTagConverter.GetTagFromName("h2");
			markdownParser = new Md(new IFormattingUnit[]
					{
						new Cursive(),
						new Bold(),
						new CodeTag()
					});
		}
			
		[Test]
		public void ReturnsTextInParagraphTags_WhenPutSimpeString()
		{
			markdownParser.RenderToHtml("abcd").Should().Be($"{openingParagraph}abcd{closingParagraph}");
			//<p>abcd</p>
		}

		[Test]
		public void ThrowsArgumentException_WhenStringIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => markdownParser.RenderToHtml(null));
		}

		#region SingleUnderscore

		[Test]
		public void PutsOneEmTag_WhenGivenOneWordSurroundedByUnderscores()
		{
			markdownParser.RenderToHtml("_abcd_").Should()
				.Be($"{openingParagraph}{openingItalic}abcd{closingItalic}{closingParagraph}");
			//<p><em>abcd</em></p>
		}
		
		[Test]
		public void DoesNotPutAnyEmTags_WhenOpeningUnderscoreIsFollowedByWhitespaceCharacter()
		{
			markdownParser.RenderToHtml("_ abcd_").Should()
				.Be($"{openingParagraph}_ abcd_{closingParagraph}");
			//<p>_ abcd_</p>
		}
		
		[Test]
		public void PutsTwoEmTags_WhenTwoSingleUnderscoreScopesAreSeparatedByLetter()
		{
			markdownParser.RenderToHtml("_a_b_d_").Should()
				.Be($"{openingParagraph}{openingItalic}a" +
				    $"_b_d{closingItalic}{closingParagraph}");
			//<p><em>a_b_d</em></p>
		}

		[Test]
		public void DoesNotPutAnyEmTags_WhenSingleUnderscoresAreNextToDigits()
		{
			markdownParser.RenderToHtml("a_1_2").Should().Be($"{openingParagraph}a_1_2{closingParagraph}");
			//<p>a_1_2</p>
		}
		
		#endregion

		#region DoubleUnderscore


		[Test]
		public void PutsOneStrongTag_WhenOneGivenOneWordSurroundedByDoubleUnderscores()
		{
			markdownParser.RenderToHtml("__ad__").Should()
				.Be($"{openingParagraph}{openingStrong}a" +
				    $"d{closingStrong}{closingParagraph}");
			//<p><strong>ad</strong></p>
		}
		
		[Test]
		public void DoesNotPutAnyStrongTags_WhenOpeningDoubleUnderscoreIsFollowedByWhitespaceCharacter()
		{
			markdownParser.RenderToHtml("__ abcd__").Should()
				.Be($"{openingParagraph}__ abcd__{closingParagraph}");
			//<p>__ abcd __</p>
		}
		
		[Test]
		public void PutsTwoStrongTags_WhenTwoDoubleUnderscoreScopesAreSeparatedByLetter()
		{
			markdownParser.RenderToHtml("__a__b__d__").Should()
				.Be($"{openingParagraph}{openingStrong}a" +
				    $"__b__d{closingStrong}{closingParagraph}");
			//<p><strong>a</strong><strong>b</strong><strong>b</strong></p>
		}
		
		#endregion

		#region SingleAndDoubleUnderscores

		[Test]
		public void PutsOnlyEmTag_WhenDoubleUnderscoreIsInsideSingleUnderscoreScope()
		{
			markdownParser.RenderToHtml("_ab __cd__ ef_").Should().Be(
				$"{openingParagraph}{openingItalic}ab __cd__ ef{closingItalic}{closingParagraph}");
			//<p><em>ab __cd__ ef</em></p>
		}

		[Test]
		public void PutsBothEmAndStrongTags_WhenSingleUnderscoreIsInsideDoubleUnderscoreScope()
		{
			markdownParser.RenderToHtml("__ab _cd_ ef__").Should().Be(
				$"{openingParagraph}{openingStrong}ab {openingItalic}cd{closingItalic} ef{closingStrong}{closingParagraph}");
			//<p><strong>ab <em>cd</em> ef</strong></p>
		}

		[Test]
		public void DoesNotPutAnyTags_WhenUnderscoresAreImpaired()
		{
			markdownParser.RenderToHtml("__ab_").Should().Be(
				$"{openingParagraph}__ab_{closingParagraph}");
			//<p>__ab_</p>
		}

		#endregion

		#region CodeTag

		[Test]
		public void PutsCodeTag_WhenOneWordIsSurroundedByBackQuote()
		{
			markdownParser.RenderToHtml("`ab`").Should().Be(
				$"{openingParagraph}{openingCode}ab{closingCode}{closingParagraph}");
			//<p><code>ab</code></p>
		}

		#endregion
//
//		#region Header1
//
//		[Test]
//		public void PutsH1Tag_WhenHashIsInTheStartOfTheString()
//		{
//			markdownParser.RenderToHtml("# kek").Should().Be(
//				$"{openingParagraph}{openingHeader1}kek{closingHeader1}{closingParagraph}");
//			//<p><h1>ab</h1></p>
//		}
//		
//		[Test]
//		public void PutsEmTagInsideH1Tag_WhenUnderscoreIsAfterHash()
//		{
//			markdownParser.RenderToHtml("# _kek_").Should().Be(
//				$"{openingParagraph}{openingHeader1}{openingItalic}kek{closingItalic}{closingHeader1}{closingParagraph}");
//			//<p><h1><em>ab</em></h1></p>
//		}
//
//		#endregion
//		
//		#region Header2
//
//		[Test]
//		public void PutsH2Tag_WhenHashIsInTheStartOfTheString()
//		{
//			markdownParser.RenderToHtml("## kek").Should().Be(
//				$"{openingParagraph}{openingHeader2}kek{closingHeader2}{closingParagraph}");
//			//<p><h2>ab</h2></p>
//		}
//
//		#endregion
	}
}