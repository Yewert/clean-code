﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
	public class Md	
	{
		private readonly string openingParagraph;
		private readonly string closingParagraph;
		private readonly IFormattingUnit[] formatters;
		private readonly IPairFinder pairFinder;
		private static readonly Regex BackslashRemover = new Regex(@"\\(?!\\)", RegexOptions.Compiled);
		private (int A, int B)[][] tagLevelSeparatedSubstringIndexes;
		
		public Md(IFormattingUnit[] formatters, IPairFinder pairFinder, string paragraphSymbol="p")
		{
			this.formatters = formatters;
			this.pairFinder = pairFinder;
			(openingParagraph, closingParagraph) = NameToTagConverter.GetTagFromName(paragraphSymbol);
		}

		public string RenderToHtml(string source)
		{
			if (source is null)
				throw new ArgumentNullException();

			source = source.Replace("<", "&lt").Replace(">", "&gt");

			tagLevelSeparatedSubstringIndexes = new (int, int)[formatters.Length][];
			for (var i = 0; i < formatters.Length; i++)
			{
				tagLevelSeparatedSubstringIndexes[i] = GetIndexesForTagType(formatters[i], i, source);
			}
			return $"{openingParagraph}{ReplaceMarkdownTagsWithHtml(source)}{closingParagraph}";
		}

		private bool IntersectsWithAnyScope(int scopePriority, int number)
		{
			for (var i = scopePriority - 1; i >= 0; i--)
			{
				if (tagLevelSeparatedSubstringIndexes[i].Any(interval => number.BelongsToSegment(interval)))
				{
					return true;
				}
			}
			return false;
		}

		private (int, int)[] GetIndexesForTagType(IFormattingUnit formatter, int scopePriority, string source)
		{
            var openings = (from Match match in formatter.MarkdownOpeningTagPattern.Matches(source) select match.Index)
	            .Where(index => !IntersectsWithAnyScope(scopePriority, index))
	            .ToArray();
            var closings = (from Match match in formatter.MarkdownClosingTagPattern.Matches(source) select match.Index)
	            .Where(index => !IntersectsWithAnyScope(scopePriority, index))
	            .ToArray();
			return pairFinder.FindTagPairs(openings, closings);
		}

		private IEnumerable<(int Position, bool IsOpening, int Level)> GetReplacementsInOrder()
		{
			var result = new List<(int Position, bool IsOpening, int Level)>();
			for (var i = 0; i < tagLevelSeparatedSubstringIndexes.Length; i++)
			{
				foreach (var segment in tagLevelSeparatedSubstringIndexes[i])
				{
					result.Add((segment.A, true, i));
					result.Add((segment.B, false, i));
				}                               				
			}
			return result.OrderBy(x => x.Position);
		}

		private string ReplaceMarkdownTagsWithHtml(string source)
		{
			var builder = new StringBuilder(source);
			var offset = 0;
			
			foreach (var replacement in GetReplacementsInOrder())
			{
				var formatter = formatters[replacement.Level];
				if (replacement.IsOpening)
				{
					builder.Remove(offset + replacement.Position, formatter.MarkdownOpeningTag.Length);
					builder.Insert(offset + replacement.Position, formatter.HtmlOpeningTag);
					offset += formatter.HtmlOpeningTag.Length - formatter.MarkdownOpeningTag.Length;
				}
				else
				{
					builder.Remove(offset + replacement.Position, formatter.MarkdownClosingTag.Length);
					builder.Insert(offset + replacement.Position, formatter.HtmlClosingTag);
					offset += formatter.HtmlClosingTag.Length - formatter.MarkdownClosingTag.Length;
				}
			}
			return BackslashRemover.Replace(builder.ToString(), "");
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
						new SingleUnderscore(),
						new DoubleUnderscore(),
						new Header1Tag(),
						new Header2Tag(), 
						new CodeTag()
					},
				new PairFinder());
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
				    $"{closingItalic}b{openingItalic}d{closingItalic}{closingParagraph}");
			//<p><em>a</em><em>b</em><em>d</em></p>
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
				    $"{closingStrong}b{openingStrong}d{closingStrong}{closingParagraph}");
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

		#region Header1

		[Test]
		public void PutsH1Tag_WhenHashIsInTheStartOfTheString()
		{
			markdownParser.RenderToHtml("# kek").Should().Be(
				$"{openingParagraph}{openingHeader1}kek{closingHeader1}{closingParagraph}");
			//<p><h1>ab</h1></p>
		}
		
		[Test]
		public void PutsEmTagInsideH1Tag_WhenUnderscoreIsAfterHash()
		{
			markdownParser.RenderToHtml("# _kek_").Should().Be(
				$"{openingParagraph}{openingHeader1}{openingItalic}kek{closingItalic}{closingHeader1}{closingParagraph}");
			//<p><h1><em>ab</em></h1></p>
		}

		#endregion
		
		#region Header2

		[Test]
		public void PutsH2Tag_WhenHashIsInTheStartOfTheString()
		{
			markdownParser.RenderToHtml("## kek").Should().Be(
				$"{openingParagraph}{openingHeader2}kek{closingHeader2}{closingParagraph}");
			//<p><h2>ab</h2></p>
		}

		#endregion
	}
}