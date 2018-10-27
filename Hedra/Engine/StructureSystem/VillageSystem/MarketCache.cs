
using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class MarketCache
    {
        public readonly Dictionary<int, List<CollisionShape>> ShelfShapes_Clones;
        public readonly Dictionary<int, VertexData> ShelfModels_Clones;
        public readonly VertexData Market0_Clone;
        public readonly VertexData Market1_Clone;
        public readonly List<CollisionShape> MarketShapes_Clone;
        public readonly List<CollisionShape> ExtraShelf_Clone;

        public MarketCache()
        {
            ShelfModels_Clones = new Dictionary<int, VertexData>();
            ShelfShapes_Clones = new Dictionary<int, List<CollisionShape>>();

            Market0_Clone = AssetManager.PLYLoader("Assets/Env/Village/MarketStand0.ply", Vector3.One);
            Market1_Clone = AssetManager.PLYLoader("Assets/Env/Village/MarketStand1.ply", Vector3.One);

            MarketShapes_Clone = AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand0.ply", 6, Vector3.One);
            ExtraShelf_Clone = AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand1.ply", 1, Vector3.One);

            ShelfShapes_Clones.Add(2, AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand2.ply", 1, Vector3.One));
            ShelfShapes_Clones.Add(3, AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand3.ply", 1, Vector3.One));
            ShelfShapes_Clones.Add(4, AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand4.ply", 1, Vector3.One));
            ShelfShapes_Clones.Add(5, AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand5.ply", 1, Vector3.One));
            ShelfShapes_Clones.Add(6, AssetManager.LoadCollisionShapes("Assets/Env/Village/MarketStand6.ply", 1, Vector3.One));

            ShelfModels_Clones.Add(2, AssetManager.PLYLoader("Assets/Env/Village/MarketStand2.ply", Vector3.One));
            ShelfModels_Clones.Add(3, AssetManager.PLYLoader("Assets/Env/Village/MarketStand3.ply", Vector3.One));
            ShelfModels_Clones.Add(4, AssetManager.PLYLoader("Assets/Env/Village/MarketStand4.ply", Vector3.One));
            ShelfModels_Clones.Add(5, AssetManager.PLYLoader("Assets/Env/Village/MarketStand5.ply", Vector3.One));
            ShelfModels_Clones.Add(6, AssetManager.PLYLoader("Assets/Env/Village/MarketStand6.ply", Vector3.One));
        }
    }
}
