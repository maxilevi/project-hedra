using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.AISystem.Humanoid
{
    public class FishermanAIComponent : FarmerAIComponent
    {
        public FishermanAIComponent(IHumanoid Parent, Vector2 FarmPosition, Vector2 FarmSize) : base(Parent, FarmPosition, FarmSize)
        {
        }

        public override void Update()
        {
            base.Update();
            var underBlock = World.GetBlockAt(Parent.Position - Vector3.UnitY * Engine.Generation.ChunkSystem.Chunk.BlockSize);
            if (underBlock.Type != BlockType.Water && underBlock.Type != BlockType.Air)
            {
                IsSitting = true;
                Parent.Boat.Disable();
            }  
            else
            {
                if (!Parent.Boat.Enabled && !Parent.IsKnocked)
                    Parent.Boat.Enable();
                Parent.Boat.Update();
            }
        }

        protected override bool IsSuitableSpot(Vector3 Point)
        {
            return Physics.IsWaterBlock(Point);
        }
    }
}