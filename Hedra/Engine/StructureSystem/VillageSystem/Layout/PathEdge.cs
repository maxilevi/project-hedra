using Hedra.Engine.Rendering.Animation.ColladaParser;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Layout
{
    internal class PathEdge
    {
        public PathVertex Origin { get; set; }
        public PathVertex End { get; set; }
    }
}