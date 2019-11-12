using System;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.WorldBuilding;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Sound;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class VillageDesign : StructureDesign
    {
        public const int MaxVillageSize = 18;
        public const int PlateauVillageRatio = 65;
        public const int MaxVillageRadius = MaxVillageSize * PlateauVillageRatio;
        public const int PathWidth = 16;
        public const float Spacing = 160;

        public override int PlateauRadius { get; } = MaxVillageRadius;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.VillageIcon);
        public override bool CanSpawnInside => true;

        public override void Build(CollidableStructure Structure)
        {
            var builder = Structure.Parameters.Get<VillageAssembler>("Builder");
            var design = Structure.Parameters.Get<PlacementDesign>("Design");
            builder.Build(design, Structure);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng, BuildVillageObject(TargetPosition));
            var region = World.BiomePool.GetRegion(TargetPosition);
            var builder = new VillageAssembler(structure, VillageLoader.Designer[region.Structures.VillageType], Rng);
            structure.Mountain.Radius = builder.Size * PlateauVillageRatio;
            structure.Mountain.Hardness = 1.5f;
            var design = builder.DesignVillage();
            design.Translate(TargetPosition);
            builder.PlaceGroundwork(design);

            structure.Parameters.Set("Builder", builder);
            structure.Parameters.Set("Design", design);
            return structure;
        }

        protected virtual Village BuildVillageObject(Vector3 TargetPosition)
        {
            return new Village(TargetPosition);
        }

        protected override bool SetupRequirements(ref Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            return /*BiomeGenerator.PathFormula(ChunkOffset.X, ChunkOffset.Y) > 0
                   && */Rng.Next(0, StructureGrid.VillageChance) == 1
                   && Biome.Generation.GetMaxHeight(TargetPosition.X, TargetPosition.Z) > BiomePool.SeaLevel
                   && (TargetPosition - World.SpawnPoint).LengthFast() > (World.SpawnVillagePoint - World.SpawnPoint).LengthFast() *  2;
        }

        public override void OnEnter(IPlayer Player)
        {
            Player.MessageDispatcher.ShowTitleMessage(Translations.Get("welcome_to_village", NameGenerator.Generate(World.Seed + 23123)), 6f);
        }
        
        public override int[] AmbientSongs => new []
        {
            SoundtrackManager.VillageAmbient
        };
    }
}
