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
    public class UndeadDungeon1Design : UndeadDungeonWithBossDesign
    {
        public override int PlateauRadius => 480;
        public override int StructureChance => StructureGrid.Dungeon1Chance;
        protected override CacheItem? Cache => CacheItem.Dungeon1;
        protected override Vector3 StructureOffset => Dungeon1Cache.Offset;
        protected override Vector3 StructureScale => Dungeon1Cache.Scale;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.Dungeon1Icon);
        protected override float GroundworkRadius => 216;
        protected override string BaseFileName => "Dungeon1";
        protected override int Level => 17;

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation,
            Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon1-Door0.ply", Vector3.One),
                Dungeon1Cache.Doors[0], Rotation, Structure, true, false);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon1-Door1.ply", Vector3.One),
                Dungeon1Cache.Doors[1], Rotation, Structure, false, false);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon1-Door2.ply", Vector3.One),
                Dungeon1Cache.Doors[2], Rotation, Structure, false, true);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon1-Door4.ply", Vector3.One),
                Dungeon1Cache.Doors[4], Rotation, Structure, false, true);

            var bossDoor0 =
                AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon1-Door5.ply", Vector3.One),
                    Dungeon1Cache.Doors[5], Rotation, Structure, true, false);
            var bossDoor1 =
                AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon1-Door6.ply", Vector3.One),
                    Dungeon1Cache.Doors[6], Rotation, Structure, false, true);
            bossDoor0.IsLocked = true;
            bossDoor1.IsLocked = true;

            var bossLever = AddLever(Structure, Dungeon1Cache.Lever1, Rotation);
            bossLever.OnActivate += _ =>
            {
                bossDoor0.InvokeInteraction(_);
                bossDoor1.InvokeInteraction(_);
            };

            var leverDoor =
                AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon1-Door3.ply", Vector3.One),
                    Dungeon1Cache.Doors[3], Rotation, Structure, true, false);
            leverDoor.IsLocked = true;

            var lever0 = AddLever(Structure, Dungeon1Cache.Lever0, Rotation);
            lever0.OnActivate += _ => { leverDoor.InvokeInteraction(_); };
        }

        protected override IEntity CreateDungeonBoss(Vector3 Position, CollidableStructure Structure)
        {
            var level = ((UndeadDungeon1Design)Structure.Design).Level;
            var rng = BuildRng(Structure);
            return BossGenerator.CreateBeasthunterBoss(Position, level, rng);
        }
    }
}