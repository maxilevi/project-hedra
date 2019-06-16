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
            Log.WriteLine($"Python engine was successfully loaded in {watch.ElapsedMilliseconds} MS");
            watch.Reset();
        }

        public static dynamic Run(string Library, string Function)
        {
            var scope = _engine.CreateScope();
            scope.SetVariable("player", GameManager.Player);
            _engine.Execute(LoadSource(CoreLibrary), scope);
            _engine.Execute(LoadSource(Library), scope);
            var wrapper = scope.GetVariable(Function);
            return wrapper;
        }


        private static string[] GetSearchPath()
        {
#if DEBUG
            return new [] {$"../../Scripts/"};
#else
            return new [] {$"{AssetManager.AppPath}/Scripts/"};
#endif
        }
        
        private static string LoadSource(string Name)
        {
            Name = Name.EndsWith(".py") ? Name : Name + ".py";
#if DEBUG
            return File.ReadAllText($"../../Scripts/{Name}");
#else
            return File.ReadAllText($"{AssetManager.AppPath}/Scripts/{Name}");
#endif
        }
    }
}