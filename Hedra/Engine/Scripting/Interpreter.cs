using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using OpenTK;

namespace Hedra.Engine.Scripting
{
    public static class Interpreter
    {
        private const string CoreLibrary = "Core.py";
        private static readonly ScriptEngine _engine;
        private static readonly Runner _runner;
        
        public static void Load(){}
        
        static Interpreter()
        {
            var watch = new Stopwatch();
            watch.Start();
            Log.WriteLine("Loading Python engine...");
            _engine = Python.CreateEngine();
            _engine.SetSearchPaths(GetSearchPath());
            _engine.Runtime.LoadAssembly(Assembly.Load(typeof(Interpreter).Assembly.FullName));
            _engine.Runtime.LoadAssembly(Assembly.Load(typeof(Vector4).Assembly.FullName));
            _runner = GameSettings.DebugMode ? (Runner) new RawRunner(_engine) : new CompiledRunner(_engine);
            Log.WriteLine($"Python engine was successfully loaded in {watch.ElapsedMilliseconds} MS");
            
            watch.Reset();
        }

        public static dynamic Run(string Library, string Function)
        {
            return _runner.Run(Library, Function);
        }

        public static T GetConstant<T>(string Library, string Variable)
        {
            return _runner.GetConstant<T>(Library, Variable);
        }


        private static string[] GetSearchPath()
        {
#if DEBUG
            return new [] {$"../../Scripts/"};
#else
            return new [] {$"{AssetManager.AppPath}/Scripts/"};
#endif
        }
    }
}