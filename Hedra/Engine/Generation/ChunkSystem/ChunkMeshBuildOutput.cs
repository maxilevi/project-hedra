using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkMeshBuildOutput
    {
        public VertexData StaticData { get; set; }
        public VertexData WaterData { get; set; }
        public VertexData InstanceData { get; set; }
        public bool Failed { get; set; }

        public ChunkMeshBuildOutput(VertexData StaticData, VertexData WaterData, VertexData InstanceData, bool Failed)
        {
            this.StaticData = StaticData;
            this.WaterData = WaterData;
            this.InstanceData = InstanceData;
            this.Failed = Failed;
        }

        public void Dispose()
        {
            StaticData.Dispose();
            WaterData.Dispose();
        }
    }
}
