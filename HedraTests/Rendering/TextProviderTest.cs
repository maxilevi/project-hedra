using System.Drawing;
using System.Linq;
using Hedra.Engine.Rendering.UI;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    [TestFixture]
    public class TextProviderTest
    {
        [Test]
        public void TestSplitting()
        {
            var str = "Hi, how $REDare you? I am $GREENvery good!";
            var output = TextProvider.BuildParams(str, null, Color.White);
            output.Texts.ToList().ForEach(TestContext.WriteLine);
            output.TextColors.ToList().ForEach(C => TestContext.WriteLine(C.ToString()));
            Assert.AreEqual(output.Texts, new []
            {
                "Hi, how ", 
                "are ", 
                "you? I am ", 
                "very ",
                "good!"
            });
            Assert.AreEqual(output.TextColors, new []
            {
                Color.White,
                Color.Red,
                Color.White,
                Color.LawnGreen,
                Color.White
            });
        }
    }
}