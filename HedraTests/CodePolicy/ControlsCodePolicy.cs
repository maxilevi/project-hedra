using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Hedra.Engine.Loader;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.PagedInterface;
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
            nameof(PlayerMovement),
            nameof(PagedInventoryArrayInterfaceManager)
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
            @"Enter",
            @"Up",
            @"Down",
            @"Left",
            @"Right"
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
            var filesAndCalls = GetAllFilesThatMatch(_regex);
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
    }
}