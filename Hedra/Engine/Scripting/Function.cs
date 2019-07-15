using System;
using System.Dynamic;
using Hedra.Engine.IO;
using Microsoft.Scripting.Hosting;

namespace Hedra.Engine.Scripting
{
    public class Function
    {
        private readonly object _base;
        private readonly string _library;
        private readonly string _name;
        private readonly ScriptEngine _engine;
        
        public Function(object Base, string Library, string Name, ScriptEngine Engine)
        {
            _base = Base;
            _library = Library;
            _name = Name;
            _engine = Engine;
        }

        public T Invoke<T>(params object[] Args)
        {
            return (T) Invoke(Args);
        }
        public object Invoke(params object[] Args)
        {
            var result = default(object);
            try
            {
                result = _engine.Operations.Invoke(_base, Args);
            }
            catch (Exception e)
            {
                Log.WriteLine(_engine.GetService<ExceptionOperations>().FormatException(e));
            }
            return result;
        }
    }
}