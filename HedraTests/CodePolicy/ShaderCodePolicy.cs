using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace HedraTests.CodePolicy
{
    [TestFixture]
    public class ShaderCodePolicy : BaseCodePolicy
    {
        private Dictionary<string, string> _shaderContents;

        public ShaderCodePolicy()
        {
            _shaderContents = new Dictionary<string, string>();
            var shaderDir = $"{SolutionDirectory}/Hedra/Shaders/";
            var files = Directory.GetFiles(shaderDir);
            for (var i = 0; i < files.Length; i++)
            {
                _shaderContents.Add(files[i], File.ReadAllText(files[i]));
            }
        }
        
        [Test]
        public void TestNumbersInDontUseSuffixLiterals()
        {
            this.AssertRegexDoesntMatch(
                new Regex(@".*[\d]+f.*|.*[\d]+F.*|.*[\d]+d.*"),
                "Invalid suffix literal"
            );
        }

        [Test]
        public void TestNumbersUseDecimals()
        {
            this.AssertRegexDoesntMatch(
                new Regex(@"(?i)textureSize.*|int.*|layout.*|\/\/.*|#.*|\[.+]|(?<=[a-zA-Z])[\d]+|[\d]+(?=\.)|\.[\d]+|([\d]+.*)"),
                "Invalid int",
                true);
        }
             
        [Test]
        public void TestShadersDontUseCompatibility()
        {
            this.AssertRegexDoesntMatch(
                new Regex(@"#.*?compatibility"),
                "Usage of compatibility mode"
            );
        }
        
        [Test]
        public void TestShadersDontUseDeprecatedFunctions()
        {
            this.AssertRegexDoesntMatch(
                new Regex(@"(gl_ModelViewMatrix|gl_ProjectionMatrix|gl_ModelViewProjectionMatrix)"),
                "Usage of deprecated functions"
            );
        }

        [Test]
        public void TestShadersUseVersion330()
        {
            this.AssertRegexDoesntMatch(
                new Regex(@"#version\s*((?!330).)+\s"),
                "Usage of invalid version"
            );
        }
        
        private void AssertRegexDoesntMatch(Regex Regex, string Message, bool CompareGroup = false)
        {
            var fails = new List<string>();
            foreach (var pair in _shaderContents)
            {
                var shaderCode = pair.Value;
                var matches = Regex.Matches(shaderCode);
                for (var i = 0; i < matches.Count; i++)
                {
                    var match = CompareGroup ? matches[i].Groups[1] : matches[i];
                    if(match.Value == string.Empty) continue;
                    fails.Add(
                        $"{Message} '{match.Value}' at file '{pair.Key}' line '{LineFromIndex(shaderCode, match.Index)}'");
                }
            }
            if(fails.Count > 0) Assert.Fail(string.Join(Environment.NewLine, fails.ToArray()));
        }
    }
}