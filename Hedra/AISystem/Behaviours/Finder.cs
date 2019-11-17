using System.Linq;
using System.Numerics;
using Hedra.Engine;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Scenes;
using Hedra.Engine.StructureSystem;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public static class Finder
    {
        public static void UpdateGrid(IEntity Parent, WaypointGrid Graph)
        {
            var graph = (WaypointGraph)null;
            var nearbyGraphs = StructureHandler.GetNearStructures(Parent.Position).Where(S => S.Waypoints != null).Select(S => S.Waypoints).ToArray();
            for (var i = 0; i < nearbyGraphs.Length; ++i)
            {
                nearbyGraphs[i].GetNearestVertex(Parent.Position, out var distance);
                if (distance <= Chunk.BlockSize * 2)
                {
                    if (graph == null) graph = nearbyGraphs[i].Clone();
                    else graph.MergeGraph(nearbyGraphs[i], (int)Chunk.BlockSize);
                }
            }

            if (graph == null)
            {
                SampleGridAndMergeGraphs(Parent, Graph, nearbyGraphs);
            }
            else
            {
                Graph.Copy(graph);
            }
        }

        private static void SampleGridAndMergeGraphs(IEntity Parent, WaypointGrid Graph, WaypointGraph[] NearbyGraphs)
        {
            Graph.Rebuild(Parent.Position - new Vector3(Graph.DimX, 0, Graph.DimY) * Chunk.BlockSize / 2,
                Chunk.BlockSize);
            for (var x = 0; x < Graph.DimX; ++x)
            {
                for (var y = 0; y < Graph.DimY; ++y)
                {
                    var position = new Vector2(x, y);
                    var realPosition = ToWorldCoordinates(position, Graph.DimX, Graph.DimY);
                    if (Parent.Physics.CollidesWithOffset(realPosition))
                        Graph.UnlinkVertex(position);
                }
            }

            for (var i = 0; i < NearbyGraphs.Length; ++i)
            {
                Graph.MergeGraph(NearbyGraphs[i], (int) Chunk.BlockSize);
            }
        }

        private static Vector3 ToWorldCoordinates(Vector2 Position, int DimX, int DimY)
        {
            return (Position - new Vector2((int) (DimX / 2f), (int) (DimY / 2f))).ToVector3() *
                   Chunk.BlockSize;
        }
    }
}