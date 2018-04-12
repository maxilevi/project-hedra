﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using OpenTK;
using ObeliskType = Hedra.Engine.QuestSystem.ObeliskType;

namespace Hedra.Engine.StructureSystem
{
    public class ObeliskDesign : StructureDesign
    {
        public override int Radius { get; set; } = 256;

        public override void Build(Vector3 Position, CollidableStructure Structure)
        {
            Chunk underChunk = World.GetChunkAt(Position);
            Vector3 scale = Vector3.One * 6;
            var rng = new Random( (int)(Position.X / 11 * (Position.Z / 13)) );
            VertexData data = AssetManager.PlyLoader("Assets/Env/Obelisk.ply", scale);
            var obelisk = new Obelisk
            {
                Position = Position,
            };

            var collisionBox = new Box(new Vector3(0, 0, 0), new Vector3(2.4f, 8, 2.4f) * scale);
            collisionBox += new Box(obelisk.Position, obelisk.Position);
            collisionBox += new Box(new Vector3(0, collisionBox.Max.Y - collisionBox.Min.Y, 0) * .5f, new Vector3(0, collisionBox.Max.Y - collisionBox.Min.Y, 0) * .5f);

            Vector3 Base = new Vector3(collisionBox.Max.X - collisionBox.Min.X, collisionBox.Max.Y - collisionBox.Min.Y, collisionBox.Max.Z - collisionBox.Min.Z) * .5f;
            collisionBox -= new Box(Base, Base);
            underChunk.AddCollisionShape(collisionBox);
            obelisk.Type = (ObeliskType) Utils.Rng.Next(0, (int)ObeliskType.MaxItems);

            //data.VariateColors(0.05f, rng);
            Vector4 typeColor = Utils.VariateColor(Obelisk.GetObeliskColor(obelisk.Type), 15, rng);

            Vector4 darkColor = Obelisk.GetObeliskStoneColor(rng);

            data.Color(new Vector4(.2f, .2f, .2f, 1f), darkColor);
            //data.Color(new Vector4(.4f, .4f, .4f, 1f), typeColor);
            data.Color(new Vector4(.6f, .6f, .6f, 1f), Obelisk.GetObeliskStoneColor(rng));

            data.Transform(obelisk.Position);
            data.ExtraData.Clear();
            data.FillExtraData(WorldRenderer.NoHighlightFlag);

            underChunk.AddStaticElement(data);

            World.HighlightArea(obelisk.Position, new Vector4(.2f, .2f, .2f, .4f), 48, -1);

            World.AddStructure(obelisk);
            World.AddChunkToQueue(underChunk, true);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Region Biome, Random Rng)
        {
            BlockType type;
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);

            var plateau = new Plateau(TargetPosition, 32, 8, height);
            World.QuestManager.AddPlateau(plateau);
            return new CollidableStructure(this, TargetPosition, plateau);
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng)
        {
            BlockType type;
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);

            return Rng.Next(0, 10) == 1 && height > 0;
        }
    }
}
