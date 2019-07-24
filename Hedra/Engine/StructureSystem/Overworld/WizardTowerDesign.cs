using Hedra.Engine.CacheSystem;
using Hedra.Engine.Localization;
using Hedra.Localization;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WizardTowerDesign : SimpleCompletableStructureDesign<WizardTower>
    {
        public override int PlateauRadius => 256;
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.WizardTowerIcon);
        protected override int StructureChance => StructureGrid.WizardTower;
        protected override CacheItem? Cache { get; }
        
        protected override WizardTower Create(Vector3 Position, float Size)
        {
            throw new System.NotImplementedException();
        }
        
        public override string DisplayName => Translations.Get("structure_wizard_tower");

        protected override string GetDescription(WizardTower Structure)
        {
            throw new System.NotImplementedException();
        }

        protected override string GetShortDescription(WizardTower Structure)
        {
            throw new System.NotImplementedException();
        }
    }
}