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
            var fails = new List<string>();
            foreach (var pair in _shaderContents)
            {
                var shaderCode = pair.Value;
                var regex = new Regex(@".*[\d]+f.*|.*[\d]+F.*|.*[\d]+d.*");
                var matches = regex.Matches(shaderCode);
                for (var i = 0; i < matches.Count; i++)
                {
                    var match = matches[i];
                    fails.Add($"Invalid suffix literal at file '{pair.Key}', line '{LineFromIndex(shaderCode, match.Index)}'");
                }
            }
            if(fails.Count > 0) Assert.Fail(string.Join(Environment.NewLine, fails.ToArray()));
        }

        [Test]
        public void TestNumbersUseDecimals()
        {
            var fails = new List<string>();
            foreach (var pair in _shaderContents)
            {
                var shaderCode = pair.Value;
                var regex = new Regex(
                    @"(?i)textureSize.*|int.*|layout.*|\/\/.*|#.*|\[.+]|(?<=[a-zA-Z])[\d]+|[\d]+(?=\.)|\.[\d]+|([\d]+.*)");
                var matches = regex.Matches(shaderCode);
                for (var i = 0; i < matches.Count; i++)
                {
                    var match = matches[i].Groups[1];
                    if(match.Value == string.Empty) continue;
                    fails.Add($"Invalid int '{match.Value}' at file '{pair.Key}' line '{LineFromIndex(shaderCode, match.Index)}'");
                }
            }
            if(fails.Count > 0) Assert.Fail(string.Join(Environment.NewLine, fails.ToArray()));
        }
    }
}