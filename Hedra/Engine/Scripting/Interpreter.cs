using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Game;
using Hedra.Rendering;
using IronPython.Hosting;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Hosting;
using OpenTK;

namespace Hedra.Engine.Scripting
{
    public static class Interpreter
    {
        private const string CoreLibrary = "Core.py";
        private static readonly ScriptEngine _engine;
        private static readonly Runner _runner;

        public static void Load()
        {
            var files = Directory.GetFiles(SearchPath, "*", SearchOption.AllDirectories);
            for (var j = 0; j < files.Length; ++j)
            {
                var name = Path.GetFileName(files[j]);
                if (!name.EndsWith(".py")) continue;
                _runner.Prepare(name);
            }
        }

        static Interpreter()
        {
            var watch = new Stopwatch();
            watch.Start();
            Log.WriteLine("Loading Python engine...");
            _engine = Python.CreateEngine();
            _engine.SetSearchPaths(new []{SearchPath});
            _engine.Runtime.LoadAssembly(Assembly.Load(typeof(Interpreter).Assembly.FullName));
            _engine.Runtime.LoadAssembly(Assembly.Load(typeof(Vector4).Assembly.FullName));
            _runner = new CompiledRunner(_engine);
            Log.WriteLine($"Python engine was successfully loaded in {watch.ElapsedMilliseconds} MS");

            watch.Reset();
        }

        public static Function GetFunction(string Library, string Function)
        {
            return new Function(_runner.GetFunction(Library, Function), Library, Function, _engine);
        }

        public static Script GetScript(string Library)
        {
            return new Script(Library);
        }

        public static Script[] GetScripts(string FolderPath)
        {
            var scripts = new List<Script>();
            var files = Directory.GetFiles($"{SearchPath}{FolderPath}", "*.py", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; ++i)
            {
                var name = Path.GetFileName(files[i]);
                if (!name.EndsWith(".py")) continue;
                scripts.Add(GetScript(name));
            }

            return scripts.ToArray();
        }

        public static T GetMember<T>(string Library, string Variable)
        {
            return (T) _runner.GetFunction(Library, Variable);
        }
        
        public static bool HasMember(string Library, string Variable)
        {
            return _runner.HasMember(Library, Variable);
        }

        public static void Reload()
        {
            _runner.Reload();
        }

        public static string SearchPath
        {

            get
            {
                if (GameSettings.DebugMode && !GameSettings.TestingMode)
                    return $"../../Scripts/";
                return $"{AssetManager.AppPath}/Scripts/";
            }
        }
    }
}