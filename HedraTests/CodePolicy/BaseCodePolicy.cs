using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace HedraTests.CodePolicy
{
    public abstract class BaseCodePolicy
    {
        protected string SolutionDirectory { get; }
        
        protected BaseCodePolicy()
        {
            SolutionDirectory = Directory.GetParent(TestContext.CurrentContext.TestDirectory).Parent.Parent.FullName;
        }
        
        protected int LineFromIndex(string Contents, int Index)
        {
            var res = 1;
            for (var i = 0; i <= Index - 1; i++)
                if (Contents[i] == '\n') res++;
            return res;
        }
        
        protected Dictionary<string, string[]> GetAllFilesThatMatch(string RegexString)
        {
            var calls = new Dictionary<string, string[]>();
            var files = Directory.GetFiles($"{SolutionDirectory}/Hedra/", "*.cs", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
            {
                var matches = Regex.Matches(File.ReadAllText(files[i]), RegexString);
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

        protected string[] GetAllModules()
        {
            return Directory.GetFiles($"{SolutionDirectory}/Hedra/Modules/", "*.json", SearchOption.AllDirectories);
        }

        protected string[] GetAllScripts()
        {
            return Directory.GetFiles($"{SolutionDirectory}/Hedra/Scripts/", "*.py", SearchOption.AllDirectories);
        }
    }
}