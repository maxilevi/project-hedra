using System;
using System.Collections.Generic;
using Hedra.Engine.IO;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Hedra.Engine.Scripting
{
    public class ErrorReporter : ErrorListener
    {
        private readonly List<string> _errors = new List<string>();

        public int Count => _errors.Count;

        public override void ErrorReported(ScriptSource Source, string Message, SourceSpan Span, int ErrorCode,
            Severity Severity)
        {
            _errors.Add(Message);
        }

        public void LogAll(string LibraryName)
        {
            Log.WriteLine($"ERRORS FOUND WHEN COMPILING {LibraryName}:{Environment.NewLine}{Environment.NewLine}");
            for (var i = 0; i < _errors.Count; ++i) Log.WriteLine(_errors[i]);
        }
    }
}