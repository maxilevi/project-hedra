using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Hedra.Engine.Localization;
using NUnit.Framework;

namespace HedraTests.CodePolicy
{
    [TestFixture]
    public class ControlsCodePolicy : BaseCodePolicy
    {
        private string _controlsClassName;
        /* These functions have multiple versions on the spec, to avoid issues we add them as exceptions. */
        private readonly string[] _exceptions =
        {
            "DebugMessageCallback",
        };

        [SetUp]
        public void Setup()
        {
            _controlsClassName = typeof(Controls).ToString();
        }

        [Test]
        public void TestThereAreNoKeysOutsideOfControls()
        {
            var filesAndCalls = GetAllKeyUsages();
            var fails = new List<string>();
            foreach (var pair in filesAndCalls)
            {
                var name = Path.GetFileNameWithoutExtension(pair.Key);
                if(name != _controlsClassName)
                    fails.Add($"GL calls in '{name}.cs' should be in {_controlsClassName}.cs");
            }
            if(fails.Count > 0) Assert.Fail(string.Join(Environment.NewLine, fails.ToArray()));
        }
 

        private Dictionary<string, string[]> GetAllKeyUsages()
        {
            var calls = new Dictionary<string, string[]>();
            var files = Directory.GetFiles($"{SolutionDirectory}/Hedra/Engine/", "*.cs", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
            {
                var matches = Regex.Matches(File.ReadAllText(files[i]), @"Key\.");
                if (matches.Count > 0)
                {
                    var matchList = new List<string>();
                    for (var k = 0; k < matches.Count; k++)
                    {
                        var str = matches[k].Value;
                        matchList.Add(str.Substring(3, str.Length-4));
                    }
                    calls.Add(files[i], matchList.ToArray());
                }
            }
            
            return calls;
        }
    }
}