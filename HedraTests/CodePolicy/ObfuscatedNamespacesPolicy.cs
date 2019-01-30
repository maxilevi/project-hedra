using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Hedra.Engine;
using NUnit.Framework;

namespace HedraTests.CodePolicy
{
    [TestFixture]
    public class ObfuscatedNamespacesPolicy : BaseCodePolicy
    {
        private readonly string[] _exceptions =
        {
            "Hedra.Engine.EntitySystem.BossSystem",
            "Hedra.Engine.EntitySystem"
        };
        
        [Test]
        public void TestThatExposedNamespacesAreNotObfuscated()
        {
            var fails = new List<string>();
            var exceptionUsed = new bool[_exceptions.Length];
            var allFiles = Directory.GetFiles($"{SolutionDirectory}/Hedra/", "*.cs", SearchOption.AllDirectories);
            var namespaceSet = new HashSet<string>();
            for (var i = 0; i < allFiles.Length; ++i)
            {
                var extractedNamespace = Regex.Match(File.ReadAllText(allFiles[i]), @"namespace\s+(.*?)\s*{").Groups[1].Value;
                if(string.IsNullOrEmpty(extractedNamespace)) continue;
                if (!namespaceSet.Contains(extractedNamespace))
                    namespaceSet.Add(extractedNamespace);
            }

            var allNamespaces = namespaceSet.ToArray();
            var obfuscatorSettings = File.ReadAllText(allFiles.First(S => S.Contains("ObfuscatorSettings")));
            for (var i = 0; i < allNamespaces.Length; ++i)
            {
                if(Regex.IsMatch(allNamespaces[i], @"^((?!Engine).)*$") && !IsExcepted(obfuscatorSettings, allNamespaces[i]))
                    fails.Add($"namespace '{allNamespaces[i]}' should not be obfuscated.");

                if(IsTemplate(allNamespaces[i]) && !IsExcepted(obfuscatorSettings, allNamespaces[i]))
                    fails.Add($"Template namespace '{allNamespaces[i]}' should not be obfuscated.");
                
                if (Regex.IsMatch(allNamespaces[i], @"Hedra\.Engine\.") &&
                    IsExcepted(obfuscatorSettings, allNamespaces[i]) && !IsTemplate(allNamespaces[i]))
                {
                    var index = -1;
                    if ((index = Array.IndexOf(_exceptions, allNamespaces[i])) == -1)
                        fails.Add($"namespace '{allNamespaces[i]}' should be obfuscated.");
                    else
                        exceptionUsed[index] = true;
                }
            }
            if(fails.Count > 0) Assert.Fail(string.Join(Environment.NewLine, fails.ToArray()));
            var k = 0;
            exceptionUsed.ToList().ForEach(B =>
            {
                if(!B) Assert.Fail($"Exception {_exceptions[k]} is not used anymore.");
                k++;
            });
        }

        private static bool IsTemplate(string Namespace)
        {
            return Regex.IsMatch(Namespace, @"\.Templates$");
        }

        private static bool IsExcepted(string ObfuscatorSettings, string Namespace)
        {
            return Regex.IsMatch(ObfuscatorSettings, $@"'{Namespace}'");
        }
    }
}