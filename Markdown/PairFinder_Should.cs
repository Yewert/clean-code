using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
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