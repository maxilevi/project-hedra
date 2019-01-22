using System;
using System.Collections.Generic;
using System.IO;
using Hedra.AISystem.Behaviours;
using NUnit.Framework;

namespace HedraTests.CodePolicy
{
    public class BehaviourCodePolicy : BaseCodePolicy
    {
        private readonly string[] _exceptions = new[]
        {
            nameof(TraverseBehaviour),
        };

        [Test]
        public void TestThereAreNoWalkBehavioursOutsideOfTraverse()
        {
            var filesAndCalls = GetAllFilesThatMatch($@"new\s+{nameof(WalkBehaviour)}\s*\(");
            var fails = new List<string>();
            foreach (var pair in filesAndCalls)
            {
                var name = Path.GetFileNameWithoutExtension(pair.Key);
                if(Array.IndexOf(_exceptions, name) != -1) continue;
                fails.Add($"{nameof(WalkBehaviour)} usage shouldn't be used in '{name}.cs'");
            }
            if(fails.Count > 0) Assert.Fail(string.Join(Environment.NewLine, fails.ToArray()));
        }
    }
}