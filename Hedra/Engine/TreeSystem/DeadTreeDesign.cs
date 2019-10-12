using System.Collections.Generic;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.TreeSystem
{
    public class DeadTreeDesign : TreeDesign
    {
        public override float Spacing => 80f;
        public override VertexData Model => CacheManager.GetModel(CacheItem.DeadTrees);
        public override VertexData Paint(VertexData Data, Vector4 WoodColor, Vector4 LeafColor)
        {
            Data.Color(AssetManager.ColorCode0, WoodColor);
            Data.AddWindValues();
            return Data;
        }
    }
}
