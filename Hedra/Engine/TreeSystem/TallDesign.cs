using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

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
