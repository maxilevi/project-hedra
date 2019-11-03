using Hedra.Engine.EntitySystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.Player;
using System.Numerics;

namespace HedraTests.Player
{
    public class MockMobSpawner : MobSpawner
    {
        public SpawnTemplate TargetTemplate { get; set; }
        
        public MockMobSpawner(IPlayer Player) : base(Player)
        {
        }

        protected override SpawnTemplate SelectMobTemplate(Vector3 NewPosition)
        {
            return TargetTemplate;
        }
    }
}