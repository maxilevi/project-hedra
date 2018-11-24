using System;
using System.Runtime.InteropServices;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class VillageDesign : StructureDesign
    {
        public const float Spacing = 160;
        public override int Radius { get; set; } = 2048;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.VillageIcon);

        public override void Build(CollidableStructure Structure)
        {
            var builder = Structure.Parameters.Get<VillageAssembler>("Builder");
            var design = Structure.Parameters.Get<PlacementDesign>("Design");
            builder.Build(design, Structure);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng, new Village(TargetPosition));
            structure.Mountain.Radius = (Rng.NextFloat() * .5f + .5f) * Radius;
            var region = World.BiomePool.GetRegion(TargetPosition);
            var builder = new VillageAssembler(structure, VillageLoader.Designer[region.Structures.VillageType], Rng);
            var design = builder.DesignVillage();
            design.Translate(TargetPosition);
            builder.PlaceGroundwork(design);

            structure.Parameters.Set("Builder", builder);
            structure.Parameters.Set("Design", design);
            return structure;
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            var height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);
            return BiomeGenerator.PathFormula(ChunkOffset.X, ChunkOffset.Y) > 0 && Rng.Next(0, 25) == 1 && height > BiomePool.SeaLevel;
        }

        private string CreateName(int Seed)
        {
            return NameGenerator.Generate(Seed);
        }

        public override void OnEnter(IPlayer Player)
        {
            Player.MessageDispatcher.ShowTitleMessage($"WELCOME TO {NameGenerator.Generate(World.Seed)}", 6f);
        }
        
        public override int[] AmbientSongs => new []
        {
            SoundtrackManager.VillageIndex
        };
    }
}
