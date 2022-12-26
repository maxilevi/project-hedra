using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.GhostTown;
using Hedra.Engine.WorldBuilding;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Structures;

public class AzathulBossDesign : SimpleStructureDesign<AzathulBoss>, ICompletableStructureDesign,
        IFindableStructureDesign
    {
        public override int StructureChance => throw new NotImplementedException();
        protected override CacheItem? Cache => null;
        public static bool Spawned { get; set; }
        public override bool CanSpawnInside => false;
        public static Vector3 Position => World.SpawnPoint;
        public override VertexData Icon => null;
        public override bool IsFixed => true;

        public string GetShortDescription(IStructure Structure)
        {
            return Translations.Get("quest_defeat_azathul_short");
        }

        public string GetDescription(IStructure Structure)
        {
            return Translations.Get("quest_defeat_azathul_description");
        }

        public override int PlateauRadius => 256;
        public string DisplayName => Translations.Get("structure_azathul_boss");

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            Spawned = true;
            return base.Setup(TargetPosition, Rng);
        }

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation,
            Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            DoWhenChunkReady(Vector3.Transform(Vector3.Zero, Translation), P =>
            {
                var offset = Vector3.UnitZ * 24;
                var boss = BossGenerator.Generate(new[] { MobType.Azathul }, P + offset, Utils.Rng);
                boss.AddComponent(new DropComponent(boss)
                {
                    DropChance = 100,
                    ItemDrop = ItemPool.Grab(new ItemPoolSettings(ItemTier.Unique)
                    {
                        RandomizeTier = false
                    })
                });
                ((GhostTownBoss)Structure.WorldObject).Boss = boss;
            }, Structure);
        }

        public override bool ShouldSetup(Vector2 ChunkOffset, ref Vector3 TargetPosition, CollidableStructure[] Items,
            Region Biome, IRandom Rng)
        {
            return ChunkOffset == World.ToChunkSpace(World.SpawnPoint) && !Spawned;
        }

        protected override AzathulBoss Create(Vector3 Position, float Size)
        {
            return new AzathulBoss(Position);
        }
    }