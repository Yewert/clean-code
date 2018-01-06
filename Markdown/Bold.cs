namespace Markdown
{
    public class Bold : IFormattingUnit
    {
        private readonly string htmlClosingTag;
        private readonly string htmlOpeningTag;
        private readonly string markdownOpeningTag;


        public Bold() : this("strong", "__")
        {
        }

        public Bold(string htmlTag, string markdownOpeningTag)
        {
            this.markdownOpeningTag = markdownOpeningTag;
            (htmlOpeningTag, htmlClosingTag) = NameToTagConverter.GetTagFromName(htmlTag);
        }

        public string MarkdownTag => markdownOpeningTag;

        public string HtmlOpeningTag => htmlOpeningTag;

        public string HtmlClosingTag => htmlClosingTag;
    }
}