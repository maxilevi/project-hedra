using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Hedra.Engine.Loader;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.UI;
using NUnit.Framework;

namespace HedraTests.CodePolicy
{
    [TestFixture]
    public class ControlsCodePolicy : BaseCodePolicy
    {
        private string _controlsClassName;
        private string _regex;
        private readonly string[] _exceptions = new[]
        {
            nameof(Chat),
            nameof(DebugInfoProvider),
            nameof(Panel),
            nameof(TextField),
            nameof(PlayerMovement)
        };
        private readonly string[] _regexExceptions =
        {
            @"ToString\(\)",
            @"VertexData",
            @"ToLowerInvariant\(\)",
            @"Setter",
            @"ReleaseFirst",
            @"Getter",
            /* Excluded key types*/
            @"Escape",
            @"Enter"
        };

        [SetUp]
        public void Setup()
        {
            _controlsClassName = typeof(Controls).Name;
            _regex = $@"Key\.(?!{string.Join("|", _regexExceptions)})";
        }

        [Test]
        public void TestThereAreNoKeysOutsideOfControls()
        {
            var filesAndCalls = GetAllKeyUsages();
            var fails = new List<string>();
            foreach (var pair in filesAndCalls)
            {
                var name = Path.GetFileNameWithoutExtension(pair.Key);
                if(Array.IndexOf(_exceptions, name) != -1) continue;
                if(name != _controlsClassName)
                    fails.Add($"OpenTK.Input.Key usages in '{name}.cs' should be in {_controlsClassName}.cs");
            }
            if(fails.Count > 0) Assert.Fail(string.Join(Environment.NewLine, fails.ToArray()));
        }
 

        private Dictionary<string, string[]> GetAllKeyUsages()
        {
            var calls = new Dictionary<string, string[]>();
            var files = Directory.GetFiles($"{SolutionDirectory}/Hedra/", "*.cs", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
            {
                var matches = Regex.Matches(File.ReadAllText(files[i]), _regex);
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