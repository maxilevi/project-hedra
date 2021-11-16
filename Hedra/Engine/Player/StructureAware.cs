using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.Fonts;
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
using Hedra.Framework;
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
        private readonly Timer _insideTimer;
        private readonly Timer _updateTimer;
        private readonly Timer _soundTimer;
        private readonly List<Pair<RigidBody, CollisionGroup>> _bodies;
        private readonly HashSet<CollidableStructure> _notInsideSet;

        public StructureAware(IPlayer Player)
        {
            _player = Player;
            _insideTimer = new Timer(.5f);
            _soundTimer = new Timer(.5f);
            _updateTimer = new Timer(.5f);
            _bodies = new List<Pair<RigidBody, CollisionGroup>>();
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

            HandleSounds();
            HandleEvents();
        }

        private void HandleSounds()
        {
            if (!_soundTimer.Tick() || _currentNearStructures == null) return;
            var none = true;
            var playerPosition = _player.Position;
            for (var i = 0; i < _currentNearStructures.Length; i++)
            {
                var structure = _currentNearStructures[i];
                if ((structure.Position.Xz() - playerPosition.Xz()).LengthFast() <
                    (structure.Mountain?.Radius ?? structure.Radius))
                {
                    if (!_wasPlayingCustom && structure.Design.AmbientSongs.Length > 0)
                    {
                        var song = structure.Design.AmbientSongs[
                            Utils.Rng.Next(0, structure.Design.AmbientSongs.Length)];
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
            if (!_insideTimer.Tick() || _currentNearStructures == null) return;

            var playerPosition = _player.Position;
            /* Fill the hashset with the current structures we are inside */
            _insideStructures.Clear();
            for (var i = 0; i < _currentNearStructures.Length; i++)
            {
                var structure = _currentNearStructures[i];
                if ((structure.Position.Xz() - playerPosition.Xz()).LengthFast() <
                    (structure.Mountain?.Radius ?? structure.Radius)) // * .75f)
                    _insideStructures.Add(structure);
            }

            /* Check which structure were entered */
            foreach (var structure in _insideStructures)
                if (!_previousInsideStructures.Contains(structure))
                {
                    structure.Design.OnEnter(_player);
                    StructureEnter?.Invoke(structure);
                }

            /* Check which structures were left */
            foreach (var structure in _previousInsideStructures)
                if (!_insideStructures.Contains(structure))
                    StructureLeave?.Invoke(structure);

            /* Swap for the next iteration */
            var tmp = _insideStructures;
            _insideStructures = _previousInsideStructures;
            _previousInsideStructures = tmp;
        }

        private bool NeedsUpdating(CollidableStructure[] Structures)
        {
            if (_currentNearStructures == null || Structures.Length != _currentNearStructures.Length ||
                Structures.Sum(S => S.Colliders.Length) != NearCollisions.Length) return true;
            var differences = false;
            for (var i = 0; i < Structures.Length; i++)
                if (!_currentNearStructures.Contains(Structures[i]))
                {
                    differences = true;
                    break;
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
                Remove(removed);
                Add(added);
            });
        }

        private void Remove(CollisionGroup[] Removed)
        {
            var removedBodies = new List<RigidBody>();
            var toRemove = new List<int>();
            for (var i = 0; i < Removed.Length; ++i)
            {
                toRemove.Clear();
                for (var j = 0; j < _bodies.Count; ++j)
                    if (Removed[i] == _bodies[j].Two)
                    {
                        removedBodies.Add(_bodies[j].One);
                        toRemove.Add(j);
                    }

                for (var j = 0; j < toRemove.Count; ++j) _bodies.RemoveAt(toRemove[j]);
            }

            for (var i = 0; i < removedBodies.Count; ++i) BulletPhysics.RemoveAndDispose(removedBodies[i]);
        }

        private void Add(CollisionGroup[] Adds)
        {
            for (var i = 0; i < Adds.Length; ++i)
                _bodies.Add(new Pair<RigidBody, CollisionGroup>(BulletPhysics.AddGroup(Adds[i]), Adds[i]));
        }

        public void Discard()
        {
            Remove(NearCollisions);
        }

        public CollisionGroup[] NearCollisions { get; private set; }
    }
}