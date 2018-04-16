﻿using Hedra.Engine.Rendering;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkMeshBuildOutput
    {
        public VertexData StaticData { get; set; }
        public VertexData WaterData { get; set; }
        public bool Failed { get; set; }
        public bool HasNoise3D { get; set; }
        public bool HasWater { get; set; }

        public ChunkMeshBuildOutput(VertexData StaticData, VertexData WaterData, bool Failed, bool HasNoise3D, bool HasWater)
        {
            this.StaticData = StaticData;
            this.WaterData = WaterData;
            this.Failed = Failed;
            this.HasNoise3D = HasNoise3D;
            this.HasWater = HasWater;
        }

        public void Dispose()
        {
            StaticData.Dispose();
            WaterData.Dispose();
        }
    }
}