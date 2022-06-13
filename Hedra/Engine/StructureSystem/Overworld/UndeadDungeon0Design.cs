using System;
using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class UndeadDungeon0Design : UndeadDungeonWithBossDesign
    {
        public override int PlateauRadius => 420;
        public override int StructureChance => StructureGrid.Dungeon0Chance;
        protected override CacheItem? Cache => CacheItem.Dungeon0;
        protected override Vector3 StructureOffset => Dungeon0Cache.Offset;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.Dungeon0Icon);
        protected override float GroundworkRadius => 180;
        protected override string BaseFileName => "Dungeon0";
        protected override int Level => 13;

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation,
            Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            /* Office */
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon0-Door0.ply", Vector3.One),
                Dungeon0Cache.Doors[0], Rotation, Structure, true, false);
            /* Entrance */
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon0-Door1.ply", Vector3.One),
                Dungeon0Cache.Doors[1], Rotation, Structure, false, false);
            /* Lever room pathway */
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon0-Door2.ply", Vector3.One),
                Dungeon0Cache.Doors[2], Rotation, Structure, true, true);
            /* Lever room */
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon0-Door3.ply", Vector3.One),
                Dungeon0Cache.Doors[3], Rotation, Structure, true, true);
            /* Boss room pathway */
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon0-Door4.ply", Vector3.One),
                Dungeon0Cache.Doors[4], Rotation, Structure, true, true);
            /* Boss room doors */
            var bossDoor0 =
                AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon0-Door5.ply", Vector3.One),
                    Dungeon0Cache.Doors[5], Rotation, Structure, true, false);
            var bossDoor1 =
                AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon0-Door6.ply", Vector3.One),
                    Dungeon0Cache.Doors[6], Rotation, Structure, false, true);

            bossDoor0.IsLocked = true;
            bossDoor1.IsLocked = true;
            var lever = AddLever(Structure, Dungeon0Cache.Lever0, Rotation);
            lever.OnActivate += _ =>
            {
                bossDoor0.InvokeInteraction(_);
                bossDoor1.InvokeInteraction(_);
            };
        }

        protected override IEntity CreateDungeonBoss(Vector3 Position, CollidableStructure Structure)
        {
            var rng = BuildRng(Structure);
            var level = ((UndeadDungeon0Design)Structure.Design).Level;
            return BossGenerator.CreateSkeletonKingBoss(Position, level, rng);
        }
    }
}