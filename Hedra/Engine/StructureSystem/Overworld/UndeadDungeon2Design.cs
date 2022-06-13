using System;
using System.Numerics;
using Hedra.API;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class UndeadDungeon2Design : UndeadDungeonWithBossDesign
    {
        public override int PlateauRadius => 480;
        public override int StructureChance => StructureGrid.Dungeon2Chance;
        protected override CacheItem? Cache => CacheItem.Dungeon2;
        protected override Vector3 StructureOffset => Dungeon2Cache.Offset;
        protected override Vector3 StructureScale => Dungeon2Cache.Scale;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.Dungeon2Icon);
        protected override float GroundworkRadius => 216;
        protected override string BaseFileName => "Dungeon2";
        protected override int Level => 13;

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation,
            Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);

            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon2-Door0.ply", Vector3.One),
                Dungeon2Cache.Doors[0], Rotation, Structure, false, false);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon2-Door2.ply", Vector3.One),
                Dungeon2Cache.Doors[2], Rotation, Structure, true, false);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon2-Door3.ply", Vector3.One),
                Dungeon2Cache.Doors[3], Rotation, Structure, true, false);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon2-Door4.ply", Vector3.One),
                Dungeon2Cache.Doors[4], Rotation, Structure, true, false);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon2-Door5.ply", Vector3.One),
                Dungeon2Cache.Doors[5], Rotation, Structure, false, false);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon2-Door6.ply", Vector3.One),
                Dungeon2Cache.Doors[6], Rotation, Structure, true, false);

            var bossDoor =
                AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon2-Door1.ply", Vector3.One),
                    Dungeon2Cache.Doors[1], Rotation, Structure, true, false);
            bossDoor.IsLocked = true;

            var bossLever = AddLever(Structure, Dungeon2Cache.Lever0, Rotation);
            bossLever.OnActivate += _ => { bossDoor.InvokeInteraction(_); };
        }

        protected override IEntity CreateDungeonBoss(Vector3 Position, CollidableStructure Structure)
        {
            var rng = BuildRng(Structure);
            var level = ((UndeadDungeon2Design)Structure.Design).Level;
            return BossGenerator.CreateBeasthunterBoss(Position, level, rng);
        }
    }
}