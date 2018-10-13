using System.Linq;
using Hedra.Engine.Events;
using Hedra.Engine.Sound;
using OpenTK.Input;

namespace Hedra.Engine.Player.ToolbarSystem
{
    public class ToolbarInputHandler
    {
        private readonly IPlayer _player;

        public ToolbarInputHandler(IPlayer Player)
        {
            _player = Player;
            EventDispatcher.RegisterKeyUp(this, this.HandleUp);
            EventDispatcher.RegisterKeyDown(this, this.HandleDown);
        }
        public void HandleUp(object Sender, KeyEventArgs EventArgs)
        {
            if (!_player.CanInteract || _player.IsKnocked || _player.Movement.IsJumping || _player.IsDead || _player.IsSwimming ||
                _player.IsUnderwater || _player.IsTravelling || _player.Inventory.Show || _player.AbilityTree.Show || GameSettings.Paused) return;

            var keyText = EventArgs.Key.ToString().ToLowerInvariant();
            if (!keyText.Contains("number")) return;

            int keyIndex = int.Parse(keyText.Substring(keyText.Length - 1, 1)) - 1;
            if (keyIndex < 0 || keyIndex > Toolbar.InteractableItems-1) return;

            _player.Toolbar.SkillAt(keyIndex)?.KeyUp();          
        }

        public void HandleDown(object Sender, KeyEventArgs EventArgs)
        {
            if (!_player.CanInteract || _player.Movement.IsJumping || _player.IsKnocked || _player.IsDead || _player.IsSwimming || _player.IsAttacking || _player.IsRiding
                || _player.IsUnderwater || _player.IsTravelling || _player.Inventory.Show || _player.AbilityTree.Show || _player.Trade.Show || GameSettings.Paused) return;


            var keyText = EventArgs.Key.ToString().ToLowerInvariant();
            if (keyText.Contains("number")) {

                int keyIndex = int.Parse(keyText.Substring(keyText.Length - 1, 1)) - 1;
                if (keyIndex < 0 || keyIndex > Toolbar.InteractableItems - 1)
                {
                    SoundManager.PlayUISound(SoundType.ButtonHover);
                    return;
                }

                var ability = _player.Toolbar.SkillAt(keyIndex);

                if (ability != null && ability.MeetsRequirements() && this.AbilitiesBeingCasted() == 0)
                {
                    SoundManager.PlaySound(SoundType.ButtonClick, _player.Position, false, 1f, 0.5f);

                    ability.Cooldown = ability.MaxCooldown;
                    _player.Mana -= ability.ManaCost;
                    ability.Use();
                }
                else
                {
                    SoundManager.PlayUISound(SoundType.ButtonHover);
                }
            }
        }

        private int AbilitiesBeingCasted()
        {
            return _player.Toolbar.Skills.Count(T => T.Casting);
        }
    }
}
