using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using NUnit.Framework;

namespace HedraTests.CodePolicy
{
    [TestFixture]
    public class UnusedTranslationKeysCodePolicy : BaseCodePolicy
    {
        private readonly string[] _exceptions =
        {
            @"_\d",
            @"_key",
            "warrior",
            "archer",
            "rogue",
            "berserker",
            "paladin",
            "druid",
            "necromancer",
            "hunter",
            "scout",
            "ninja",
            "assassin",
            "requires_.+"
        };
        
        [Test]
        public void TestThereAreNoUnusedKeys()
        {
            var englishKeys = IniParser.Parse(File.ReadAllText($"{GameLoader.AppPath}/Translations/English.po"))
                .Select(P => P.Key)
                .Where(K => _exceptions.ToList().All(E => !Regex.IsMatch(K, E)))
                .ToArray();
            var set = new HashSet<string>(englishKeys);
            var sourceFiles = GetAllFilesThatMatch(@"\bTranslation[|s]*\b")
                .Select(P => P.Key)
                .ToArray();
            for (var k = 0; k < sourceFiles.Length; ++k)
            {
                for (var i = 0; i < englishKeys.Length; ++i)
                {
                    var source = File.ReadAllText(sourceFiles[k]);
                    if (source.Contains(englishKeys[i]))
                        set.Remove(englishKeys[i]);                    
                }
                englishKeys = set.ToArray();
            }
            if(set.Count > 0) Assert.Fail(string.Join(Environment.NewLine, set.Select(S => $"Translation key '{S}' seems to be unused. Please remove it.").ToArray()));
        }
    }
}