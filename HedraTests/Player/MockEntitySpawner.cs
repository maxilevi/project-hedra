using Hedra.Engine.EntitySystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.Player;
using OpenToolkit.Mathematics;

namespace HedraTests.Player
{
    public class MockEntitySpawner : EntitySpawner
    {
        public SpawnTemplate TargetTemplate { get; set; }
        
        public MockEntitySpawner(IPlayer Player) : base(Player)
        {
        }

        protected override SpawnTemplate SelectMobTemplate(Vector3 NewPosition)
        {
            return TargetTemplate;
        }
    }
}