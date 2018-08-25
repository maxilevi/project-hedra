using System.Collections.Generic;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.TreeSystem
{
    public class DeadTreeDesign : TreeDesign
    {
        public override float Spacing => 60f;
        public override VertexData Model => CacheManager.GetModel(CacheItem.DeadTrees);
        public override VertexData Paint(VertexData Data, Vector4 WoodColor, Vector4 LeafColor)
        {
            Data.Color(AssetManager.ColorCode0, WoodColor);
            Data.Extradata = new List<float>();
            Data.Extradata.AddRange(Data.GenerateWindValues());

            return Data;
        }
    }
}
