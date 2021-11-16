using System;
using SixLabors.ImageSharp;
using SixLabors.Fonts;
using System.Linq;
using System.Text.RegularExpressions;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Rendering.UI;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    [TestFixture]
    public class TextProviderTest
    {
        [Test]
        public void TestWrapping()
        {
            var str = "thank $(BOLD)(VIOLET){u}, next you";
            Assert.AreEqual(
                $"thank {Environment.NewLine}$(BOLD)(VIOLET){{u}}, {Environment.NewLine}next you",
                TextProvider.Wrap(str, 5)
            );
            str = "I am in need of $(ORANGE)(BOLD){3 MUSHROOM}. Can you help me?";
            Assert.AreEqual(
                $"I am in need of $(ORANGE)(BOLD){{3 MUSHROOM}}.{Environment.NewLine} Can you help me?",
                TextProvider.Wrap(str, 26)
            );
            str = "Using the items you collected for me I need you to craft me a $(ORANGE)(BOLD){HEALTH POTION}";
            Assert.AreEqual(
                $"Using the items you collected {Environment.NewLine}for me I need you to craft {Environment.NewLine}me a $(ORANGE)(BOLD){{HEALTH POTION}}",
                TextProvider.Wrap(str, 26)
            );
            /*
            str = "Using the items you collected I need you to craft me $(ORANGE)(BOLD){3 HEALTH POTION}";
            Assert.AreEqual(
                $"Using the items you collected {Environment.NewLine}I need you to craft me $(ORANGE)(BOLD){{3}}{Environment.NewLine}$(ORANGE)(BOLD){{}}$(ORANGE)(BOLD){{ HEALTH POTION}}",
                TextProvider.Wrap(str, 26)
            );*/
        }

        [Test]
        public void TestTextSubString()
        {
            var str = "thank $(BOLD)(VIOLET){u}, next";
            var realStr = TextProvider.StripFormat(str);
            Assert.AreEqual(
                TextProvider.Substr(str, 6),
                "thank "
            );
            Assert.AreEqual(
                TextProvider.Substr(str, 7),
                "thank $(BOLD)(VIOLET){u}"
            );
            Assert.AreEqual(
                TextProvider.Substr(str, realStr.Length),
                str
            );
        }

        [Test]
        public void TestMultipleProperties()
        {
            var str = $"Hi, how ${TextFormatting.Bigger}{TextFormatting.Gold}{TextFormatting.Bold}{{are you?}}";
            var defaultFont = FontCache.GetNormal(1);
            var output = TextProvider.BuildParams(str, defaultFont, Color.White);
            output.Texts.ToList().ForEach(TestContext.WriteLine);
            output.TextColors.ToList().ForEach(C => TestContext.WriteLine(C.ToString()));
            output.TextFonts.ToList().ForEach(F => TestContext.WriteLine($"Font: {F.Size} {F.Style} {F.FontFamily}"));
            Assert.AreEqual(new[]
            {
                "Hi, how ",
                "are you?"
            }, output.Texts);
            Assert.AreEqual(new[]
            {
                Color.White,
                Color.Gold
            }, output.TextColors);
            Assert.AreEqual(new[]
            {
                defaultFont,
                FontCache.GetBold(1.5f)
            }, output.TextFonts);
        }

        [Test]
        public void TestSplittingWithNewlines()
        {
            var str = $"Hi, how {Environment.NewLine} I am $(GREEN){{very}}{Environment.NewLine} good!";
            var defaultFont = FontCache.GetBold(1);
            var output = TextProvider.BuildParams(str, defaultFont, Color.White);
            output.Texts.ToList().ForEach(TestContext.WriteLine);
            Assert.AreEqual(new[]
            {
                "Hi, how ",
                Environment.NewLine,
                " I am ",
                "very",
                Environment.NewLine,
                " good!"
            }, output.Texts);
        }

        [Test]
        public void TestSplitting()
        {
            var str = "Hi, how $(RED){are you?} I am $(GREEN){very} good!";
            var defaultFont = FontCache.GetBold(1);
            var output = TextProvider.BuildParams(str, defaultFont, Color.White);
            output.Texts.ToList().ForEach(TestContext.WriteLine);
            output.TextColors.ToList().ForEach(C => TestContext.WriteLine(C.ToString()));
            Assert.AreEqual(new[]
            {
                "Hi, how ",
                "are you?",
                " I am ",
                "very",
                " good!"
            }, output.Texts);
            Assert.AreEqual(new[]
            {
                Color.White,
                Color.Red,
                Color.White,
                Color.LawnGreen,
                Color.White
            }, output.TextColors);
            Assert.AreEqual(new[]
            {
                defaultFont,
                defaultFont,
                defaultFont,
                defaultFont,
                defaultFont
            }, output.TextFonts);
        }
    }
}