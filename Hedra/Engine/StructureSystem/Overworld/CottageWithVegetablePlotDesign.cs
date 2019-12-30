using System;
using System.Linq;
using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Scenes;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.WorldBuilding;
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
            var model = DynamicCache.Get("Assets/Env/Structures/VegetablePlot/VegetablePlot0.ply", Scale);
            var shapes = DynamicCache.GetShapes("Assets/Env/Structures/VegetablePlot/VegetablePlot0.ply", Scale);
            var farmTranslation = Translation.ExtractTranslation() + StructureOffset + farmOffset;
            
            model.Transform(rotation);
            model.Translate(farmTranslation);

            for (var i = 0; i < shapes.Count; ++i)
            {
                shapes[i].Transform(rotation);
                shapes[i].Transform(farmTranslation);
            }
            
            Structure.AddStaticElement(model);
            Structure.AddCollisionShape(shapes.ToArray());
            
            WitchHutDesign.PlacePlants(Structure, Translation, rotation, StructureOffset + farmOffset, Rng);
            
            var region = World.BiomePool.GetRegion(Structure.Position);
            var root = VillageLoader.Designer[region.Structures.VillageType];
            AddHouse(Structure, Vector3.UnitZ * 15f, root, Rng);
            AddSleepingPadsPositions(Structure, rotation, farmTranslation);
        }

        private void AddSleepingPadsPositions(CollidableStructure Structure, Matrix4x4 Rotation, Vector3 Translation)
        {
            var positionsModel =
                DynamicCache.Get("Assets/Env/Structures/VegetablePlot/VegetablePlot0-Scene.ply", Scale);
            positionsModel.Transform(Rotation);
            positionsModel.Translate(Translation);
            
            var cottage = (CottageWithVegetablePlot)Structure.WorldObject;
            cottage.BanditPositions = positionsModel.Ungroup().Select(G => G.AverageVertices()).ToArray();
            cottage.Scale = Scale;
        }

        protected override BlockType PathType => BlockType.StonePath;
        protected override Vector3 NPCOffset => Vector3.UnitZ * -20f - Vector3.UnitX * 15f;
        public override int PlateauRadius => 128;

        protected override float GroundworkRadius => 32;
        public override VertexData Icon => null;

        private Vector3 Scale => WitchHutCache.Scale * StructureScale;
    }
}