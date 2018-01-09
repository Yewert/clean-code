using System.Collections.Generic;

namespace Markdown
{
    public class PairFinder : IPairFinder
    {
        public (int a, int b)[] FindTagPairs(int[] openings, int[] closings)
        {
            var result = new List<(int, int)>();
            var currentOpeningIndex = 0;
            var currentClosingIndex = 0;
            while (currentOpeningIndex < openings.Length && currentClosingIndex < closings.Length)
            {
                if (closings[currentClosingIndex] <= openings[currentOpeningIndex])
                {
                    currentClosingIndex++;
                }

                else if (closings[currentClosingIndex] > openings[currentOpeningIndex])
                {
                    if (currentOpeningIndex < openings.Length - 1)
                    {
                        if (openings[currentOpeningIndex + 1] < closings[currentClosingIndex])
                            currentOpeningIndex++;
                        else if (openings[currentOpeningIndex + 1] >= closings[currentClosingIndex])
                        {
                            result.Add((openings[currentOpeningIndex], closings[currentClosingIndex]));
                            currentOpeningIndex += openings[currentOpeningIndex + 1] == closings[currentClosingIndex] 
                                ? 2 
                                : 1;
                            currentClosingIndex++;
                        }
                    }
                    else
                    {
                        result.Add((openings[currentOpeningIndex], closings[currentClosingIndex]));
                        currentOpeningIndex++;
                        currentClosingIndex++;
                    }
                }
            }
            return result.ToArray();
        }
    }
}