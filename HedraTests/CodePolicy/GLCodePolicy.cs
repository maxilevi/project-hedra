using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.UI;
using NUnit.Framework;

namespace HedraTests.CodePolicy
{
    [TestFixture]
    public class GLCodePolicy : BaseCodePolicy
    {
        private string _documentation;
        private const float MaxVersion = 3.3f;
        private string _glClassName;
        private string _debugDrawerClassName;
        /* These functions have multiple versions on the spec, to avoid issues we add them as exceptions. */
        private readonly string[] _exceptions =
        {
            "DebugMessageCallback",
            "Uniform1",
            "Uniform2",
            "Uniform3",
            "Uniform4",
            "UniformMatrix2",
            "UniformMatrix2x3",
            "UniformMatrix3",
            "UniformMatrix4",
        };
        
        [SetUp]
        public void Setup()
        {
            _documentation = File.ReadAllText($"{base.SolutionDirectory}/references/gl.xml");
            _glClassName = typeof(GLProvider).Name;
            _debugDrawerClassName = typeof(BasicGeometry).Name;
        }

        [Test]
        public void TestThereAreNoGLVersionsOutsideProvider()
        {
            var filesAndCalls = GetAllGLCalls();
            var fails = new List<string>();
            foreach (var pair in filesAndCalls)
            {
                var name = Path.GetFileNameWithoutExtension(pair.Key);
                if(name != _glClassName && name != _debugDrawerClassName)
                    fails.Add($"GL calls in '{name}.cs' should be in {_glClassName}.cs");
            }
            if(fails.Count > 0) Assert.Fail(string.Join(Environment.NewLine, fails.ToArray()));
        }
        
        [Test]
        public void TestAllGLCallsVersionAreLowerThanTheLimit()
        {
            var filesAndCalls = GetAllGLCalls();
            var fails = new List<string>();
            foreach (var pair in filesAndCalls)
            {
                var calls = pair.Value;
                var file = pair.Key;
                for (var i = 0; i < calls.Length; i++)
                {
                    var ver = GetRequiredVersion(calls[i]);
                    if (ver > MaxVersion && Array.IndexOf(_exceptions, calls[i]) == -1)
                        fails.Add(
                            $"Call 'GL.{calls[i]}' in '{file}' has a minium version of '{ver}' and the limit is '{MaxVersion}'");
                }
            }

            if(fails.Count > 0) Assert.Fail(string.Join(Environment.NewLine, fails.ToArray()));
        }

        private Dictionary<string, string[]> GetAllGLCalls()
        {
            var calls = new Dictionary<string, string[]>();
            var files = Directory.GetFiles($"{SolutionDirectory}/Hedra/", "*.cs", SearchOption.AllDirectories);
            var regexes = new []
            {
                @"GL\.([a-zA-Z0-9]+)\(",
                @"_gl\.([a-zA-Z0-9]+)\("
            };
            for (var i = 0; i < files.Length; i++)
            {
                var matchList = new List<string>();
                for (var j = 0; j < regexes.Length; ++j)
                {
                    var source = File.ReadAllText(files[i]);
                    var matches = Regex.Matches(source, regexes[j]);
                    if (matches.Count > 0)
                    {
                        for (var k = 0; k < matches.Count; k++)
                        {
                            var str = matches[k].Groups[1].Value;
                            matchList.Add(str);
                        }
                    }
                }
                if(matchList.Count > 0)
                    calls.Add(files[i], matchList.ToArray());
            }
            
            return calls;
        }
        
        private float GetRequiredVersion(string FunctionName)
        {
            if (FunctionName.Equals("GetApi", StringComparison.InvariantCultureIgnoreCase)) return 0f;
            var firstMatch = Regex.Match(_documentation, @"M:OpenTK\.Graphics\.OpenGL\.GL\." + FunctionName + @"[\(\" + "\"" + @"].*\s*.*");
            var match = Regex.Match(firstMatch.Value, @"(requires: v)([0-9]\.[0-9])");
            return float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture.NumberFormat);
        }
    }
}