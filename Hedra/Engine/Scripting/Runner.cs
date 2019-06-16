using Hedra.Core;
using Microsoft.Scripting.Hosting;

namespace Hedra.Engine.Scripting
{
    public abstract class Runner
    {
        protected string CoreLibrary = "Core.py";
        protected readonly ScriptEngine Engine;

        protected Runner(ScriptEngine Engine)
        {
            this.Engine = Engine;
        }
        public abstract void Load();

        public dynamic Run(string Library, string Function)
        {
            return DoRun(ParseLibraryName(Library), Function);
        }
        
        public T GetConstant<T>(string Library, string Variable)
        {
            return (T) DoRun(ParseLibraryName(Library), Variable);
        }

        private string ParseLibraryName(string Library)
        {
            return Library.EndsWith(".py") ? Library : Library + ".py";
        }
        
        protected abstract dynamic DoRun(string Library, string Function);
    }
}