using System;
using System.Dynamic;
using Hedra.Engine.IO;
using Microsoft.Scripting.Hosting;

namespace Hedra.Engine.Scripting
{
    public class Function : DynamicObject
    {
        private readonly dynamic _base;
        private readonly string _library;
        private readonly string _name;
        private readonly ScriptEngine _engine;
        
        public Function(dynamic Base, string Library, string Name, ScriptEngine Engine)
        {
            _base = Base;
            _library = Library;
            _name = Name;
            _engine = Engine;
        }
        
        public override bool TryInvoke(InvokeBinder Binder, object[] Args, out object Result)
        {
            Result = null;
            try
            {
                Result = _engine.Operations.Invoke(_base, Args);
            }
            catch (Exception e)
            {
                Log.WriteLine($"Python call '{_name}' from '{_library}' failed with the following exception:\n\n{e}");
            }
            return true;
        }
    }
}