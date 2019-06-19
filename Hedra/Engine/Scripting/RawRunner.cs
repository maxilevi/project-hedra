using System.IO;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Microsoft.Scripting.Hosting;

namespace Hedra.Engine.Scripting
{
    public class RawRunner : Runner
    {
        public RawRunner(ScriptEngine Engine) : base(Engine)
        {
        }
        
        public override void Load() {}

        protected override ScriptScope DoRun(string Library)
        {
            var scope = Engine.CreateScope();
            scope.SetVariable("player", GameManager.PlayerExists ? GameManager.Player : null);
            Engine.Execute(Get(Library), scope);
            return scope;
        }

        private static string Get(string Name)
        {
            if(GameSettings.DebugMode)
                return File.ReadAllText($"../../Scripts/{Name}");
            return File.ReadAllText($"{AssetManager.AppPath}/Scripts/{Name}");
        }
    }
}