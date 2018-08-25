using System.Collections.Generic;
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
        
        public StructureAware(IPlayer Player)
        {
            _player = Player;
        }
        
        public void Update()
        {           
            var collidableStructures = (from item in World.StructureGenerator.Structures
                where (item.Position.Xz - _player.Position.Xz).LengthSquared < item.Radius * item.Radius
                select item).ToArray();
          

            var nearCollidableStructure = collidableStructures.FirstOrDefault();

            if (nearCollidableStructure != null)
            {

                if ((nearCollidableStructure.Position.Xz - _player.Position.Xz).LengthFast <
                    nearCollidableStructure.Radius && nearCollidableStructure.Design is VillageDesign)
                {
                    SoundtrackManager.PlayTrack(SoundtrackManager.VillageIndex, true);
                    _wasPlayingAmbient = true;
                }

                NearCollisions = nearCollidableStructure.Colliders;
            }
            else if (_wasPlayingAmbient)
            {
                _wasPlayingAmbient = false;
                SoundtrackManager.PlayTrack(SoundtrackManager.LoopableSongsStart);
            }
            else
            {
                this.NearCollisions = null;
            }
        }
        
        public ICollidable[] NearCollisions { get; private set; }
    }
}