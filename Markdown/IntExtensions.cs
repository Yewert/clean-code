using System;

namespace Markdown
{
    public static class IntExtensions
    {
        public static bool BelongsToInterval(this int number, (int a, int b) interval)
        {
            if(interval.a > interval.b)
                throw new ArgumentException();
            return number > interval.a && number < interval.b;
        }

        public static bool BelongsToSegment(this int number, (int a, int b) segment)
        {
            if(segment.a > segment.b)
                throw new ArgumentException();
            return number.BelongsToInterval(segment) || number == segment.a || number == segment.b;
        }
    }
}