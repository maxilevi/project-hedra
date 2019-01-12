using System;
using System.Linq;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class WellDesign : StructureDesign
    {
        public override int Radius => 80;
        public override VertexData Icon => null;
        
        public override void Build(CollidableStructure Structure)
        {
            var originalModel = CacheManager.GetModel(CacheItem.Well);
            var rng = BuildRng(Structure);
            var transformation = Matrix4.CreateRotationY(Mathf.Radian * rng.NextFloat() * 360.0f) * 
                                 Matrix4.CreateTranslation(Structure.Position);

            if (rng.Next(0, 3) == 1)
            {
                var npc = World.WorldBuilding.SpawnVillager(
                    Vector3.TransformPosition(Vector3.UnitZ * 8f, transformation),
                    rng
                );
                npc.Physics.UsePhysics = false;
                npc.IsSitting = true;
                //npc.AddComponent(new QuestGiverComponent(npc, QuestPool.Grab().Build(npc.Position, Utils.Rng, npc)));
                ((Well) Structure.WorldObject).NPC = npc;
            }
            
            Structure.AddStaticElement(
                originalModel.Clone().Transform(transformation)
            );
            Structure.AddCollisionShape(
                CacheManager.GetShape(originalModel).DeepClone().Select(S => S.Transform(transformation)).ToArray()
            );           
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng, new Well(TargetPosition, 48));
            structure.AddGroundwork(new RoundedGroundwork(TargetPosition, 24, BlockType.StonePath));
            structure.Mountain.Radius = 48;
            return structure;
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            var height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);
            return Rng.Next(0, 80) == 1 && height > BiomePool.SeaLevel;
        }
    }
}