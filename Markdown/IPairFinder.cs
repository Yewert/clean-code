namespace Markdown
{
    public interface IPairFinder
    {
        (int a, int b)[] FindTagPairs(int[] openings, int[] closings);
    }
}