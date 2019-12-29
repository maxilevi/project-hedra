using System;
using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class CottageWithVegetablePlotDesign : CottageDesign<CottageWithVegetablePlot>
    {
        protected override CottageWithVegetablePlot Create(Vector3 Position, float Size)
        {
            return new CottageWithVegetablePlot(Position, Size);
        }
        
        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            var farmOffset = Vector3.UnitX * -5f + Vector3.UnitZ * -15f;
            var rotation = Matrix4x4.CreateRotationY(-90 * Mathf.Radian);
            var model = DynamicCache.Get("Assets/Env/Structures/VegetablePlot/VegetablePlot0.ply", WitchHutCache.Scale * StructureScale);
            var shapes = DynamicCache.GetShapes("Assets/Env/Structures/VegetablePlot/VegetablePlot0.ply", WitchHutCache.Scale * StructureScale);
            
            model.Transform(rotation);
            model.Transform(Translation);
            model.Translate(StructureOffset + farmOffset);

            for (var i = 0; i < shapes.Count; ++i)
            {
                shapes[i].Transform(rotation);
                shapes[i].Transform(Translation);
                shapes[i].Transform(StructureOffset + farmOffset);
            }
            
            Structure.AddStaticElement(model);
            Structure.AddCollisionShape(shapes.ToArray());
            
            WitchHutDesign.PlacePlants(Structure, Translation, rotation, StructureOffset + farmOffset, Rng);
            
            var region = World.BiomePool.GetRegion(Structure.Position);
            var root = VillageLoader.Designer[region.Structures.VillageType];
            AddHouse(Structure, Vector3.UnitZ * 15f, root, Rng);
        }

        protected override BlockType PathType => BlockType.StonePath;
        protected override Vector3 NPCOffset => Vector3.UnitZ * -20f - Vector3.UnitX * 15f;
        public override int PlateauRadius => 128;

        protected override float GroundworkRadius => 32;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.CauldronIcon);
    }
}