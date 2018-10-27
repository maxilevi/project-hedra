using Hedra.Engine.EntitySystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Player;
using OpenTK;

namespace HedraTests.Player
{
    public class MockEntitySpawner : EntitySpawner
    {
        public SpawnTemplate TargetTemplate { get; set; }
        
        public MockEntitySpawner(IPlayer Player) : base(Player)
        {
        }

        public override SpawnTemplate SelectMobTemplate(Vector3 NewPosition)
        {
            return TargetTemplate;
        }
    }
}