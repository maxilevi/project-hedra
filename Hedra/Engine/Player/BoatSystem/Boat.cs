using System;
using Hedra.EntitySystem;

namespace Hedra.Engine.Player.BoatSystem
{
    public class Boat : IBoat, IDisposable
    {
        private readonly BoatAudioHandler _audioHandler;
        private readonly IHumanoid _human;
        private readonly BoatModelHandler _modelHandler;
        private readonly BoatStateHandler _stateHandler;


        public Boat(IHumanoid Humanoid)
        {
            _human = Humanoid;
            _stateHandler = new BoatStateHandler(_human);
            _modelHandler = new BoatModelHandler(_human, _stateHandler);
            _audioHandler = new BoatAudioHandler(_human, _stateHandler);
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

        public void Dispose()
        {
            _modelHandler.Dispose();
            _audioHandler.Dispose();
        }
    }
}