using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace HedraTests.CodePolicy
{
    [TestFixture]
    public class DanglingReferencesCodePolicy : BaseCodePolicy
    {
        [Test]
        public void TestNoUpdateManager()
        {
            AssertBothInSameFile(@"UpdateManager\.Add", @"UpdateManager\.Remove", "UpdateManager");
        }
        
        [Test]
        public void TestNoDrawManager()
        {
            AssertBothInSameFile(@"DrawManager\.Add", @"DrawManager\.Remove", "DrawManager");
        }
        
        [Test]
        public void TestNoEventDispatcher()
        {
            AssertBothInSameFile(@"EventDispatcher\.Add", @"EventDispatcher\.Remove", "EventDispatcher");
        }
        
        [Test]
        public void TestNoKeyDown()
        {
            AssertBothInSameFile(@"EventDispatcher\.RegisterKeyDown", @"EventDispatcher\.UnregisterKeyDown", "RegisterKeyDown");
        }
        
        [Test]
        public void TestNoKeyUp()
        {
            AssertBothInSameFile(@"EventDispatcher\.RegisterKeyUp", @"EventDispatcher\.UnregisterKeyUp", "RegisterKeyUp");
        }
        
        [Test]
        public void TestNoKeyPress()
        {
            AssertBothInSameFile(@"EventDispatcher\.RegisterKeyPress", @"EventDispatcher\.UnregisterKeyPress", "RegisterKeyPress");
        }
        
        [Test]
        public void TestNoMouseDown()
        {
            AssertBothInSameFile(@"EventDispatcher\.RegisterMouseDown", @"EventDispatcher\.UnregisterMouseDown", "RegisterMouseDown");
        }

        [Test]
        public void TestNoMouseUp()
        {
            AssertBothInSameFile(@"EventDispatcher\.RegisterMouseUp", @"EventDispatcher\.UnregisterMouseUp", "RegisterMouseUp");
        }
        
        [Test]
        public void TestNoMouseMove()
        {
            AssertBothInSameFile(@"EventDispatcher\.RegisterMouseMove", @"EventDispatcher\.UnregisterMouseMove", "RegisterMouseMove");
        }
        
        private void AssertBothInSameFile(string PatternA, string PatternB, string Name)
        {
            var files = Directory.GetFiles($"{SolutionDirectory}/Hedra/", "*.cs", SearchOption.AllDirectories);
            var failures = new List<string>();
            for (var i = 0; i < files.Length; i++)
            {
                var contents = File.ReadAllText(files[i]);
                if (Regex.IsMatch(contents, PatternA))
                {
                    if (!Regex.IsMatch(contents, PatternB))
                        failures.Add($"Class '{files[i]}' is added to the '{Name}' but is never removed.");
                }
            }
            if(failures.Count > 0) Assert.Fail(string.Join(Environment.NewLine, failures.ToArray()));
        }
    }
}