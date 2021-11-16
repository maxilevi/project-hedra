using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Rendering;

namespace Hedra.Engine.TreeSystem
{
    public class WillowDesign : TreeDesign
    {
        public override float Spacing => 80f;
        public override VertexData Model => CacheManager.GetModel(CacheItem.WillowTrees);

        public override VertexData Paint(VertexData Data, Vector4 WoodColor, Vector4 LeafColor)
        {
            Data.Color(AssetManager.ColorCode0, WoodColor * .75f);
            Data.Color(AssetManager.ColorCode1, LeafColor * .45f);
            return Data;
        }
    }
}