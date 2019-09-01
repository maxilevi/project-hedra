using System;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.Player.BoatSystem
{
    public class BoatAudioHandler : IDisposable
    {
        private readonly BoatStateHandler _stateHandler;
        private readonly IHumanoid _humanoid;
        private readonly AreaSound _areaSound;

        public BoatAudioHandler(IHumanoid Humanoid, BoatStateHandler StateHandler)
        {
            _humanoid = Humanoid;
            _stateHandler = StateHandler;
            _areaSound = new AreaSound(SoundType.BoatMove, Vector3.Zero, 16f);
        }

        public void Update()
        {
            /*
            _areaSound.Position = _player.Position;
            _areaSound.Update(_stateHandler.Enabled && _stateHandler.Velocity.LengthFast > 15);
            */
        }

        public void Dispose()
        {
            _areaSound.Dispose();
        }
    }
}
