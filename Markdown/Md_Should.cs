using System;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown
{
    [TestFixture]
    public class Md_Should
    {
        private string openingParagraph;
        private string closingParagraph;
        private string openingItalic;
        private string closingItalic;
        private string openingStrong;
        private string closingStrong;
        private string openingCode;
        private string closingCode;
        private char escapeSymbol;
        private Md markdownParser;
        [SetUp]
        public void SetUp()
        {
            (openingParagraph, closingParagraph) = NameToTagConverter.GetTagFromName("p");
            (openingItalic, closingItalic) = NameToTagConverter.GetTagFromName("em");
            (openingStrong, closingStrong) = NameToTagConverter.GetTagFromName("strong");
            (openingCode, closingCode) = NameToTagConverter.GetTagFromName("code");
            escapeSymbol = '\\';
            markdownParser = new Md(new IFormattingUnit[]
            {
                new Italic(),
                new Bold(),
                new CodeTag()
            }, new PairFinder(), escapeSymbol);
        }
			
        [Test]
        public void ReturnsTextInParagraphTags_WhenPutSimpeString()
        {
            markdownParser.RenderToHtml("abcd").Should().Be($"{openingParagraph}abcd{closingParagraph}");
            //<p>abcd</p>
        }

        [Test]
        public void ThrowsArgumentException_WhenStringIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => markdownParser.RenderToHtml(null));
        }

        #region SingleUnderscore

        [Test]
        public void PutsOneEmTag_WhenGivenOneWordSurroundedByUnderscores()
        {
            markdownParser.RenderToHtml("_abcd_").Should()
                .Be($"{openingParagraph}{openingItalic}abcd{closingItalic}{closingParagraph}");
            //<p><em>abcd</em></p>
        }
        
        [Test]
        public void EscapesCorrectly_InSimpleCase()
        {
            markdownParser.RenderToHtml($"{escapeSymbol}_abcd{escapeSymbol}_").Should()
                .Be($"{openingParagraph}_abcd_{closingParagraph}");
            //<p><em>abcd</em></p>
        }
        
        [Test]
        public void DoesNotPutAnyEmTags_WhenOpeningTagIsEscaped()
        {
            markdownParser.RenderToHtml($"{escapeSymbol}_abcd_").Should()
                .Be($"{openingParagraph}_abcd_{closingParagraph}");
            //<p><em>abcd</em></p>
        }
        
        [Test]
        public void DoesNotEscape_WhenEvenAmountOfEscapeSymbols()
        {
            markdownParser.RenderToHtml($"{escapeSymbol}{escapeSymbol}_abcd_").Should()
                .Be($"{openingParagraph}{escapeSymbol}{openingItalic}abcd{closingItalic}{closingParagraph}");
            //<p><em>abcd</em></p>
        }
		
        [Test]
        public void DoesNotPutAnyEmTags_WhenOpeningUnderscoreIsFollowedByWhitespaceCharacter()
        {
            markdownParser.RenderToHtml("_ abcd_").Should()
                .Be($"{openingParagraph}_ abcd_{closingParagraph}");
            //<p>_ abcd_</p>
        }
		
        [Test]
        public void PutsTwoEmTags_WhenTwoSingleUnderscoreScopesAreSeparatedByLetter()
        {
            markdownParser.RenderToHtml("_a_b_d_").Should()
                .Be($"{openingParagraph}{openingItalic}a{closingItalic}" +
                    $"b{openingItalic}d{closingItalic}{closingParagraph}");
            //<p><em>a</em>b<em>d</em></p>
        }

        [Test]
        public void DoesNotPutAnyEmTags_WhenSingleUnderscoresAreNextToDigits()
        {
            markdownParser.RenderToHtml("a_1_2").Should().Be($"{openingParagraph}a_1_2{closingParagraph}");
            //<p>a_1_2</p>
        }
		
        #endregion

        #region DoubleUnderscore


        [Test]
        public void PutsOneStrongTag_WhenOneGivenOneWordSurroundedByDoubleUnderscores()
        {
            markdownParser.RenderToHtml("__ad__").Should()
                .Be($"{openingParagraph}{openingStrong}a" +
                    $"d{closingStrong}{closingParagraph}");
            //<p><strong>ad</strong></p>
        }
		
        [Test]
        public void DoesNotPutAnyStrongTags_WhenOpeningDoubleUnderscoreIsFollowedByWhitespaceCharacter()
        {
            markdownParser.RenderToHtml("__ abcd__").Should()
                .Be($"{openingParagraph}__ abcd__{closingParagraph}");
            //<p>__ abcd __</p>
        }
		
        [Test]
        public void PutsTwoStrongTags_WhenTwoDoubleUnderscoreScopesAreSeparatedByLetter()
        {
            markdownParser.RenderToHtml("__a__b__d__").Should()
                .Be($"{openingParagraph}{openingStrong}a{closingStrong}" +
                    $"b{openingStrong}d{closingStrong}{closingParagraph}");
            //<p><strong>a</strong><strong>b</strong><strong>b</strong></p>
        }
		
        #endregion

        #region SingleAndDoubleUnderscores

        [Test]
        public void PutsOnlyEmTag_WhenDoubleUnderscoreIsInsideSingleUnderscoreScope()
        {
            markdownParser.RenderToHtml("_ab __cd__ ef_").Should().Be(
                $"{openingParagraph}{openingItalic}ab __cd__ ef{closingItalic}{closingParagraph}");
            //<p><em>ab __cd__ ef</em></p>
        }

        [Test]
        public void PutsBothEmAndStrongTags_WhenSingleUnderscoreIsInsideDoubleUnderscoreScope()
        {
            markdownParser.RenderToHtml("__ab _cd_ ef__").Should().Be(
                $"{openingParagraph}{openingStrong}ab {openingItalic}cd{closingItalic} ef{closingStrong}{closingParagraph}");
            //<p><strong>ab <em>cd</em> ef</strong></p>
        }

        [Test]
        public void DoesNotPutAnyTags_WhenUnderscoresAreImpaired()
        {
            markdownParser.RenderToHtml("__ab_").Should().Be(
                $"{openingParagraph}__ab_{closingParagraph}");
            //<p>__ab_</p>
        }

        #endregion

        #region CodeTag

        [Test]
        public void PutsCodeTag_WhenOneWordIsSurroundedByBackQuote()
        {
            markdownParser.RenderToHtml("`ab`").Should().Be(
                $"{openingParagraph}{openingCode}ab{closingCode}{closingParagraph}");
            //<p><code>ab</code></p>
        }

        #endregion
    }
}