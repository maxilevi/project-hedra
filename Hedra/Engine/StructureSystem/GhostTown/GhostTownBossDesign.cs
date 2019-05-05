using System;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.GhostTown
{
    public class GhostTownBossDesign : SimpleStructureDesign<GhostTownBoss>
    {
        public override int PlateauRadius => 256;
        public override VertexData Icon => null;
        protected override int StructureChance => throw new NotImplementedException();
        protected override CacheItem? Cache => null;
        public static bool Spawned { get; set; }
        
        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            Spawned = true;
            return base.Setup(TargetPosition, Rng);
        }

        protected override void DoBuild(CollidableStructure Structure, Matrix4 Rotation, Matrix4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            DoWhenChunkReady(Vector3.TransformPosition(Vector3.Zero, Translation), P =>
            {
                var offset = Vector3.UnitZ * 24;
                var boss = BossGenerator.Generate(new []{MobType.Lich}, P + offset, Utils.Rng);
                boss.AddComponent(new DropComponent(boss)
                {
                    DropChance = 100,
                    ItemDrop = ItemPool.Grab(new ItemPoolSettings(ItemTier.Unique))
                });
                ((GhostTownBoss)Structure.WorldObject).Boss = boss;
            }, Structure);
        }

        public override bool ShouldSetup(Vector2 ChunkOffset, Vector3 TargetPosition, CollidableStructure[] Items, Region Biome, IRandom Rng)
        {
            return ChunkOffset == World.ToChunkSpace(World.SpawnPoint) && !Spawned;
        }

        protected override GhostTownBoss Create(Vector3 Position, float Size)
        {
            return new GhostTownBoss(Position);
        }      
    }
}