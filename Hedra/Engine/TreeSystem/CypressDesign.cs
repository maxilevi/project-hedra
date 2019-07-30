using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.TreeSystem
{
    public class CypressDesign : TreeDesign
    {
        public override float Spacing => 50f;
        public override VertexData Model => CacheManager.GetModel(CacheItem.CypressTrees);

        public override VertexData Paint(VertexData Data, Vector4 WoodColor, Vector4 LeafColor)
        {
            Data.Color(AssetManager.ColorCode0, WoodColor);
            Data.Color(AssetManager.ColorCode1, LeafColor * .95f);

            return Data;
        }
    }
}
