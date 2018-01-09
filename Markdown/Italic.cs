using System;
using System.Collections.Generic;

namespace Markdown
{
    public class Italic : IFormattingUnit
    {
        private readonly string htmlClosingTag;
        private readonly string htmlOpeningTag;
        private readonly string markdownOpeningTag;
        private readonly HashSet<char> illegalFollowings;
        private readonly HashSet<char> illegalPreceedings;


        public Italic(Func<string, (string, string)> getTagFromName) : this("em", "_", "1234567890 ", "1234567890 ", getTagFromName)
        {
        }

        public Italic(
            string htmlTag,
            string markdownOpeningTag,
            string illegalPreceeding,
            string illegalFollowing,
            Func<string, (string, string)> getTagFromName)
        {
            this.markdownOpeningTag = markdownOpeningTag;
            illegalFollowings = new HashSet<char>(illegalFollowing);
            illegalPreceedings = new HashSet<char>(illegalPreceeding);
            (htmlOpeningTag, htmlClosingTag) = getTagFromName(htmlTag);
        }

        public string MarkdownTag => markdownOpeningTag;

        public string HtmlOpeningTag => htmlOpeningTag;

        public string HtmlClosingTag => htmlClosingTag;
        
        public bool isLegalPrecedingCharacter(char ch)
        {
            return !illegalPreceedings.Contains(ch);
        }

        public bool isLegalFollowingCharacter(char ch)
        {
            return !illegalFollowings.Contains(ch);
        }
    }
}