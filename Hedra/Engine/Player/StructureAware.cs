using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.StructureSystem;
using OpenTK;

namespace Hedra.Engine.Player
{
    public delegate void OnStructureEnter(CollidableStructure Structure);
    
    public class StructureAware : IStructureAware
    {
        public event OnStructureEnter StructureEnter;
        private bool _wasPlayingCustom;
        private readonly IPlayer _player;
        private CollidableStructure[] _currentNearStructures;
        private Timer _enterTimer;

        public StructureAware(IPlayer Player)
        {
            _player = Player;
            _enterTimer = new Timer(12f)
            {
                AutoReset = false
            };
            _enterTimer.MakeReady();
            NearCollisions = new CollisionGroup[0];
        }
        
        public void Update()
        {
            var collidableStructures = StructureHandler.GetNearStructures(_player.Position);

            if (this.NeedsUpdating(collidableStructures))
            {
                _currentNearStructures = collidableStructures.ToArray();
                NearCollisions = collidableStructures.SelectMany(S => S.Colliders).ToArray();               
            }

            _enterTimer.Tick();
            this.HandleSounds();
        }

        private void HandleSounds()
        {
            var none = true;
            for (var i = 0; i < _currentNearStructures.Length; i++)
            {
                var structure = _currentNearStructures[i];
                if ((structure.Position.Xz - _player.Position.Xz).LengthFast < structure.Radius * .75f)
                {
                    if (!_wasPlayingCustom && structure.Design.AmbientSongs.Length > 0)
                    {
                        var song = structure.Design.AmbientSongs[Utils.Rng.Next(0, structure.Design.AmbientSongs.Length)];
                        SoundtrackManager.PlayTrack(song, true);
                        _wasPlayingCustom = true;
                        if (_enterTimer.Ready)
                        {
                            structure.Design.OnEnter(_player);
                            StructureEnter?.Invoke(structure);
                        }
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
        
        private bool NeedsUpdating(CollidableStructure[] Structures)
        {
            if (_currentNearStructures == null || Structures.Length != _currentNearStructures.Length 
                || Structures.Sum(S => S.Colliders.Length) != NearCollisions.Length) return true;
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

        public CollisionGroup[] NearCollisions { get; private set; }
    }
}