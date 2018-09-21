using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.BoatSystem
{
    public class BoatInputHandler
    {
        private readonly BoatStateHandler _stateHandler;
        private readonly IPlayer _player;

        public BoatInputHandler(IPlayer Player, BoatStateHandler StateHandler)
        {
            _player = Player;
            _stateHandler = StateHandler;
            EventDispatcher.RegisterKeyDown(this, delegate(object Sender, KeyEventArgs Args)
            {
                if (Key.B == Args.Key)
                {
                    if(!Enabled && _stateHandler.CanEnable())
                        Enabled = true;
                    else if (Enabled)
                        Enabled = false;
                }
            });
        }
        
        public void Update()
        {
            if(!Enabled) return;

            _player.Movement.ProcessMovement(_player.FacingDirection.Y, _player.Movement.MoveFormula(_player.View.Forward) * 1.25f);

            if (GameManager.Keyboard[Key.A])
            {

            }
            if (GameManager.Keyboard[Key.D])
            {

            }
        }
        
        public bool Enabled { get; set; }
    }
}