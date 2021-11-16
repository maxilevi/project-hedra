using System;
using System.Linq;
using Hedra.Engine.Events;
using Hedra.Game;
using Hedra.Sound;

namespace Hedra.Engine.Player.ToolbarSystem
{
    public class ToolbarInputHandler : IDisposable
    {
        private readonly IPlayer _player;

        public ToolbarInputHandler(IPlayer Player)
        {
            _player = Player;
            EventDispatcher.RegisterKeyUp(this, HandleUp);
            EventDispatcher.RegisterKeyDown(this, HandleDown);
        }

        public void Dispose()
        {
            EventDispatcher.UnregisterKeyUp(this);
            EventDispatcher.UnregisterKeyDown(this);
        }

        private void HandleUp(object Sender, KeyEventArgs EventArgs)
        {
            if (!_player.CanInteract || _player.IsKnocked || _player.Movement.IsJumping || _player.IsDead ||
                _player.IsSwimming ||
                _player.IsUnderwater || _player.IsTravelling || _player.InterfaceOpened || GameManager.InMenu) return;

            var keyText = EventArgs.Key.ToString().ToLowerInvariant();
            if (!keyText.Contains("number")) return;

            var keyIndex = int.Parse(keyText.Substring(keyText.Length - 1, 1)) - 1;
            if (keyIndex < 0 || keyIndex > Toolbar.InteractableItems - 1) return;

            _player.Toolbar.SkillAt(keyIndex)?.KeyUp();
        }

        private void HandleDown(object Sender, KeyEventArgs EventArgs)
        {
            if (!_player.CanInteract || _player.Movement.IsJumping || _player.IsKnocked || _player.IsDead ||
                _player.IsSwimming || _player.IsRiding
                || _player.IsUnderwater || _player.IsTravelling || _player.InterfaceOpened ||
                GameManager.InMenu) return;


            var keyText = EventArgs.Key.ToString().ToLowerInvariant();
            if (keyText.Contains("number"))
            {
                var keyIndex = int.Parse(keyText.Substring(keyText.Length - 1, 1)) - 1;
                if (keyIndex < 0 || keyIndex > Toolbar.InteractableItems - 1)
                {
                    SoundPlayer.PlayUISound(SoundType.ButtonHover);
                    return;
                }

                var ability = _player.Toolbar.SkillAt(keyIndex);

                if (ability != null && ability.MeetsRequirements() && AbilitiesBeingCasted() == 0 &&
                    (!_player.IsAttacking || ability.CanBeCastedWhileAttacking))
                {
                    if (!ability.Casting)
                        SoundPlayer.PlaySound(SoundType.ButtonClick, _player.Position, false, 1f, 0.5f);
                    _player.Mana -= ability.ManaCost;
                    ability.Use();
                }
                else
                {
                    if (ability != null && ability.Casting) return;
                    SoundPlayer.PlayUISound(SoundType.ButtonHover);
                }
            }
        }

        private int AbilitiesBeingCasted()
        {
            return _player.Toolbar.Skills.Count(T => T.Casting);
        }
    }
}