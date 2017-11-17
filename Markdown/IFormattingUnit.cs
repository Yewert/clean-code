using System.Text.RegularExpressions;

namespace Markdown
{
    public interface IFormattingUnit
    {
        Regex MarkdownOpeningTagPattern { get; }
        Regex MarkdownClosingTagPattern { get; }
        string MarkdownOpeningTag { get; }
        string MarkdownClosingTag { get; }
        string HtmlOpeningTag { get; }
        string HtmlClosingTag { get; }
        string HtmlTagName { get; }
        
    }
}