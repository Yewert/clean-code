using System.Text.RegularExpressions;

namespace Markdown
{
    public class DoubleUnderscore : IFormattingUnit
    {
        private readonly Regex markdownOpeningTag;
        private readonly Regex markdownClosingTag;
        private readonly string htmlTagName;
        private readonly string htmlClosingTag;
        private readonly string htmlOpeningTag;
        private string markdownTag;


        public DoubleUnderscore() : this("strong", @"(?<![_\d\\])__(?![_\s\d])", @"(?<![_\s\d\\])__(?![_\d])", "__")
        {
        }

        private DoubleUnderscore(string htmlTag,
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