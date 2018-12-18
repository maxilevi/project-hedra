using System;
using System.Linq;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Humanoid
{
    public class RoamingVillagerAIComponent : BaseVillagerAIComponent
    {
        private const int SearchRadius = 24;
        private VillageGraph Graph { get; }
        private Vector2 _targetVertex;
        private Vector2 _partialTargetVertex;
        private GraphEdge _currentEdge;
        private bool _reachedTarget;
        private readonly Timer _timeoutTimer;
        private Action _reachedCallback;
        
        public RoamingVillagerAIComponent(IHumanoid Parent, VillageGraph Graph) : base(Parent, false)
        {
            this.Graph = Graph;
            _reachedTarget = true;
            _timeoutTimer = new Timer(48 + Utils.Rng.NextFloat() * 40);
        }

        public override void Update()
        {
            base.Update();
            if (_reachedTarget)
            {
                if (MovementTimer.Tick())
                {
                    if (CanSit && true)
                        Sit();               
                    else                
                        FindAnotherSpot();             
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

        private void Sit()
        {
            var nearestBench = World.InRadius<Bench>(Parent.Position, SearchRadius).First();
            MoveTo(nearestBench.Position.Xz, delegate
            {
                nearestBench.InvokeInteraction(Parent);
            });
        }
        
        private void FindAnotherSpot()
        {
            _targetVertex = NewPoint.Xz;
            SelectNextEdge();
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

            if (allEdges.Count(E => E != currentEdge) > 0)
            {
                MoveTo(lowestVertex, delegate
                {
                    if(_targetVertex == _partialTargetVertex) 
                        _reachedTarget = true;
                    else
                        SelectNextEdge();
                });
            }
        }
        
        protected override void OnTargetPointReached()
        {
            _reachedCallback?.Invoke();
        }

        protected override void OnMovementStuck()
        {
            _reachedTarget = true;
            MovementTimer.MakeReady();
        }

        private void MoveTo(Vector2 Position, Action Callback)
        {
            _reachedTarget = false;
            _partialTargetVertex = Position;
            _reachedCallback = Callback;
        }

        private Vector2 RandomPointInsideVillage => 
            new Vector2(Utils.Rng.NextFloat() * 2 - 1, Utils.Rng.NextFloat() * 2 - 1) * .25f * Graph.Size + Parent.Position.Xz;

        private bool ShouldSit => Utils.Rng.Next(0, 2) == 1;

        private bool CanSit => World.InRadius<Bench>(Parent.Position, SearchRadius).Length > 0;
        
        protected override float WaitTime => 30 + Utils.Rng.NextFloat() * 120;

        protected override Vector3 NewPoint => Graph.GetNearestVertex(RandomPointInsideVillage).ToVector3();
    }
}