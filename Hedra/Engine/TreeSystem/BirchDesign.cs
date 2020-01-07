using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Rendering;

namespace Hedra.Engine.TreeSystem
{
    public class BirchDesign : TreeDesign
    {
        public override float Spacing => 50;
        public override VertexData Model => CacheManager.GetModel(CacheItem.BirchTrees);
        public override VertexData Paint(VertexData Data, Vector4 WoodColor, Vector4 LeafColor)
        {
            Data.Color(AssetManager.ColorCode1, LeafColor);
            return Data;
        }
    }
}