using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Scenes;
using Hedra.Engine.WorldBuilding;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Dungeon0Design : SimpleCompletableStructureDesign<Dungeon>
    {
        public override int PlateauRadius => 384;
        public override string DisplayName => Translations.Get("structure_dungeon");
        public override VertexData Icon => CacheManager.GetModel(CacheItem.Dungeon0Icon);
        protected override int StructureChance => StructureGrid.Dungeon0Chance;
        protected override CacheItem? Cache => CacheItem.Dungeon0;

        protected override Vector3 StructureOffset => Dungeon0Cache.Offset;
        protected override BlockType PathType => BlockType.StonePath;
        protected override float GroundworkRadius => 180;
        
        protected override Dungeon Create(Vector3 Position, float Size)
        {
            return new Dungeon(Position);
        }

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            SceneLoader.LoadIfExists(Structure, "Assets/Env/Structures/Dungeon/Dungeon0.ply", Vector3.One, Rotation * Translation, Settings);
            
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door0.ply", Vector3.One), Dungeon0Cache.Doors[0], Rotation, Structure, true, false);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door1.ply", Vector3.One), Dungeon0Cache.Doors[1], Rotation, Structure, false, false);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door2.ply", Vector3.One), Dungeon0Cache.Doors[2], Rotation, Structure, true, true);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door3.ply", Vector3.One), Dungeon0Cache.Doors[3], Rotation, Structure, true, true);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door4.ply", Vector3.One), Dungeon0Cache.Doors[4], Rotation, Structure, true, true);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door5.ply", Vector3.One), Dungeon0Cache.Doors[5], Rotation, Structure, true, false);
            AddDoor(AssetManager.PLYLoader($"Assets/Env/Structures/Dungeon/Dungeon0-Door6.ply", Vector3.One), Dungeon0Cache.Doors[6], Rotation, Structure, false, true);
        }

        protected override string GetDescription(Dungeon Structure) => throw new System.NotImplementedException();

        protected override string GetShortDescription(Dungeon Structure) => throw new System.NotImplementedException();

        private static BaseStructure BuildTrigger0(Vector3 Point, VertexData Mesh)
        {
            return new Dungeon0Trigger(Point, Mesh);
        }
        
        private static SceneSettings Settings { get; } = new SceneSettings
        {
            LightRadius = PointLight.DefaultRadius * 1.5f,
            IsNightLight = false,
            Structure1Creator = BuildTrigger0,
            Structure2Creator = (P, V) => new Lever(P, SceneLoader.GetRadius(V))
        };
    }
}