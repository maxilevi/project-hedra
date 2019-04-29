using Hedra.Engine.CacheSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WizardTowerDesign : SimpleStructureDesign<WizardTower>
    {
        public override int PlateauRadius => 256;
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.WizardTowerIcon);
        protected override int StructureChance => StructureGrid.WizardTower;
        protected override CacheItem? Cache { get; }
        
        protected override WizardTower Create(Vector3 Position, float Size)
        {
            throw new System.NotImplementedException();
        }
    }
}