using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace HedraTests.CodePolicy
{
    [TestFixture]
    public class SolutionTest : BaseCodePolicy
    {
        [Test]
        public void TestSolutionDoesntContainCopyAlways()
        {
            const string regex = "<CopyToOutputDirectory>\\s*Always\\s*</CopyToOutputDirectory>";
            var contents = File.ReadAllText($"{SolutionDirectory}/Hedra/Hedra.csproj");
            var matches = Regex.Matches(contents, regex);

            var concatenatedString = string.Empty;
            for (var i = 0; i < matches.Count; i++)
            {
                concatenatedString +=
                    $"Found copy always option at line {LineFromIndex(contents, matches[i].Index)}{Environment.NewLine}";
            }
            if(matches.Count > 0) Assert.Fail(concatenatedString);
        }
    }
}