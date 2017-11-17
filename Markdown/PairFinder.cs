using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

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
    
    [TestFixture]
    public class PairFinder_Should
    {
        private PairFinder pairFinder;
        [SetUp]
        public void SetUp()
        {
             pairFinder = new PairFinder();   
        }
        [Test]
        public void ReturnEmptyArray_WhenGivenTwoEmptyArrays()
        {
            pairFinder.FindTagPairs(new int[] { }, new int[] { }).Should().BeEmpty();
        }

        [Test]
        public void ReturnEmptySequence_WhenOnlyClosingsAreEmpty()
        {
            pairFinder.FindTagPairs(new int[] { 10 }, new int[] { }).Should().BeEmpty();
        }
        
        [Test]
        public void ReturnEmptySequence_WhenOnlyOpeningsAreEmpty()
        {
            pairFinder.FindTagPairs(new int[] { 10 }, new int[] { }).Should().BeEmpty();
        }
        
       [ Test]
        public void ReturnEmptySequence_WhenOpeningIsGreaterThanCosing()
        {
            pairFinder.FindTagPairs(new int[] { 10 }, new int[] { 9 }).Should().BeEmpty();
        }

        [Test]
        public void ReturnCorrectResult_OnSimpleTest()
        {
            pairFinder.FindTagPairs(new []{ 10 }, new []{ 11 }).Should().BeEquivalentTo(new []{(10, 11)});
        }
        
        [Test]
        public void ReturnCorrectResult_OnTwoCrossoverSegments()
        {
            pairFinder.FindTagPairs(new []{ 10, 11 }, new []{ 11, 12 }).Should().BeEquivalentTo(new []{(10, 11)});
        }
        
        [Test]
        public void ReturnCorrectResult_OnOneOpeningAndTwoClosing()
        {
            pairFinder.FindTagPairs(new []{ 10}, new []{ 11, 12 }).Should().BeEquivalentTo(new []{(10, 11)});
        }
    }
}