using System;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class ObeliskDesign : StructureDesign
    {
        public override int PlateauRadius { get; } = 256;
        public override VertexData Icon => null;

        public override void Build(CollidableStructure Structure)
        {
            var position = Structure.Position;
            var obelisk = (Obelisk) Structure.WorldObject;
            var scale = Vector3.One * 6;
            var rng = new Random( (int)(position.X / 11 * (position.Z / 13)) );
            var originalModel = CacheManager.GetModel(CacheItem.Obelisk);
            var model = originalModel.ShallowClone();
            model.Scale(scale);

            var collisionBox = new Box(new Vector3(0, 0, 0), new Vector3(2.4f, 8, 2.4f) * scale);
            collisionBox += new Box(position, position);
            collisionBox += new Box(new Vector3(0, collisionBox.Max.Y - collisionBox.Min.Y, 0) * .5f, new Vector3(0, collisionBox.Max.Y - collisionBox.Min.Y, 0) * .5f);

            var Base = new Vector3(collisionBox.Max.X - collisionBox.Min.X, collisionBox.Max.Y - collisionBox.Min.Y, collisionBox.Max.Z - collisionBox.Min.Z) * .5f;
            collisionBox -= new Box(Base, Base);
            obelisk.Type = (ObeliskType) Utils.Rng.Next(0, (int)ObeliskType.MaxItems);

            //data.VariateColors(0.05f, rng);
            var typeColor = Utils.VariateColor(Obelisk.GetObeliskColor(obelisk.Type), 15, rng);

            var darkColor = Obelisk.GetObeliskStoneColor(rng);

            model.Color(new Vector4(.2f, .2f, .2f, 1f), darkColor);
            //data.Color(new Vector4(.4f, .4f, .4f, 1f), typeColor);
            model.Color(new Vector4(.6f, .6f, .6f, 1f), Obelisk.GetObeliskStoneColor(rng));

            model.Translate(position);
            model.Extradata.Clear();
            model.FillExtraData(WorldRenderer.NoHighlightFlag);

            Structure.AddCollisionShape(collisionBox.ToShape());
            Structure.AddStaticElement(model);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng, new Obelisk(TargetPosition));
            structure.Mountain.Radius = 32;
            return structure;
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            var height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);
            return Rng.Next(0, StructureGrid.ObeliskChance) == 1 && height > BiomePool.SeaLevel && Math.Abs(LandscapeGenerator.River(TargetPosition.Xz)) < 0.005f;
        }
    }
}
