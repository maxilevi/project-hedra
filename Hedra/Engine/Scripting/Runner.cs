using System;
using Hedra.Core;
using Hedra.Engine.IO;
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
            try
            {
                var scope = DoRun(ParseLibraryName(Library));
                return scope.ContainsVariable(Function) ? scope.GetVariable(Function) : null;
            }
            catch (Exception e)
            {
                ReportFailure($"{Library}::{Function}", e);
            }

            return null;
        }

        public ScriptScope GetScript(string Library)
        {
            try
            {
                return DoRun(ParseLibraryName(Library));
            }
            catch (Exception e)
            {
                ReportFailure(Library, e);
            }

            return null;
        }

        protected static void ReportFailure(string Name, Exception Exception)
        {
            Log.WriteLine($"INTERPRETER PANIC at '{Name}':{Environment.NewLine}{Environment.NewLine}{Exception}");
        }

        private string ParseLibraryName(string Library)
        {
            return Library.EndsWith(".py") ? Library : Library + ".py";
        }
        
        protected abstract ScriptScope DoRun(string Library);
    }
}