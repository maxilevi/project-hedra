namespace Hedra.Engine.Rendering
{
    public class LodedVertexData
    {
        public CompressedVertexData NormalModel { get; set; }
        public CompressedVertexData LodedModel { get; set; }
    }
}