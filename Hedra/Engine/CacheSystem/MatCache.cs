using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class MatCache : CacheType
    {
        public MatCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Mat0.ply", Vector3.One));

            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Mat0.ply", 2, Vector3.One));
        }
    }
}
