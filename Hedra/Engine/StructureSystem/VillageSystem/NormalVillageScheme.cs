using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class NormalVillageScheme : VillageScheme
    {
        public override Vector3 StablePosition => -Vector3.UnitX * 160.0f - Vector3.UnitZ * 150.0f;
        public override Vector3 BlacksmithPosition => -Vector3.UnitZ * 30 - Vector3.UnitX * 180f;
        public override Vector3 MarketPosition => -Vector3.UnitY * 2.0f + Vector3.UnitZ * 140.0f;
        public override Vector3 WindmillPosition => -Vector3.UnitY * 2.0f - Vector3.UnitZ * 140.0f;
        public override Vector3 FarmPosition =>  -Vector3.UnitZ * 140.0f;

        public NormalVillageScheme()
        {
            this.AddGroundwork(MarketPosition, 96);
            this.AddGroundwork(FarmPosition + Vector3.UnitX * 360f, 96);
            this.AddGroundwork(Vector3.UnitX * 180.0f - Vector3.UnitZ * 140.0f, 64);
            this.AddGroundwork(FarmPosition, 96);
            this.AddGroundwork(FarmPosition + Vector3.UnitX * 220.0f + Vector3.UnitZ * 180, 96);
            this.AddGroundwork(FarmPosition + Vector3.UnitX * 220.0f + Vector3.UnitZ * 400, 96);
            this.AddGroundwork(Vector3.UnitZ * -30 + Vector3.UnitX * -180, 64);

            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    this.AddGroundwork(
                        Vector3.UnitX * 241.0f + Vector3.UnitZ * 60.0f
                        + Vector3.UnitX * i * Chunk.BlockSize * 20f
                        + Vector3.UnitZ * j * Chunk.BlockSize * 20f, 48);
                }
            }

            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    this.AddGroundwork(
                        Vector3.UnitX * 360.0f + Vector3.UnitZ * 60.0f
                        + Vector3.UnitX * i * Chunk.BlockSize * 20f
                        + Vector3.UnitZ * j * Chunk.BlockSize * 20f, 48);
                }
            }

            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    this.AddGroundwork(
                        Vector3.UnitX * 140.0f - Vector3.UnitZ * 175.0f
                        + Vector3.UnitX * i * Chunk.BlockSize * 20f
                        + Vector3.UnitZ * j * Chunk.BlockSize * 20f, 48);
                }
            }
        }
    }
}
