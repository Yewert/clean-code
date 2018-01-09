using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public class EscapeSymbol : IFormattingUnit
    {
        private readonly HashSet<char> legalFollowings;
        private string markdownTag;
        private string htmlOpeningTag;
        private string htmlClosingTag;

        public EscapeSymbol(int amount, HashSet<char> legalFollowings, char escapeSymbol = '\\')
        {
            this.legalFollowings = legalFollowings;
            markdownTag = string.Join("", Enumerable.Repeat(escapeSymbol, amount));
            htmlOpeningTag = string.Join("", Enumerable.Repeat(escapeSymbol, amount / 2));
            htmlClosingTag = htmlOpeningTag;
        }

        public string MarkdownTag
        {
            get { return markdownTag; }
        }

        public string HtmlOpeningTag
        {
            get { return htmlOpeningTag; }
        }

        public string HtmlClosingTag
        {
            get { return htmlClosingTag; }
        }

        public bool isLegalPrecedingCharacter(char ch)
        {
            return true;
        }

        public bool isLegalFollowingCharacter(char ch)
        {
            return legalFollowings.Contains(ch);
        }
    }
}