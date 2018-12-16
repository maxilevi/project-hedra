using System.Linq;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Management;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Humanoid
{
    public class RoamingVillagerAIComponent : BaseVillagerAIComponent
    {
        private VillageGraph Graph { get; }
        private Vector2 _targetVertex;
        private Vector2 _partialTargetVertex;
        private GraphEdge _currentEdge;
        private bool _reachedTarget;
        private readonly Timer _timeoutTimer;
        
        public RoamingVillagerAIComponent(IHumanoid Parent, VillageGraph Graph) : base(Parent, false)
        {
            this.Graph = Graph;
            _reachedTarget = true;
            _timeoutTimer = new Timer(60);
        }

        public override void Update()
        {
            base.Update();
            if (_reachedTarget)
            {
                if (MovementTimer.Tick())
                {
                    _reachedTarget = false;
                    _targetVertex = NewPoint.Xz;
                    SelectNextEdge();
                }
            }
            else
            {
                if (_timeoutTimer.Tick())
                    _reachedTarget = true;
                else
                    base.Move(_partialTargetVertex.ToVector3());
            }
        }

        private void SelectNextEdge()
        {
            var currentVertex = Graph.GetNearestVertex(Parent.Position.Xz);
            var currentVertexIndex = Graph.GetIndex(currentVertex);
            var allEdges = Graph.GetEdgesWithVertex(currentVertex);
            var lowest = float.MaxValue;
            var lowestVertex = Vector2.Zero;
            var currentEdge = _currentEdge;
            for (var i = 0; i < allEdges.Length; ++i)
            {
                if (currentEdge == allEdges[i]) continue;
                var otherVertexIndex = allEdges[i].GetOtherVertex(currentVertexIndex);
                var otherVertex = Graph.FromIndex(otherVertexIndex);
                var dist = (otherVertex - _targetVertex).LengthSquared;
                if (dist < lowest)
                {
                    lowest = dist;
                    lowestVertex = otherVertex;
                    _currentEdge = allEdges[i];
                }
            }
            if(allEdges.Count(E => E != currentEdge) > 0) _partialTargetVertex = lowestVertex;
        }
        
        protected override void OnTargetPointReached()
        {
            if(_targetVertex == _partialTargetVertex) 
                _reachedTarget = true;
            else
                SelectNextEdge();
        }

        private Vector2 RandomPointInsideVillage => 
            new Vector2(Utils.Rng.NextFloat() * 2 - 1, Utils.Rng.NextFloat() * 2 - 1) * .25f * Graph.Size + Parent.Position.Xz;

        protected override float WaitTime => 30 + Utils.Rng.NextFloat() * 120;

        protected override Vector3 NewPoint => Graph.GetNearestVertex(RandomPointInsideVillage).ToVector3();
    }
}