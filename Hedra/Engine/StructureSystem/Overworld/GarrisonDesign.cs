using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class GarrisonDesign : SimpleStructureDesign<Garrison>
    {
        public override int PlateauRadius => 384;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.GarrisonIcon);
        public override bool CanSpawnInside => false;
        protected override int StructureChance => StructureGrid.GarrisonChance;
        protected override CacheItem? Cache => CacheItem.Garrison;

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation,
            Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            /* Office */
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Garrison/Garrison0-Door0.ply", Vector3.One),
                GarrisonCache.Doors[0], Rotation, Structure, true, true);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Garrison/Garrison0-Door1.ply", Vector3.One),
                GarrisonCache.Doors[1], Rotation, Structure, true, true);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Garrison/Garrison0-Door2.ply", Vector3.One),
                GarrisonCache.Doors[2], Rotation, Structure, true, true);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Garrison/Garrison0-Door3.ply", Vector3.One),
                GarrisonCache.Doors[3], Rotation, Structure, true, true);
        }

        protected override Garrison Create(Vector3 Position, float Size)
        {
            return new Garrison(Position);
        }
    }
}