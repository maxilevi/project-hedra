using System;
using System.Linq;
using System.Numerics;
using Hedra.Engine;
using Hedra.Engine.Scenes;
using Hedra.Engine.StructureSystem;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class DungeonTraverseBehaviour : TraverseBehaviour
    {
        protected WaypointGraph Graph { get; set; }
        public DungeonTraverseBehaviour(IEntity Parent, bool UseCollision = false) : base(Parent, UseCollision)
        {
            Parent.Physics.CollidesWithEntities = false;
        }

        protected override void CreateGraph()
        {
        }

        public override void Update()
        {
            if(Graph == null)
                Graph = StructureHandler.GetNearStructures(Parent.Position).First(S => S.Waypoints != null).Waypoints;
            base.Update();
        }

        protected override void UpdateGraph()
        {
        }

        protected override void RebuildGraph()
        {
        }

        protected override void ForceRebuildGraph()
        {
            UpdatePath();
        }

        protected override void DisposeGraph()
        {
        }

        protected override Vector2[] DoUpdatePath(Vector3 Origin, out bool CanReach)
        {
            CanReach = true;
            var sourceVertex = Graph.GetNearestVertex(Origin);
            var targetVertex = Graph.GetNearestVertex(Target);
            return Graph.GetShortestPath(sourceVertex, targetVertex).Select(W => W.Position.Xz()).ToArray();
        }

        protected override Vector3 CalculateTargetPoint(Vector2 PathPoint)
        {
            return PathPoint.ToVector3() + Parent.Position.Y * Vector3.UnitY;
        }
    }
}