using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class WizardTowerCache : CacheType
    {
        public WizardTowerCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/WizardTower/WizardTower0.ply", Vector3.One));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/WizardTower/WizardTower0.ply",
                Vector3.One));
        }

        public override CacheItem Type => CacheItem.WizardTower;

        public static Vector3 Door0 => new Vector3(-4.29917f, 36.76129f, -41.54929f);
    }
}