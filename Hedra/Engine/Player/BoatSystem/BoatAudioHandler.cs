using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.Player.BoatSystem
{
    public class BoatAudioHandler
    {
        private readonly BoatStateHandler _stateHandler;
        private readonly IPlayer _player;
        private readonly AreaSound _areaSound;

        public BoatAudioHandler(IPlayer Player, BoatStateHandler StateHandler)
        {
            _player = Player;
            _stateHandler = StateHandler;
            _areaSound = new AreaSound(SoundType.BoatMove, Vector3.Zero, 16f);
        }

        public void Update()
        {
            _areaSound.Position = _player.Position;
            _areaSound.Update(_stateHandler.Enabled && _stateHandler.Velocity.LengthFast > 15);
        }
    }
}
