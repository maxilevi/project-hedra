using System;
using System.IO;
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
    }
}