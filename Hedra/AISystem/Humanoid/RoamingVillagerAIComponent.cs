using System;
using System.Linq;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Humanoid
{
    public class RoamingVillagerAIComponent : BaseVillagerAIComponent
    {
        private const int BenchSearchRadius = 24;
        private const int TalkSearchRadius = 8;
        private VillageGraph Graph { get; }
        private Vector2 _targetVertex;
        private Vector2 _partialTargetVertex;
        private GraphEdge _currentEdge;
        private bool _reachedTarget;
        private readonly Timer _timeoutTimer;
        private Action _reachedCallback;
        private float _errorMargin = DefaultErrorMargin;
        private readonly Timer _interactionTimer;
        
        public RoamingVillagerAIComponent(IHumanoid Parent, VillageGraph Graph) : base(Parent, false)
        {
            this.Graph = Graph;
            _reachedTarget = true;
            MovementTimer.MakeReady();
            _timeoutTimer = new Timer(48 + Utils.Rng.NextFloat() * 40);
            _interactionTimer = new Timer(8 + Utils.Rng.NextFloat() * 14f);
            Parent.Speed = .85f;
        }

        public override void Update()
        {
            base.Update();
            if (_reachedTarget)
            {
                if (MovementTimer.Tick())
                {
                    if (Utils.Rng.Next(0, 5) == 1)
                        GoToMarket();
                    else
                        FindAnotherSpot();
                }
                else if (_interactionTimer.Tick())
                {
                    ManageInteractions();
                }
            }
            else
            {
                if (_timeoutTimer.Tick())
                    _reachedTarget = true;
                else
                    base.Move(_partialTargetVertex.ToVector3(), _errorMargin);

                ManageInteractions();
            }
        }

        private void ManageInteractions()
        {
            if (CanSit && ShouldSit)
                Sit();
            if (CanTalk && ShouldTalk)
                Talk();
        }

        private void Talk()
        {
            var nearestVillager = World.InRadius<IHumanoid>(Parent.Position, TalkSearchRadius)
                .First(V => V.SearchComponent<RoamingVillagerAIComponent>() != null);
            MoveTo(nearestVillager.Position.Xz, delegate
            {
                nearestVillager.SearchComponent<TalkComponent>().Simulate(Utils.Rng.NextFloat() * 12 + 6);
            }, 8);
            _reachedTarget = true;
        }
        
        private void GoToMarket()
        {
            var nearestVillage = World.InRadius<Village>(Parent.Position, VillageDesign.MaxVillageRadius).First();
            MoveTo(nearestVillage.Position.Xz, delegate
            {
                _interactionTimer.MakeReady();
            }, MarketParameters.MarketSize * .25f);
        }
        
        private void Sit()
        {
            var nearestBench = World.InRadius<Bench>(Parent.Position, BenchSearchRadius).First();
            MoveTo(nearestBench.Position.Xz, delegate
            {
                nearestBench.InvokeInteraction(Parent);
            });
            _reachedTarget = true;
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
                var randomUnit = new Vector2(Utils.Rng.NextFloat() * 2 - 1, Utils.Rng.NextFloat() * 2 - 1);
                MoveTo(lowestVertex + randomUnit * 12f, delegate
                {
                    if(_targetVertex == _partialTargetVertex) 
                        _reachedTarget = true;
                    else
                        SelectNextEdge();
                }, 12);
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

        private void MoveTo(Vector2 Position, Action Callback, float ErrorMargin = DefaultErrorMargin)
        {
            _reachedTarget = false;
            _partialTargetVertex = Position;
            _reachedCallback = Callback;
            _errorMargin = ErrorMargin;
        }

        private Vector2 RandomPointInsideVillage => 
            new Vector2(Utils.Rng.NextFloat() * 2 - 1, Utils.Rng.NextFloat() * 2 - 1) * .25f * Graph.Size + Parent.Position.Xz;

        private bool ShouldSit => Utils.Rng.Next(0, 2) == 1;

        private bool CanSit => World.InRadius<Bench>(Parent.Position, BenchSearchRadius).Length > 0;

        private bool ShouldTalk => true;//Utils.Rng.Next(0, 2) == 1;

        private bool CanTalk => World.InRadius<IHumanoid>(Parent.Position, TalkSearchRadius)
                                    .Any(V => V.SearchComponent<RoamingVillagerAIComponent>() != null);
        
        protected override float WaitTime => 30 + Utils.Rng.NextFloat() * 120;

        protected override Vector3 NewPoint => Graph.GetNearestVertex(RandomPointInsideVillage).ToVector3();
    }
}