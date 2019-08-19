using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK;
using System.Linq;
using Hedra.Engine.PhysicsSystem;
using Hedra.Rendering;

namespace Hedra.Engine.CacheSystem
{
    public class RockCache : CacheType
    {
        public RockCache()
        {
            /*
             * WARNING: if you add more models here, equality operators on Chunk.cs won't work,
             *  fix that before adding new models
             */

            this.AddModel(AssetManager.PLYLoader("Assets/Env/Rock1.ply", Vector3.One));
            var shape = AssetManager.PLYLoader("Assets/Env/Colliders/Rock0_Collider0.ply", Vector3.One, Vector3.Zero, Vector3.Zero, false);
            
            var list = new List<CollisionShape>
            {
                new CollisionShape(shape.Vertices, shape.Indices)
            };

            this.AddShapes(list);
        }
        
        public override CacheItem Type => CacheItem.Rock;
    }
}