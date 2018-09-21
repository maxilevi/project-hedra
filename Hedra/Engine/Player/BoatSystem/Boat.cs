namespace Hedra.Engine.Player.BoatSystem
{
    public class Boat : IBoat
    {
        private readonly IPlayer _player;
        private readonly BoatStateHandler _stateHandler;
        private readonly BoatModelHandler _modelHandler;
        
        public Boat(IPlayer Player)
        {
            _player = Player;
            _stateHandler = new BoatStateHandler(_player);
            _modelHandler = new BoatModelHandler(_player, _stateHandler);
        }

        public void Update()
        {
            _stateHandler.Update();
            _modelHandler.Update();
        }

        public bool Enabled => _stateHandler.Enabled;
    }
}