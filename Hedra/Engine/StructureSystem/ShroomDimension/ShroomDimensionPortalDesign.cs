using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Structures;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class ShroomDimensionPortalDesign : SimpleFindableStructureDesign<ShroomDimensionPortal>,
        ICompletableStructureDesign
    {
        public override int PlateauRadius => 128;
        public override bool CanSpawnInside => true;
        protected virtual bool SpawnNpc => true;
        public override int StructureChance => StructureGrid.ShroomPortalChance;
        protected override CacheItem? Cache => CacheItem.ShroomPortal;
        protected override Vector3 StructureScale => Vector3.One * 2.5f;
        protected override BlockType PathType => BlockType.Grass;

        public override string DisplayName => Translations.Get("structure_portal");
        public override VertexData Icon => CacheManager.GetModel(CacheItem.ShroomPortalIcon);

        public string GetShortDescription(IStructure Structure)
        {
            return Translations.Get("quest_complete_portal_short");
        }

        public string GetDescription(IStructure Structure)
        {
            return Translations.Get("quest_complete_portal_description");
        }

        protected override void ApplyColors(VertexData Model, RegionColor Colors)
        {
            base.ApplyColors(Model, Colors);
            Model.Color(AssetManager.ColorCode0, Colors.GrassColor);
            Model.Color(AssetManager.ColorCode1, Colors.StoneColor * 1.25f);
            Model.Color(AssetManager.ColorCode2, Colors.StoneColor * 1f);
            Model.Color(AssetManager.ColorCode3, Colors.WoodColor * .75f);
        }

        protected override ShroomDimensionPortal Create(Vector3 Position, float Size)
        {
            return new ShroomDimensionPortal(Position + Vector3.UnitY * 8f, StructureScale * 10f,
                RealmHandler.ShroomDimension, SpawnShroomDimensionPortalDesign.Position);
        }
    }
}