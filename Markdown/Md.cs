﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markdown
{
	public class Md	
	{
		private readonly IFormattingUnit[] formatters;
		private readonly IPairFinder pairFinder;
		private readonly string openingParagraph;
		private readonly string closingParagraph;
		private readonly Dictionary<IFormattingUnit, int> tagPriorities;
		private readonly Dictionary<string, IFormattingUnit> tagToFormatter;
		private readonly Func<char, bool> isAllowedInTag;
		private readonly Func<int, EscapeSymbol> escapeFactory;
		

		public Md(IFormattingUnit[] formatters, IPairFinder pairFinder, char escapeSymbol = '\\', string paragraphSymbol="p")
		{
			this.formatters = formatters;
			this.pairFinder = pairFinder;
			var tagChars = new HashSet<char>(formatters.SelectMany(f => f.MarkdownTag)) {'\\'};
			escapeFactory = amount => new EscapeSymbol(amount, tagChars, escapeSymbol);
			isAllowedInTag = tagChars.Contains;
			tagPriorities = formatters.Select((f, i) => (f, i)).ToDictionary(v => v.Item1, v => v.Item2);
			tagToFormatter = formatters.Select(f => (f.MarkdownTag, f)).ToDictionary(v => v.Item1, v => v.Item2);
			(openingParagraph, closingParagraph) = NameToTagConverter.GetTagFromName(paragraphSymbol);
		}

		public string RenderToHtml(string source)
		{
			if (source is null)
				throw new ArgumentNullException();

			source = source.Replace("<", "&lt").Replace(">", "&gt");
			var tags = new List<(IFormattingUnit formatter, int position, bool canBeOpening, bool canBeClosing)>();
			var currentToken = new StringBuilder();
			var escapeSequences = new List<(EscapeSymbol formatter, int position)>();
			for (var i = 0; i < source.Length + 1; i++)
			{
				if (i < source.Length && isAllowedInTag(source[i]))
				{
					currentToken.Append(source[i]);
					continue;
				}

				var potentialTag = currentToken.ToString();
				var preceedingCharacterIndex = i - potentialTag.Length - 1;
				var followingCharacterIndex = i;

				var amountOfEscapeChars = potentialTag.TakeWhile(c => c == '\\').Count();
				if (amountOfEscapeChars != 0)
				{
					var esc = escapeFactory(amountOfEscapeChars);
					escapeSequences.Add((esc, preceedingCharacterIndex + 1));
					if (amountOfEscapeChars % 2 != 0)
					{
						currentToken.Clear();
						continue;
					}

					potentialTag = potentialTag.Substring(amountOfEscapeChars);
				}
				if (!tagToFormatter.ContainsKey(potentialTag))
				{
					currentToken.Clear();
					continue;
				}

				var formatter = tagToFormatter[potentialTag];
				var canBeOpening = false;
				var canBeClosing = false;
				if (preceedingCharacterIndex >= 0)
				{
					canBeClosing = formatter.isLegalPrecedingCharacter(source[preceedingCharacterIndex]);
				}
				if (followingCharacterIndex < source.Length)
				{
					canBeOpening = formatter.isLegalFollowingCharacter(source[followingCharacterIndex]);
				}
				if (!canBeClosing && !canBeOpening)
				{
					currentToken.Clear();
					continue;
				}

				tags.Add((formatter, preceedingCharacterIndex + 1 + amountOfEscapeChars, canBeOpening, canBeClosing));
				currentToken.Clear();
			}
			
			var tagLevelSeparatedSubstringIndexes = new (int, int)[formatters.Length][];
			for (var i = 0; i < formatters.Length; i++)
			{
				tagLevelSeparatedSubstringIndexes[i] = 
					GetIndexesForTagType(formatters[i], tags, tagLevelSeparatedSubstringIndexes);
			}
			return $"{openingParagraph}" +
			       $"{ReplaceMarkdownTagsWithHtml(source, GetReplacementsInOrder(escapeSequences, tagLevelSeparatedSubstringIndexes))}" +
			       $"{closingParagraph}";
		}
		
		private bool IntersectsWithAnyScope(int scopePriority, int number,
			IReadOnlyList<(int, int)[]> tagLevelSeparatedSubstringIndexes)
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
		
		private (int, int)[] GetIndexesForTagType(
			IFormattingUnit formatter,
			List<(IFormattingUnit formatter, int position, bool canBeOpening, bool canBeClosing)> tags,
			IReadOnlyList<(int, int)[]> tagLevelSeparatedSubstringIndexes
			)
		{
			var openings = tags
				.Where(x => 
					x.formatter == formatter && x.canBeOpening)
				.Where(x => 
					!IntersectsWithAnyScope(
						tagPriorities[formatter],
						x.position,
						tagLevelSeparatedSubstringIndexes))
				.Select(x => x.position)
				.ToArray();
			var closings = tags
				.Where(x => 
					x.formatter == formatter && x.canBeClosing)
				.Where(x => 
					!IntersectsWithAnyScope(
						tagPriorities[formatter],
						x.position,
						tagLevelSeparatedSubstringIndexes))
				.Select(x => x.position)
				.ToArray();
			return pairFinder.FindTagPairs(openings, closings);
		}

		private IEnumerable<(int position, bool isOpening, IFormattingUnit formatter)> GetReplacementsInOrder(
			List<(EscapeSymbol formatter, int position)> escapeSymbols,
			IReadOnlyList<(int, int)[]> tagLevelSeparatedSubstringIndexes)
		{
			var result = new List<(int position, bool isOpening, IFormattingUnit formatter)>();
			for (var i = 0; i < tagLevelSeparatedSubstringIndexes.Count; i++)
			{
				foreach (var segment in tagLevelSeparatedSubstringIndexes[i])
				{
					result.Add((segment.Item1, true, formatters[i]));
					result.Add((segment.Item2, false, formatters[i]));
				}                               				
			}
			result.AddRange(escapeSymbols.Select(x => (x.position, true, (IFormattingUnit)x.formatter)));
			return result.OrderBy(x => x.position);
		}

		private string ReplaceMarkdownTagsWithHtml(
			string source,
			IEnumerable<(int position, bool isOpening, IFormattingUnit formatter)> replacements)
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
}