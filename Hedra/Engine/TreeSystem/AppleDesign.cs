using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Rendering;

namespace Hedra.Engine.TreeSystem
{
    public class AppleDesign : TreeDesign
    {
        public override float Spacing => 120f;
        public override VertexData Model => CacheManager.GetModel(CacheItem.AppleTrees);

        public override VertexData Paint(VertexData Data, Vector4 WoodColor, Vector4 LeafColor)
        {
            Data.Color(AssetManager.ColorCode0, WoodColor);
            Data.Color(AssetManager.ColorCode1, LeafColor * .95f);

            return Data;
        }
    }
}