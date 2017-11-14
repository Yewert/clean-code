using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Markdown
{
    public interface IFormattingUnit
    {
        Regex MarkdownOpeningTagPattern { get; }
        Regex MarkdownClosingTagPattern { get; }
        string MarkdownTag { get; }
        string HtmlOpeningTag { get; }
        string HtmlClosingTag { get; }
        string HtmlTagName { get; }
        
    }
}