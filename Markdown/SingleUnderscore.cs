﻿using System.Text.RegularExpressions;

namespace Markdown
{
    public class SingleUnderscore : IFormattingUnit
    {
        private readonly Regex markdownOpeningTagPattern;
        private readonly Regex markdownClosingTagPattern;
        private readonly string htmlTagName;
        private readonly string htmlClosingTag;
        private readonly string htmlOpeningTag;
        private readonly string markdownOpeningTag;
        private readonly string markdownClosingTag;


        public SingleUnderscore() : this("em", @"(?<![_\d\\])_(?![_\s\d])", @"(?<![_\s\d\\])_(?![_\d])", "_", "_")
        {
        }

        private SingleUnderscore(string htmlTag,
            string markdownOpeningTagPattern,
            string markdownClosingTagPattern, string markdownOpeningTag, string markdownClosingTag)
        {
            htmlTagName = htmlTag;
            this.markdownOpeningTag = markdownOpeningTag;
            this.markdownClosingTag = markdownClosingTag;
            (htmlOpeningTag, htmlClosingTag) = NameToTagConverter.GetTagFromName(htmlTag);
            this.markdownOpeningTagPattern = new Regex(markdownOpeningTagPattern, RegexOptions.Compiled);
            this.markdownClosingTagPattern = new Regex(markdownClosingTagPattern, RegexOptions.Compiled);
        }

        public Regex MarkdownOpeningTagPattern => markdownOpeningTagPattern;

        public Regex MarkdownClosingTagPattern => markdownClosingTagPattern;

        public string MarkdownOpeningTag => markdownOpeningTag;

        public string MarkdownClosingTag => markdownClosingTag;

        public string HtmlOpeningTag => htmlOpeningTag;

        public string HtmlClosingTag => htmlClosingTag;

        public string HtmlTagName => htmlTagName;
    }
}