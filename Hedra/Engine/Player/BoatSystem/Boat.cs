namespace Hedra.Engine.Player.BoatSystem
{
    public class Boat : IBoat
    {
        private readonly IPlayer _player;
        private readonly BoatStateHandler _stateHandler;
        private readonly BoatModelHandler _modelHandler;
        private readonly BoatAudioHandler _audioHandler;

        
        public Boat(IPlayer Player)
        {
            _player = Player;
            _stateHandler = new BoatStateHandler(_player);
            _modelHandler = new BoatModelHandler(_player, _stateHandler);
            _audioHandler = new BoatAudioHandler(_player, _stateHandler);
        }

        public void Update()
        {
            _stateHandler.Update();
            _modelHandler.Update();
            _audioHandler.Update();
        }

        public bool CanEnable => _stateHandler.CanEnable;

        public void Enable()
        {
            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
        }
        
        public bool Enabled
        {
            get => _stateHandler.Enabled;
            private set => _stateHandler.Enabled = value;
        }
    }
}