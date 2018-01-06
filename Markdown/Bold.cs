using System.Collections.Generic;

namespace Markdown
{
    public class Bold : IFormattingUnit
    {
        private readonly string htmlClosingTag;
        private readonly string htmlOpeningTag;
        private readonly string markdownOpeningTag;
        private readonly HashSet<char> illegalFollowings;
        private readonly HashSet<char> illegalPreceedings;


        public Bold() : this("strong", "__", "1234567890 ", "1234567890 ")
        {
        }

        public Bold(string htmlTag, string markdownOpeningTag, string illegalPreceeding, string illegalFollowing)
        {
            this.markdownOpeningTag = markdownOpeningTag;
            illegalFollowings = new HashSet<char>(illegalFollowing);
            illegalPreceedings = new HashSet<char>(illegalPreceeding);
            (htmlOpeningTag, htmlClosingTag) = NameToTagConverter.GetTagFromName(htmlTag);
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