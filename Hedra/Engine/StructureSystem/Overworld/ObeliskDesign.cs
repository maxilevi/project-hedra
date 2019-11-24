using System;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class ObeliskDesign : StructureDesign
    {
        public override int PlateauRadius { get; } = 256;
        public override VertexData Icon => null;
        public override bool CanSpawnInside => true;

        public override void Build(CollidableStructure Structure)
        {
            var position = Structure.Position;
            var obelisk = (Obelisk) Structure.WorldObject;
            var scale = Vector3.One * 6;
            var rng = new Random( (int)(position.X / 11 * (position.Z / 13)) );
            var originalModel = CacheManager.GetModel(CacheItem.Obelisk);
            var model = originalModel.ShallowClone();
            var shapes = CacheManager.GetShape(originalModel).DeepClone().ToArray();
            
            var typeColor = Utils.VariateColor(Obelisk.GetObeliskColor(obelisk.Type), 15, rng);
            var darkColor = Obelisk.GetObeliskStoneColor(rng);

            model.Scale(scale);
            model.Color(new Vector4(.2f, .2f, .2f, 1f), darkColor);
            model.Color(new Vector4(.6f, .6f, .6f, 1f), Obelisk.GetObeliskStoneColor(rng));
            model.Translate(position);
            model.Extradata.Clear();
            model.FillExtraData(WorldRenderer.NoHighlightFlag);

            for (var i = 0; i < shapes.Length; ++i)
            {
                shapes[i].Transform(Matrix4x4.CreateScale(scale));
                shapes[i].Transform(Matrix4x4.CreateTranslation(position));
            }
            
            obelisk.Type = (ObeliskType) Utils.Rng.Next(0, (int)ObeliskType.MaxItems);

            Structure.AddCollisionShape(shapes);
            Structure.AddStaticElement(model);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng, new Obelisk(TargetPosition));
            structure.Mountain.Radius = 32;
            return structure;
        }

        protected override bool SetupRequirements(ref Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            var height = Biome.Generation.GetMaxHeight(TargetPosition.X, TargetPosition.Z);
            return Rng.Next(0, StructureGrid.ObeliskChance) == 1 && height > BiomePool.SeaLevel && Math.Abs(Biome.Generation.RiverAtPoint(TargetPosition.X, TargetPosition.Z)) < 0.005f;
        }
    }
}
