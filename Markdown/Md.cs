using System;
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
		private readonly List<IFormattingUnit> formatters;
		private (int A, int B)[][] tagLevelSeparatedSubstringIndexes;
		
		public Md(string paragraphSymbol="p")
		{
			formatters = new List<IFormattingUnit>()
			{
				new SingleUnderscore(),
				new DoubleUnderscore()
			};
			(openingParagraph, closingParagraph) = NameToTagConverter.GetTagFromName(paragraphSymbol);
		}

		public string RenderToHtml(string source)
		{
			if (source is null)
				throw new ArgumentNullException();

			tagLevelSeparatedSubstringIndexes = new (int, int)[formatters.Count][];
			for (int i = 0; i < formatters.Count; i++)
			{
				tagLevelSeparatedSubstringIndexes[i] = GetIndexesForTagType(formatters[i], i, source);
			}
			return $"{openingParagraph}{ReplaceMarkdownTagsWithHtml(source)}{closingParagraph}";
		}

		private bool IntersectsWithAnyScope(int scopePriority, int number)
		{
			for (int i = scopePriority - 1; i >= 0; i--)
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
			return PairFinder.FindTagPairs(openings, closings);
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
					builder.Remove(offset + replacement.Position, formatter.MarkdownTag.Length);
					builder.Insert(offset + replacement.Position, formatter.HtmlOpeningTag);
					offset += formatter.HtmlOpeningTag.Length - formatter.MarkdownTag.Length;
				}
				else
				{
					builder.Remove(offset + replacement.Position, formatter.MarkdownTag.Length);
					builder.Insert(offset + replacement.Position, formatter.HtmlClosingTag);
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
		private Md markdownParser;
		[SetUp]
		public void SetUp()
		{
			(openingParagraph, closingParagraph) = NameToTagConverter.GetTagFromName("p");
			(openingItalic, closingItalic) = NameToTagConverter.GetTagFromName("em");
			(openingStrong, closingStrong) = NameToTagConverter.GetTagFromName("strong");
			markdownParser = new Md();
		}
			
		[Test]
		public void ReturnsTextInParagraphTags_WhenPutSimpeString()
		{
			markdownParser.RenderToHtml("abcd").Should().Be($"{openingParagraph}abcd{closingParagraph}");
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
		}
		
		[Test]
		public void DoesNotPutAnyEmTags_WhenOpeningUnderscoreIsFollowedByWhitespaceCharacter()
		{
			markdownParser.RenderToHtml("_ abcd_").Should()
				.Be($"{openingParagraph}_ abcd_{closingParagraph}");
		}
		
		[Test]
		public void PutsTwoEmTags_WhenTwoSingleUnderscoreScopesAreSeparatedByLetter()
		{
			markdownParser.RenderToHtml("_a_b_d_").Should()
				.Be($"{openingParagraph}{openingItalic}a" +
				    $"{closingItalic}b{openingItalic}d{closingItalic}{closingParagraph}");
		}
		
		#endregion

		#region DoubleUnderscore


		[Test]
		public void PutsOneStrongTag_WhenOneGivenOneWordSurroundedByDoubleUnderscores()
		{
			markdownParser.RenderToHtml("__ad__").Should()
				.Be($"{openingParagraph}{openingStrong}a" +
				    $"d{closingStrong}{closingParagraph}");
		}
		
		[Test]
		public void DoesNotPutAnyStrongTags_WhenOpeningDoubleUnderscoreIsFollowedByWhitespaceCharacter()
		{
			markdownParser.RenderToHtml("__ abcd__").Should()
				.Be($"{openingParagraph}__ abcd__{closingParagraph}");
		}
		
		[Test]
		public void PutsTwoStrongTags_WhenTwoDoubleUnderscoreScopesAreSeparatedByLetter()
		{
			markdownParser.RenderToHtml("__a__b__d__").Should()
				.Be($"{openingParagraph}{openingStrong}a" +
				    $"{closingStrong}b{openingStrong}d{closingStrong}{closingParagraph}");
		}
		
		#endregion

		#region SingleAndDoubleUnderscores

		[Test]
		public void PutsOnlyEmTag_WhenDoubleUnderscoreIsInsideSingleUnderscoreScope()
		{
			markdownParser.RenderToHtml("_ab __cd__ ef_").Should().Be(
				$"{openingParagraph}{openingItalic}ab __cd__ ef{closingItalic}{closingParagraph}");
		}

		[Test]
		public void PutsBothEmAndStrongTags_WhenSingleUnderscoreIsInsideDoubleUnderscoreScope()
		{
			markdownParser.RenderToHtml("__ab _cd_ ef__").Should().Be(
				$"{openingParagraph}{openingStrong}ab {openingItalic}cd{closingItalic} ef{closingStrong}{closingParagraph}");
		}

		[Test]
		public void DoesNotPutAnyTags_WhenUnderscoresAreImpaired()
		{
			markdownParser.RenderToHtml("__ab_").Should().Be(
				$"{openingParagraph}__ab_{closingParagraph}");
		}

		#endregion
	}
}