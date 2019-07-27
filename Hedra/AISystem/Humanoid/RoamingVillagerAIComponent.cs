using System;
using System.Linq;
using System.Security.Cryptography;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Game;
using OpenTK;

namespace Hedra.AISystem.Humanoid
{
    public sealed class RoamingVillagerAIComponent : RawMovementHumanoidAIComponent
    {
        private const int BenchSearchRadius = 24;
        private const int TalkSearchRadius = 8;
        protected override bool ShouldSleep => true;
        private float MaxSpeed { get; }
        private VillageGraph Graph { get; }
        private Timer LookTimer { get; }
        private Vector2 _targetVertex;
        private Vector2 _partialTargetVertex;
        private GraphEdge _currentEdge;
        private bool _reachedTarget;
        private readonly Timer _timeoutTimer;
        private Action _reachedCallback;
        private float _errorMargin = DefaultErrorMargin;
        private readonly Timer _interactionTimer;
        private Vector2 _lastPosition;
        private bool _isInteracting;
        private readonly Timer _sitTimer;
        private readonly Timer _talkTimer;
        private float _targetSpeed;
        private readonly Timer _movementTimer;
        
        public RoamingVillagerAIComponent(IHumanoid Parent, VillageGraph Graph) : base(Parent)
        {
            this.Graph = Graph;
            _reachedTarget = true;
            _movementTimer = new Timer(WaitTime);
            _movementTimer.MarkReady();
            MaxSpeed = Utils.Rng.NextFloat() * .25f + .8f;
            _timeoutTimer = new Timer(48 + Utils.Rng.NextFloat() * 40);
            _interactionTimer = new Timer(8 + Utils.Rng.NextFloat() * 14f);
            _sitTimer = new Timer(Utils.Rng.NextFloat() * 8f + 8)
            {
                AutoReset = false
            };
            _talkTimer = new Timer(Utils.Rng.NextFloat() * 8f + 8)
            {
                AutoReset = false
            };
            LookTimer = new Timer(Utils.Rng.NextFloat() * 8f + 6);
        }

        public override void Update()
        {
            if (!base.CanUpdate) return;

            if (Parent.IsNear(GameManager.Player, 16) && !_isInteracting || (Parent.SearchComponent<TalkComponent>()?.Talking ?? false))
            {
                Parent.LookAt(GameManager.Player);
                _targetSpeed = 0;
            }
            else
            {
                if (_reachedTarget && !_isInteracting)
                {
                    if (_movementTimer.Tick())
                        FindLocation();
                    else if (LookTimer.Tick())
                        LookAtRandom();
                }
                else if (!_reachedTarget)
                {
                    if (_timeoutTimer.Tick())
                        _reachedTarget = true;
                    else
                        base.Move(_partialTargetVertex.ToVector3(), _errorMargin);
                }

                if (!_isInteracting)
                    ManageInteractions();
                _talkTimer.Tick();
                _sitTimer.Tick();
                _targetSpeed = MaxSpeed;
            }
            Parent.Speed = Mathf.Lerp(Parent.Speed, _targetSpeed, Time.DeltaTime);
        }

        private void LookAtRandom()
        {
            Physics.DirectionToEuler(
                (new Vector3(Utils.Rng.NextFloat(), Utils.Rng.NextFloat(), Utils.Rng.NextFloat()) * 2 - Vector3.One).Xz.NormalizedFast().ToVector3()
            );
        }
        
        private void FindLocation()
        {
            if (Utils.Rng.Next(0, 5) == 1)
                GoToMarket();
            else
                FindRandomSpot();
        }

        private void ManageInteractions()
        {
            if ((Parent.BlockPosition.Xz - _lastPosition).LengthSquared < Chunk.BlockSize*Chunk.BlockSize) return;
            _lastPosition = Parent.BlockPosition.Xz;
            if (ShouldSit && CanSit(out var bench))
                Sit(bench);
            if (ShouldTalk && CanTalk(out var human))
                Talk(human);
        }


        private bool CanSit(out Bench Bench)
        {
            return null != (Bench = World.InRadius<Bench>(Parent.Position, BenchSearchRadius).FirstOrDefault(
                            B => !B.IsOccupied
                       )) && _sitTimer.Ready;
        }

        private bool CanTalk(out IHumanoid Human)
        {
            return null != (Human = World.InRadius<IHumanoid>(Parent.Position, TalkSearchRadius)
                       .FirstOrDefault(
                           V => V.SearchComponent<RoamingVillagerAIComponent>() != null 
                                && V != Parent
                                && (!V.SearchComponent<TalkComponent>()?.Talking ?? false)
                           )
                       ) && _talkTimer.Ready;
        }
        
        private void Talk(IHumanoid NearestVillager)
        {
            MoveTo(NearestVillager.Position.Xz, delegate
            {
                if (Parent.Disposed) return;
                var time = Utils.Rng.NextFloat() * 8 + 8;
                TalkWith(NearestVillager, time);
                NearestVillager.SearchComponent<RoamingVillagerAIComponent>().TalkWith(Parent, time);
            }, 8);
            _isInteracting = true;
        }
        
        /* Called from RoamingVillagerAIComponent::Talk */
        private void TalkWith(IHumanoid Humanoid, float Time)
        {
            Parent.LookAt(Humanoid);
            Parent.SearchComponent<TalkComponent>()?.Simulate(Humanoid, Time, delegate
            {
                _talkTimer.Reset();
                _isInteracting = false;
                FindLocation();
            });
            _reachedTarget = true;
            _isInteracting = true;
        }
        
        private void Sit(InteractableStructure NearestBench)
        {
            MoveTo(NearestBench.Position.Xz, delegate
            {
                NearestBench.InvokeInteraction(Parent);
                TaskScheduler.After(Utils.Rng.NextFloat() * 8 + 12, delegate
                {
                    if (Parent.Disposed) return;
                    _sitTimer.Reset();
                    _isInteracting = false;
                    Parent.IsSitting = false;
                    FindLocation();
                });
            }, 8);
            _isInteracting = true;
        }
        
        private void GoToMarket()
        {
            var nearestVillage = World.InRadius<Village>(Parent.Position, VillageDesign.MaxVillageRadius).FirstOrDefault();
            if (nearestVillage == null) return;
            MoveTo(nearestVillage.Position.Xz, delegate
            {
                _interactionTimer.MarkReady();
            }, MarketParameters.MarketSize * .25f);
        }
        
        private void FindRandomSpot()
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
                MoveTo(lowestVertex + randomUnit * 24f, delegate
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
            base.OnMovementStuck();
            _reachedTarget = true;
            _movementTimer.MarkReady();
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

        private bool ShouldTalk => true;//Utils.Rng.Next(0, 2) == 1;
        
        protected float WaitTime => 8 + Utils.Rng.NextFloat() * 10;

        protected Vector3 NewPoint => Graph.GetNearestVertex(RandomPointInsideVillage).ToVector3();
    }
}