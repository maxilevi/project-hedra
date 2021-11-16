using System;
using System.Collections.Generic;
using System.IO;
using Hedra.Engine.IO;
using Hedra.Game;
using Microsoft.Scripting.Hosting;

namespace Hedra.Engine.Scripting
{
    public class CompiledRunner : Runner
    {
        private readonly Dictionary<string, ScriptScope> _scripts;
        private readonly Dictionary<string, DateTime> _sources;

        public CompiledRunner(ScriptEngine Engine) : base(Engine)
        {
            _scripts = new Dictionary<string, ScriptScope>();
            _sources = new Dictionary<string, DateTime>();
        }

        private static string DirectoryPath => Interpreter.SearchPath;
        private static bool WatchChanges => GameSettings.WatchScriptChanges;

        public override void Reload()
        {
            _scripts.Clear();
            _sources.Clear();
        }

        protected override ScriptScope DoRun(string Library)
        {
            return GetOrCompile(Library);
        }

        public override void Prepare(string Library)
        {
            Log.WriteLine($"Loading {Library}...");
            DoRun(Library);
        }

        private ScriptScope GetOrCompile(string Library)
        {
            if (!WatchChanges && _scripts.ContainsKey(Library)) return _scripts[Library];
            var lastWrite = _sources.ContainsKey(Library) ? _sources[Library] : DateTime.MinValue;
            if (ScriptChanged(Library, lastWrite, out var newWrite)) return Compile(Library, Get(Library), newWrite);
            return _scripts[Library];
        }

        private ScriptScope Compile(string Name, string Source, DateTime Date)
        {
            var scope = Engine.CreateScope();
            if (Source != null)
            {
                var scriptSource = Engine.CreateScriptSourceFromString(Source);
                scope.SetVariable("player", GameManager.PlayerExists ? GameManager.Player : null);
                var listener = new ErrorReporter();
                var compiled = scriptSource.Compile(listener);
                if (listener.Count == 0)
                {
                    if (Execute(Name, compiled, scope))
                    {
                        if (_scripts.ContainsKey(Name))
                            _scripts.Remove(Name);

                        _scripts.Add(Name, scope);

                        if (_sources.ContainsKey(Name))
                            _sources.Remove(Name);

                        _sources.Add(Name, Date);
                    }
                }
                else
                {
                    listener.LogAll(Name);
                }
            }

            return scope;
        }

        private bool Execute(string Name, CompiledCode Code, ScriptScope Scope)
        {
            try
            {
                Code.Execute(Scope);
            }
            catch (Exception e)
            {
                ReportFailure(Name, e);
                return false;
            }

            return true;
        }

        private static bool ScriptChanged(string Name, DateTime LastTime, out DateTime NewWrite)
        {
            NewWrite = default;
            try
            {
                NewWrite = File.GetLastWriteTime($"{DirectoryPath}{Name}");
                return !NewWrite.Equals(LastTime);
            }
            catch (Exception e)
            {
                ReportFailure(Name, e);
                return false;
            }
        }

        private static string Get(string Name)
        {
            try
            {
                return File.ReadAllText($"{DirectoryPath}{Name}");
            }
            catch (Exception e)
            {
                ReportFailure(Name, e);
                return null;
            }
        }
    }
}