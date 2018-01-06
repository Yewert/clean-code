using System.Text.RegularExpressions;

namespace Markdown
{
    public interface IFormattingUnit
    {
        string MarkdownTag { get; }
        string HtmlOpeningTag { get; }
        string HtmlClosingTag { get; }
    }
}