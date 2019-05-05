using Hedra.Engine.CacheSystem;
using Hedra.Engine.Localization;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WitchHutDesign : SimpleCompletableStructureDesign<WitchHut>
    {
        public override int PlateauRadius => 128;
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.WitchHutIcon);
        public override VertexData QuestIcon => Icon;
        protected override int StructureChance => StructureGrid.WitchHut;
        protected override CacheItem? Cache => CacheItem.WitchHut;
        public override string DisplayName => Translations.Get("structure_witch_hut");
        
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
    }
}