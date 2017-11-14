﻿using System.Text.RegularExpressions;

namespace Markdown
{
    public class SingleUnderscore : IFormattingUnit
    {
        private readonly Regex markdownOpeningTag;
        private readonly Regex markdownClosingTag;
        private readonly string htmlTagName;
        private readonly string htmlClosingTag;
        private readonly string htmlOpeningTag;
        private string markdownTag;


        public SingleUnderscore() : this("em", @"(?<![_\d\\])_(?![_\s\d])", @"(?<![_\s\d\\])_(?![_\d])", "_")
        {
        }

        private SingleUnderscore(string htmlTag,
            string markdownOpeningTagPattern,
            string markdownClosingTagPattern, string markdownTag)
        {
            htmlTagName = htmlTag;
            this.markdownTag = markdownTag;
            (htmlOpeningTag, htmlClosingTag) = NameToTagConverter.GetTagFromName(htmlTag);
            markdownOpeningTag = new Regex(markdownOpeningTagPattern, RegexOptions.Compiled);
            markdownClosingTag = new Regex(markdownClosingTagPattern, RegexOptions.Compiled);
        }

        public Regex MarkdownOpeningTagPattern => markdownOpeningTag;

        public Regex MarkdownClosingTagPattern => markdownClosingTag;

        public string MarkdownTag => markdownTag;

        public string HtmlOpeningTag => htmlOpeningTag;

        public string HtmlClosingTag => htmlClosingTag;

        public string HtmlTagName => htmlTagName;
    }
}