using System;
using System.Collections.Generic;
using System.IO;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Hedra.Engine.Scripting
{
    public class RawRunner : Runner
    {
        private readonly Dictionary<string, string> _sources;
        private readonly Dictionary<string, CompiledCode> _scripts;
        public RawRunner(ScriptEngine Engine) : base(Engine)
        {
            _scripts = new Dictionary<string, CompiledCode>();
            _sources = new Dictionary<string, string>();
        }

        public override void Load()
        {
        }

        protected override ScriptScope DoRun(string Library)
        {
            var scope = Engine.CreateScope();
            scope.SetVariable("player", GameManager.PlayerExists ? GameManager.Player : null);
            var code = GetOrCompile(Library);
            if (code != null)
                code.Execute(scope);
            else
                Log.WriteLine("Failed to execute python call.");
            return scope;
        }
        
        private CompiledCode GetOrCompile(string Library)
        {
            CheckForChanges(Library);
            if (_scripts.ContainsKey(Library)) return _scripts[Library];
            var source = _sources[Library];
            var code = (CompiledCode) null;
            if (source != null)
            {
                var scriptSource = Engine.CreateScriptSourceFromString(source);
                var listener = new ErrorReporter();
                code = scriptSource.Compile(listener);
                if (listener.Count == 0)
                    _scripts.Add(Library, code);
                else
                    listener.LogAll(Library);
                
            }
            return code;
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
                Log.WriteLine(e);
                return null;
            }
        }

        private static string DirectoryPath => GameSettings.DebugMode ? "../../Scripts/" : $"{AssetManager.AppPath}/Scripts/";
        protected bool WatchChanges => true;
    }
}