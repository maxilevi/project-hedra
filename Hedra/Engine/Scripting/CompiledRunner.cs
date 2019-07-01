using System;
using System.Collections.Generic;
using System.IO;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Game;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Hedra.Engine.Scripting
{
    public class CompiledRunner : Runner
    {
        private readonly Dictionary<string, string> _sources;
        private readonly Dictionary<string, ScriptScope> _scripts;
        public CompiledRunner(ScriptEngine Engine) : base(Engine)
        {
            _scripts = new Dictionary<string, ScriptScope>();
            _sources = new Dictionary<string, string>();
        }

        public override void Load()
        {
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
            CheckForChanges(Library);
            if (_scripts.ContainsKey(Library)) return _scripts[Library];
            var source = _sources[Library];
            var scope = Engine.CreateScope();
            if (source != null)
            {
                var scriptSource = Engine.CreateScriptSourceFromString(source);
                scope.SetVariable("player", GameManager.PlayerExists ? GameManager.Player : null);
                var listener = new ErrorReporter();
                var compiled = scriptSource.Compile(listener);
                if (listener.Count == 0)
                {
                    if(Execute(Library, compiled, scope))
                        _scripts.Add(Library, scope);
                }
                else
                {
                    listener.LogAll(Library);
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
        
        private void CheckForChanges(string Library)
        {
            var source = _sources.ContainsKey(Library) ? _sources[Library] : null;
            if (!WatchChanges && source != null) return;
            var newSource = Get(Library);
            if (newSource != source)
            {
                if (_sources.ContainsKey(Library))
                    _sources[Library] = newSource;
                else
                    _sources.Add(Library, newSource);
                
                if(_scripts.ContainsKey(Library))
                    _scripts.Remove(Library);
                GetOrCompile(Library);
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

        private static string DirectoryPath => Interpreter.SearchPath;
        protected bool WatchChanges => GameSettings.DebugMode;
    }
}