using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using System.Numerics;
using Hedra.Engine.Scenes;
using Hedra.Numerics;

namespace Hedra.Engine.Pathfinding
{
    public static class Finder
    {
        public static void UpdateGrid(IEntity Parent, WaypointGrid Graph)
        {
            for (var x = 0; x < Graph.DimX; ++x)
            {
                for (var y = 0; y < Graph.DimY; ++y)
                {
                    var realPosition = ToWorldCoordinates(new Vector2(x, y), Graph.DimX, Graph.DimY);
                    Graph.LinkVertex(realPosition.Xz());
                    if(Parent.Physics.CollidesWithOffset(realPosition))
                        Graph.UnlinkVertex(realPosition.Xz());
                }
            }
        }

        private static Vector3 ToWorldCoordinates(Vector2 Position, int DimX, int DimY)
        {
            return (Position - new Vector2((int) (DimX / 2f), (int) (DimY / 2f))).ToVector3() *
                   Chunk.BlockSize;
        }
    }
}