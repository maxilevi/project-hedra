using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkMeshBuildOutput
    {
        public NativeVertexData StaticData { get; set; }
        public NativeVertexData WaterData { get; set; }
        public NativeVertexData InstanceData { get; set; }
        public bool Failed { get; set; }

        public ChunkMeshBuildOutput(NativeVertexData StaticData, NativeVertexData WaterData, NativeVertexData InstanceData, bool Failed)
        {
            this.StaticData = StaticData;
            this.WaterData = WaterData;
            this.InstanceData = InstanceData;
            this.Failed = Failed;
        }

        public void Dispose()
        {
            StaticData?.Dispose();
            WaterData?.Dispose();
            InstanceData?.Dispose();
        }
    }
}
