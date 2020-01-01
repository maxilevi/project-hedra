using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BulletSharp;
using Hedra.Core;
using Hedra.Engine.Bullet;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Sound;
using System.Numerics;
using Hedra.Numerics;
using CollisionShape = BulletSharp.CollisionShape;
using TaskScheduler = Hedra.Core.TaskScheduler;

namespace Hedra.Engine.Player
{
    public delegate void OnStructureEnter(CollidableStructure Structure);
    public delegate void OnStructureLeave(CollidableStructure Structure);
    public delegate void OnStructureCompleted(CollidableStructure Structure);
    
    public class StructureAware : IStructureAware
    {
        public event OnStructureEnter StructureEnter;
        public event OnStructureLeave StructureLeave;
        public event OnStructureCompleted StructureCompleted;
        private bool _wasPlayingCustom;
        private readonly IPlayer _player;
        private HashSet<CollidableStructure> _insideStructures;
        private HashSet<CollidableStructure> _previousInsideStructures;
        private CollidableStructure[] _currentNearStructures;
        private readonly Timer _enterTimer;
        private readonly Timer _insideTimer;
        private readonly Timer _updateTimer;
        private readonly Dictionary<CollisionGroup, RigidBody> _bodies;
        private readonly HashSet<CollidableStructure> _notInsideSet;

        public StructureAware(IPlayer Player)
        {
            _player = Player;
            _enterTimer = new Timer(12f)
            {
                AutoReset = false
            };
            _insideTimer = new Timer(2f)
            {
                AutoReset = false
            };
            _updateTimer = new Timer(.5f);
            _enterTimer.MarkReady();
            _bodies = new Dictionary<CollisionGroup, RigidBody>();
            _notInsideSet = new HashSet<CollidableStructure>();
            _previousInsideStructures = new HashSet<CollidableStructure>();
            _insideStructures = new HashSet<CollidableStructure>();
            NearCollisions = new CollisionGroup[0];
        }
        
        public void Update()
        {
            /* Use all the structures */
            var collidableStructures = World.StructureHandler.StructureItems;

            if (_updateTimer.Tick() && NeedsUpdating(collidableStructures))
            {
                _currentNearStructures = collidableStructures.ToArray();
                SetNearCollisions(collidableStructures.SelectMany(S => S.Colliders).ToArray());
            }

            _enterTimer.Tick();
            HandleSounds();
            HandleEvents();
        }

        private void HandleSounds()
        {
            if(_currentNearStructures == null) return;
            var none = true;
            for (var i = 0; i < _currentNearStructures.Length; i++)
            {
                var structure = _currentNearStructures[i];
                if ((structure.Position.Xz() - _player.Position.Xz()).LengthFast() < (structure.Mountain?.Radius ?? structure.Radius))
                {
                    if (!_wasPlayingCustom && structure.Design.AmbientSongs.Length > 0)
                    {
                        var song = structure.Design.AmbientSongs[Utils.Rng.Next(0, structure.Design.AmbientSongs.Length)];
                        SoundtrackManager.PlayRepeating(song);
                        _wasPlayingCustom = true;
                    }
                    none = false;
                }
            }
            if (_wasPlayingCustom && none)
            {
                _wasPlayingCustom = false;
                SoundtrackManager.PlayAmbient();
            }
        }

        private void HandleEvents()
        {
            if(!_insideTimer.Tick() || _currentNearStructures == null) return;

            /* Fill the hashset with the current structures we are inside */
            _insideStructures.Clear();
            for (var i = 0; i < _currentNearStructures.Length; i++)
            {
                var structure = _currentNearStructures[i];
                if ((structure.Position.Xz() - _player.Position.Xz()).LengthFast() < (structure.Mountain?.Radius ?? structure.Radius))// * .75f)
                {
                    _insideStructures.Add(structure);
                }
            }

            /* Check which structure were entered */
            foreach (var structure in _insideStructures)
            {
                if (!_previousInsideStructures.Contains(structure))
                {
                    structure.Design.OnEnter(_player);
                    StructureEnter?.Invoke(structure);
                }
            }

            /* Check which structures were left */
            foreach (var structure in _previousInsideStructures)
            {
                if (!_insideStructures.Contains(structure))
                {
                    StructureLeave?.Invoke(structure);
                }
            }

            /* Swap for the next iteration */
            var tmp = _insideStructures;
            _insideStructures = _previousInsideStructures;
            _previousInsideStructures = tmp;
        }
        
        private bool NeedsUpdating(CollidableStructure[] Structures)
        {
            if (_currentNearStructures == null || Structures.Length != _currentNearStructures.Length || Structures.Sum(S => S.Colliders.Length) != NearCollisions.Length) return true;
            var differences = false;
            for (var i = 0; i < Structures.Length; i++)
            {
                if (!_currentNearStructures.Contains(Structures[i]))
                {
                    differences = true;
                    break;
                }
            }
            return differences;
        }

        private void SetNearCollisions(CollisionGroup[] New)
        {
            var added = New.Except(NearCollisions).ToArray();
            var removed = NearCollisions.Except(New).ToArray();
            NearCollisions = New;
            TaskScheduler.Parallel(() =>
            {
                for (var i = 0; i < removed.Length; ++i)
                {
                    BulletPhysics.RemoveAndDispose(_bodies[removed[i]]);
                    _bodies.Remove(removed[i]);
                }

                for (var i = 0; i < added.Length; ++i)
                {
                    _bodies[added[i]] = BulletPhysics.AddGroup(added[i]);
                }
            });
        }
        
        public CollisionGroup[] NearCollisions { get; private set; }
    }
}