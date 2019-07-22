using System;
using Hedra.Core;
using Hedra.Engine.IO;
using Microsoft.Scripting.Hosting;

namespace Hedra.Engine.Scripting
{
    public abstract class Runner
    {
        protected readonly ScriptEngine Engine;

        protected Runner(ScriptEngine Engine)
        {
            this.Engine = Engine;
        }
        public object GetFunction(string Library, string Function)
        {
            try
            {
                var scope = DoRun(Library);
                return scope.ContainsVariable(Function) ? scope.GetVariable(Function) : null;
            }
            catch (Exception e)
            {
                ReportFailure($"{Library}::{Function}", e);
            }

            return null;
        }

        public bool HasMember(string Library, string Member)
        {
            var scope = DoRun(Library);
            return scope.ContainsVariable(Member);
        }

        public abstract void Reload();

        public abstract void Prepare(string Library);
        
        protected static void ReportFailure(string Name, Exception Exception)
        {
            Log.WriteLine($"INTERPRETER PANIC at '{Name}':{Environment.NewLine}{Environment.NewLine}{Exception}");
        }

        protected abstract ScriptScope DoRun(string Library);
    }
}