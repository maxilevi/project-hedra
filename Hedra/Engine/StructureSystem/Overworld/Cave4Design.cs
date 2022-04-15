using System;
using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;

namespace Hedra.Engine.StructureSystem.Overworld;

public class Cave4Design : CaveWithBossDesign
{
    public override int StructureChance => StructureGrid.Cave4Chance;
    protected override CacheItem? Cache => CacheItem.Cave4;
    protected override Vector3 StructureOffset => Cave4Cache.Offset;
    protected override string BaseFileName => "Cave4";
    protected override int Level => 32;
    protected override bool HasAmbientHandler => true;

    protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation,
            Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave4-Door0.ply", Vector3.One), Rotation, Structure, true, true);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave4-Door1.ply", Vector3.One), Rotation, Structure, true, false);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave4-Door2.ply", Vector3.One), Rotation, Structure, true, true);
            
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave4-Door3.ply", Vector3.One), Rotation, Structure, true, true);
            
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave4-Door4.ply", Vector3.One), Rotation, Structure, false, false);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave4-Door5.ply", Vector3.One), Rotation, Structure, false, true);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave4-Door6.ply", Vector3.One), Rotation, Structure, true, false);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave4-Door7.ply", Vector3.One), Rotation, Structure, false, true);
            /* Front doors */
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave4-Door8.ply", Vector3.One), Rotation, Structure, false, false);
            AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave4-Door9.ply", Vector3.One), Rotation, Structure, true, true);
            /*Boss room doors */ 
            var bossDoor0 = AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave4-Door10.ply", Vector3.One), Rotation, Structure, false, false);
            var bossDoor1 = AddDoor(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave4-Door11.ply", Vector3.One), Rotation, Structure, true, true);
            
            bossDoor0.IsLocked = true;
            bossDoor1.IsLocked = true;
            var lever = AddLever(Structure, Cave4Cache.Lever0, Rotation);
            lever.OnActivate += _ =>
            {
                bossDoor0.InvokeInteraction(_);
                bossDoor1.InvokeInteraction(_);
            };
        }
}