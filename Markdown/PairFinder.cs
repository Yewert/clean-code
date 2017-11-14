using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    public static class PairFinder
    {
        public static (int a, int b)[] FindTagPairs(int[] openings, int[] closings)
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
                    if (currentOpeningIndex < closings.Length - 1)
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
    
    [TestFixture]
    public class PairFinder_Should
    {
        [Test]
        public void ReturnEmptyArray_WhenGivenTwoEmptyArrays()
        {
            PairFinder.FindTagPairs(new int[] { }, new int[] { }).Should().BeEmpty();
        }

        [Test]
        public void ReturnEmptySequence_WhenOnlyClosingsAreEmpty()
        {
            PairFinder.FindTagPairs(new int[] { 10}, new int[] { }).Should().BeEmpty();
        }
        
        [Test]
        public void ReturnEmptySequence_WhenOnlyOpeningsAreEmpty()
        {
            PairFinder.FindTagPairs(new int[] { 10}, new int[] { }).Should().BeEmpty();
        }
        
       [ Test]
        public void ReturnEmptySequence_WhenOpeningIsGreaterThanCosing()
        {
            PairFinder.FindTagPairs(new int[] { 10}, new int[] { 9}).Should().BeEmpty();
        }

        [Test]
        public void ReturnCorrectResult_OnSimpleTest()
        {
            PairFinder.FindTagPairs(new []{10}, new []{11}).Should().BeEquivalentTo(new []{(10, 11)});
        }
        
        [Test]
        public void ReturnCorrectResult_OnTwoCrossoverSegments()
        {
            PairFinder.FindTagPairs(new []{10, 11}, new []{11, 12}).Should().BeEquivalentTo(new []{(10, 11)});
        }
    }
}