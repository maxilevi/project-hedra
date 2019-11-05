using System;
using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Scenes;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class DungeonDesign : SimpleCompletableStructureDesign<Dungeon>
    {
        public override int PlateauRadius => 256;
        public override string DisplayName => Translations.Get("structure_dungeon");
        public override VertexData Icon => CacheManager.GetModel(CacheItem.Dungeon0Icon);
        protected override int StructureChance => StructureGrid.Dungeon0Chance;
        protected override CacheItem? Cache => CacheItem.Dungeon0;
        //protected override BlockType PathType => BlockType.StonePath;
        //protected override float GroundworkRadius => 180;
        
        protected override Dungeon Create(Vector3 Position, float Size)
        {
            return new Dungeon(Position);
        }

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            SceneLoader.LoadIfExists(Structure, "Assets/Env/Structures/Dungeon/Dungeon0.ply", Vector3.One, Rotation * Translation, new SceneSettings
            {
                LightRadius = 32,
                IsNightLight = false
            });
        }

        protected override string GetDescription(Dungeon Structure) => throw new System.NotImplementedException();

        protected override string GetShortDescription(Dungeon Structure) => throw new System.NotImplementedException();
    }
}