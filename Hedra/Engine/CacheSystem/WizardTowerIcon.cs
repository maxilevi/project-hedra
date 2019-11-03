using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class WizardTowerIcon : CacheType
    {
        public override CacheItem Type => CacheItem.WizardTowerIcon;

        public WizardTowerIcon()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/WizardTower/WizardTower0-Icon.ply", Vector3.One));
        }
    }
}