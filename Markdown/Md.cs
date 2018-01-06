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
		

		public Md(IFormattingUnit[] formatters, IPairFinder pairFinder, string paragraphSymbol="p")
		{
			this.formatters = formatters;
			this.pairFinder = pairFinder;
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
			var tags = new List<(IFormattingUnit formatter, int position, bool canBeOpening, bool canBeClosing)>();
			var currentToken = new StringBuilder();
			
			for (var i = 0; i < source.Length + 1; i++)
			{
				if (i < source.Length && isAllowedInTag(source[i]))
				{
					currentToken.Append(source[i]);
					continue;
				}

				var potentialTag = currentToken.ToString();
				if (!tagToFormatter.ContainsKey(potentialTag))
				{
					currentToken.Clear();
					continue;
				}

				var formatter = tagToFormatter[potentialTag];
				var canBeOpening = false;
				var canBeClosing = false;
				var preceedingCharacterIndex = i - currentToken.Length - 1;
				if (preceedingCharacterIndex >= 0)
				{
					canBeClosing = formatter.isLegalPrecedingCharacter(source[preceedingCharacterIndex]);
				}
				var followingCharacterIndex = i;
				if (followingCharacterIndex < source.Length)
				{
					canBeOpening = formatter.isLegalFollowingCharacter(source[followingCharacterIndex]);
				}
				if (!canBeClosing && !canBeOpening)
				{
					currentToken.Clear();
					continue;
				}

				tags.Add((formatter, preceedingCharacterIndex + 1, canBeOpening, canBeClosing));
				currentToken.Clear();
			}
			
			var tagLevelSeparatedSubstringIndexes = new (int, int)[formatters.Length][];
			for (var i = 0; i < formatters.Length; i++)
			{
				tagLevelSeparatedSubstringIndexes[i] = 
					GetIndexesForTagType(formatters[i], tags, tagLevelSeparatedSubstringIndexes);
			}
			return $"{openingParagraph}" +
			       $"{ReplaceMarkdownTagsWithHtml(source, GetReplacementsInOrder(tagLevelSeparatedSubstringIndexes))}" +
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

		private int? FindPairPositionOrNull(
			IFormattingUnit formatter,
			Stack<(IFormattingUnit formatter, int position, bool canBeClosing)> stack)
		{
			var depth = 0;
			foreach (var entry in stack)
			{
				if (tagPriorities[entry.formatter] > tagPriorities[formatter])
					return null;
				if (entry.formatter == formatter)
					break;
				depth++;
			}

			if (depth == stack.Count)
				return null;
			for (int i = 0; i < depth; i++)
			{
				stack.Pop();
			}

			var (_, targetPos, _) = stack.Pop();
			return targetPos;
		}

		private IEnumerable<(int position, bool isOpening, int level)> GetReplacementsInOrder(
			IReadOnlyList<(int, int)[]> tagLevelSeparatedSubstringIndexes)
		{
			var result = new List<(int Position, bool IsOpening, int Level)>();
			for (var i = 0; i < tagLevelSeparatedSubstringIndexes.Count; i++)
			{
				foreach (var segment in tagLevelSeparatedSubstringIndexes[i])
				{
					result.Add((segment.Item1, true, i));
					result.Add((segment.Item2, false, i));
				}                               				
			}
			return result.OrderBy(x => x.Position);
		}

		private string ReplaceMarkdownTagsWithHtml(
			string source,
			IEnumerable<(int position, bool isOpening, int level)> replacements)
		{
			var builder = new StringBuilder(source);
			var offset = 0;
			
			foreach (var replacement in replacements.OrderBy(x => x.position))
			{
				var formatter = formatters[replacement.level];
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