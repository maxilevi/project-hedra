using System;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WitchHutDesign : SimpleCompletableStructureDesign<WitchHut>
    {
        public override int PlateauRadius => 160;
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.WitchHutIcon);
        public override VertexData QuestIcon => Icon;
        protected override int StructureChance => StructureGrid.WitchHut;
        protected override CacheItem? Cache => CacheItem.WitchHut;
        protected override BlockType PathType => BlockType.Grass;
        protected override bool NoPlantsZone => true;
        public override string DisplayName => Translations.Get("structure_witch_hut");

        protected override void DoBuild(CollidableStructure Structure, Matrix4 Rotation, Matrix4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            AddDoor(WitchHutCache.Hut0Door0, WitchHutCache.Hut0Door0Position, Rotation, Structure, WitchHutCache.Hut0Door0InvertedRotation, WitchHutCache.Hut0Door0InvertedPivot);
            AddDoor(WitchHutCache.Hut0Door1, WitchHutCache.Hut0Door1Position, Rotation, Structure, WitchHutCache.Hut0Door1InvertedRotation, WitchHutCache.Hut0Door1InvertedPivot);
        }

        protected override WitchHut Create(Vector3 Position, float Size)
        {
            return new WitchHut(Position);
        }

        protected override string GetDescription(WitchHut Structure)
        {
            return "";//Translations.Get();
        }

        protected override string GetShortDescription(WitchHut Structure)
        {
            return "";//Translations.Get();
        }

        protected override float BuildRotationAngle(Random Rng)
        {
            return Rng.Next(0, 4) * 90;
        }
    }
}