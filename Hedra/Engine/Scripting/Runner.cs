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

        public dynamic GetFunction(string Library, string Function)
        {
            return DoRun(ParseLibraryName(Library)).GetVariable(Function);
        }
        
        public T GetConstant<T>(string Library, string Variable)
        {
            return (T) DoRun(ParseLibraryName(Library)).GetVariable(Variable);
        }

        public ScriptScope GetScript(string Library)
        {
            return DoRun(ParseLibraryName(Library));
        }

        private string ParseLibraryName(string Library)
        {
            return Library.EndsWith(".py") ? Library : Library + ".py";
        }
        
        protected abstract ScriptScope DoRun(string Library);
    }
}