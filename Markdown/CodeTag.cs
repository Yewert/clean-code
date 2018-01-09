using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown
{
    public class CodeTag : IFormattingUnit
    {
        private readonly string htmlClosingTag;
        private readonly string htmlOpeningTag;
        private readonly string markdownOpeningTag;
        private readonly HashSet<char> illegalFollowings;
        private readonly HashSet<char> illegalPreceedings;


        public CodeTag(Func<string, (string, string)> getTagFromName) 
            : this("code", "`", "1234567890 ", "1234567890 ", getTagFromName)
        {
        }

        public CodeTag(
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