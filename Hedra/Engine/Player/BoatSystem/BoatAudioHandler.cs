using System;
using System.Numerics;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Sound;

namespace Hedra.Engine.Player.BoatSystem
{
    public class BoatAudioHandler : IDisposable
    {
        private readonly AreaSound _areaSound;
        private readonly IHumanoid _humanoid;
        private readonly BoatStateHandler _stateHandler;

        public BoatAudioHandler(IHumanoid Humanoid, BoatStateHandler StateHandler)
        {
            _humanoid = Humanoid;
            _stateHandler = StateHandler;
            _areaSound = new AreaSound(SoundType.BoatMove, Vector3.Zero, 16f);
        }

        public void Dispose()
        {
            _areaSound.Dispose();
        }

        public void Update()
        {
            /*
            _areaSound.Position = _player.Position;
            _areaSound.Update(_stateHandler.Enabled && _stateHandler.Velocity.LengthFast() > 15);
            */
        }
    }
}