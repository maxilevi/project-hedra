using System.Numerics;
using Hedra.AISystem;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Dungeon0MobAITrigger : CollisionTrigger
    {
        public Dungeon0MobAITrigger(Vector3 Position, VertexData Mesh) : base(Position, Mesh)
        {
            OnCollision += Entity =>
            {
                var dualAI = Entity.SearchComponent<DualAIComponent>();
                dualAI?.Switch();
            };
        }
    }
}