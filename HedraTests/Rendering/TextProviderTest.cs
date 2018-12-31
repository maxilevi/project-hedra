using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    [TestFixture]
    public class TextProviderTest
    {
        [Test]
        public void TestMultipleProperties()
        {
            var str = $"Hi, how ${TextFormatting.Bigger}{TextFormatting.Gold}{TextFormatting.Bold}{{are you?}}";
            var defaultFont = FontCache.Get(AssetManager.NormalFamily, 1);
            var output = TextProvider.BuildParams(str, defaultFont, Color.White);
            output.Texts.ToList().ForEach(TestContext.WriteLine);
            output.TextColors.ToList().ForEach(C => TestContext.WriteLine(C.ToString()));
            output.TextFonts.ToList().ForEach(F => TestContext.WriteLine($"Font: {F.Size} {F.Style} {F.FontFamily}"));
            Assert.AreEqual(new []
            {
                "Hi, how ", 
                "are you?"
            }, output.Texts);
            Assert.AreEqual(new []
            {
                Color.White,
                Color.Gold
            }, output.TextColors);
            Assert.AreEqual(new []
            {
                defaultFont,
                FontCache.Get(AssetManager.BoldFamily, 1.5f, FontStyle.Bold),
            }, output.TextFonts);
        }
        
        [Test]
        public void TestSplittingWithNewlines()
        {
            var str = $"Hi, how {Environment.NewLine} I am $(GREEN){{very}}{Environment.NewLine} good!";
            var defaultFont = FontCache.Get(AssetManager.BoldFamily, 1, FontStyle.Bold);
            var output = TextProvider.BuildParams(str, defaultFont, Color.White);
            output.Texts.ToList().ForEach(TestContext.WriteLine);
            Assert.AreEqual(new []
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
            var defaultFont = FontCache.Get(AssetManager.BoldFamily, 1, FontStyle.Bold);
            var output = TextProvider.BuildParams(str, defaultFont, Color.White);
            output.Texts.ToList().ForEach(TestContext.WriteLine);
            output.TextColors.ToList().ForEach(C => TestContext.WriteLine(C.ToString()));
            Assert.AreEqual(new []
            {
                "Hi, how ", 
                "are you?",
                " I am ", 
                "very",
                " good!"
            }, output.Texts);
            Assert.AreEqual(new []
            {
                Color.White,
                Color.Red,
                Color.White,
                Color.LawnGreen,
                Color.White
            }, output.TextColors);
            Assert.AreEqual(new []
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