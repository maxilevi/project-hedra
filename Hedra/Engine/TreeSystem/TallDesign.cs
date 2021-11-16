using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Rendering;

namespace Hedra.Engine.TreeSystem
{
    public class TallDesign : TreeDesign
    {
        public override float Spacing => 40f;
        public override VertexData Model => CacheManager.GetModel(CacheItem.TallTrees);

        public override VertexData Paint(VertexData Data, Vector4 WoodColor, Vector4 LeafColor)
        {
            Data.Color(AssetManager.ColorCode0, WoodColor);
            Data.Color(AssetManager.ColorCode1, LeafColor * .8f);

            return Data;
        }
    }
}