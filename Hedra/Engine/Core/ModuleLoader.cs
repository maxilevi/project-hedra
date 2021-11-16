using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hedra.API;
using Hedra.Engine.IO;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Framework;
using Newtonsoft.Json;

namespace Hedra.Engine.Core
{
    public interface IModuleLoader
    {
        void DoLoadModules(string AppPath);
    }

    public abstract class ModuleLoader<T, U> : ModuleLoader<string, T, U>
        where T : class, IModuleLoader, new() where U : INamedTemplate
    {
        protected override string ParseKey(string Name)
        {
            return Name.ToLowerInvariant();
        }
    }

    public abstract class ModuleLoader<S, T, U> : Singleton<T>, IModuleLoader
        where T : class, IModuleLoader, new() where U : INamedTemplate
    {
        protected readonly object Lock = new object();
        protected readonly Dictionary<S, U> Templates = new Dictionary<S, U>();
        protected abstract string FolderPrefix { get; }

        public void DoLoadModules(string AppPath)
        {
            lock (Lock)
            {
                Templates.Clear();
                var modules = Directory.GetFiles($"{AppPath}/Modules/{FolderPrefix}/", "*",
                    SearchOption.AllDirectories);
                var mods = ModificationsLoader.Get($"/{FolderPrefix}/");
                var templates = Load<U>(modules.Concat(mods).ToArray());
                foreach (var template in templates) Templates[ParseKey(template.Name)] = template;
            }
        }

        protected abstract S ParseKey(string Name);

        public static void LoadModules(string AppPath)
        {
            Instance.DoLoadModules(AppPath);
        }

        private static T[] Load<T>(string[] Modules)
        {
            var list = new List<T>();
            foreach (var module in Modules)
            {
                var ext = Path.GetExtension(module);
                if (ext != ".json") continue;
                var obj = FromJSON<T>(module, File.ReadAllText(module), out var result);

                if (!result) continue;

                list.Add(obj);
            }

            return list.ToArray();
        }

        private static T FromJSON<T>(string Path, string Data, out bool Success)
        {
            try
            {
                var factory = JsonConvert.DeserializeObject<T>(Data);
                Success = true;
                return factory;
            }
            catch (Exception e)
            {
                Success = false;
                Log.WriteLine($"Exception while reading file '{Path}':{Environment.NewLine}{e}");
            }

            return default;
        }
    }
}