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
    public class StructureAware : IStructureAware
    {
        private bool _wasPlayingAmbient;
        private readonly IPlayer _player;
        private CollidableStructure[] _currentNearStructures;
        private Timer _villageMessageTimer;

        public StructureAware(IPlayer Player)
        {
            _player = Player;
            _villageMessageTimer = new Timer(12f);
            NearCollisions = new ICollidable[0];
        }
        
        public void Update()
        {           
            var collidableStructures = (from item in World.StructureGenerator.Structures
                where (item.Position.Xz - _player.Position.Xz).LengthSquared < item.Radius * item.Radius
                select item).ToArray();

            if (this.NeedsUpdating(collidableStructures))
            {
                _currentNearStructures = collidableStructures.ToArray();
                NearCollisions = collidableStructures.SelectMany(S => S.Colliders).ToArray();               
            }

            _villageMessageTimer.Tick();
            this.HandleSounds();
        }

        private void HandleSounds()
        {
            var none = true;
            for (var i = 0; i < _currentNearStructures.Length; i++)
            {
                if ((_currentNearStructures[i].Position.Xz - _player.Position.Xz).LengthFast <
                    _currentNearStructures[i].Radius * .75f && _currentNearStructures[i].Design is VillageDesign)
                {
                    if (!_wasPlayingAmbient)
                    {
                        SoundtrackManager.PlayTrack(SoundtrackManager.VillageIndex, true);
                        _wasPlayingAmbient = true;
                        if (_villageMessageTimer.Ready)
                            _player.MessageDispatcher.ShowTitleMessage($"WELCOME TO {VillageDesign.CreateName(World.Seed)}", 6f);
                    }
                    none = false;
                }
            }
            if (_wasPlayingAmbient && none)
            {
                _wasPlayingAmbient = false;
                SoundtrackManager.PlayTrack(SoundtrackManager.LoopableSongsStart);
            }
        }
        
        private bool NeedsUpdating(CollidableStructure[] Structures)
        {
            if (_currentNearStructures == null || Structures.Length != _currentNearStructures.Length) return true;
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

        public ICollidable[] NearCollisions { get; private set; }
    }
}