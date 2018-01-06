﻿using System.Text.RegularExpressions;

namespace Markdown
{
    public class CodeTag : IFormattingUnit
    {
        private readonly string htmlClosingTag;
        private readonly string htmlOpeningTag;
        private readonly string markdownOpeningTag;


        public CodeTag() : this("code", "`")
        {
        }

        public CodeTag(string htmlTag, string markdownOpeningTag)
        {
            this.markdownOpeningTag = markdownOpeningTag;
            (htmlOpeningTag, htmlClosingTag) = NameToTagConverter.GetTagFromName(htmlTag);
        }

        public string MarkdownTag => markdownOpeningTag;

        public string HtmlOpeningTag => htmlOpeningTag;

        public string HtmlClosingTag => htmlClosingTag;
    }
}