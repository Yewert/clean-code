namespace Markdown
{
    public class Cursive : IFormattingUnit
    {
        private readonly string htmlClosingTag;
        private readonly string htmlOpeningTag;
        private readonly string markdownOpeningTag;


        public Cursive() : this("em", "_")
        {
        }

        public Cursive(string htmlTag, string markdownOpeningTag)
        {
            this.markdownOpeningTag = markdownOpeningTag;
            (htmlOpeningTag, htmlClosingTag) = NameToTagConverter.GetTagFromName(htmlTag);
        }

        public string MarkdownTag => markdownOpeningTag;

        public string HtmlOpeningTag => htmlOpeningTag;

        public string HtmlClosingTag => htmlClosingTag;
    }
}