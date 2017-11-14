using System;

namespace Markdown
{
    public static class NameToTagConverter
    {
        public static (string openingTag, string closingTag) GetTagFromName(string name)
        {
            if (name is null)
                throw new ArgumentNullException();
            return ($"<{name}>", $"</{name}>");
        }
    }
}