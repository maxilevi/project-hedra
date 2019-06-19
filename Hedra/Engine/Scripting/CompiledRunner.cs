using System.Collections.Generic;
using System.IO;
using Hedra.Engine.Management;
using Microsoft.Scripting.Hosting;

namespace Hedra.Engine.Scripting
{
    public class CompiledRunner : Runner
    {
        private ScriptScope _main;
        public CompiledRunner(ScriptEngine Engine) : base(Engine)
        {
        }

        public override void Load()
        {
            var files = Directory.GetFiles($"{AssetManager.AppPath}/Scripts/");
            for (var i = 0; i < files.Length; ++i)
            {
                var source = Engine.CreateScriptSourceFromString(File.ReadAllText(files[i]));
                source.Execute(_main);
            }
        }

        protected override ScriptScope DoRun(string Library)
        {
            throw new System.NotImplementedException();
        }
    }
}