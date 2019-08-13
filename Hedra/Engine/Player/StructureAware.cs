using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Sound;
using OpenTK;
using CollisionShape = BulletSharp.CollisionShape;

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
        private CollidableStructure _insideStructure;
        private CollidableStructure[] _currentNearStructures;
        private readonly Timer _enterTimer;
        private readonly Timer _insideTimer;
        private readonly Timer _updateTimer;
        private readonly Dictionary<CollisionGroup, object> _keys;

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
            _keys = new Dictionary<CollisionGroup, object>();
            NearCollisions = new CollisionGroup[0];
        }
        
        public void Update()
        {
            var collidableStructures = StructureHandler.GetNearStructures(_player.Position);

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
                if ((structure.Position.Xz - _player.Position.Xz).LengthFast < structure.Radius * .75f)
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
            var isInsideAny = false;
            for (var i = 0; i < _currentNearStructures.Length; i++)
            {
                var structure = _currentNearStructures[i];
                if ((structure.Position.Xz - _player.Position.Xz).LengthFast < structure.Radius * .75f)
                {
                    isInsideAny = true;
                    if (_insideStructure == null)
                    {
                        _insideStructure = structure;
                        _insideStructure.Design.OnEnter(_player);
                        StructureEnter?.Invoke(_insideStructure);
                        break;
                    }
                }
            }

            if (!isInsideAny && _insideStructure != null)
            {
                /* _insideStructure.Design.OnLeave(_player); */
                StructureLeave?.Invoke(_insideStructure);
                _insideStructure = null;
            }
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
            
            for (var i = 0; i < removed.Length; ++i)
            {
                BulletPhysics.BulletPhysics.RemoveCustom(_keys[removed[i]]);
            }
            
            for (var i = 0; i < added.Length; ++i)
            {
                _keys[added[i]] = new object();
                BulletPhysics.BulletPhysics.AddCustom(_keys[added[i]], added[i].Colliders);
            }
        }
        
        public CollisionGroup[] NearCollisions { get; private set; }
    }
}