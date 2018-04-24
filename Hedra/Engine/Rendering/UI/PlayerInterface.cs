using System.Collections.Generic;
using Hedra.Engine.Events;
using OpenTK.Input;

namespace Hedra.Engine.Rendering.UI
{
    public abstract class PlayerInterface
    {
        private static readonly List<PlayerInterface> Interfaces;
        private static PlayerInterface _openedInterface;
        public abstract Key OpeningKey { get; }
        public abstract bool Show { get; set; }

        protected PlayerInterface()
        {
            Interfaces.Add(this);
        }

        static PlayerInterface()
        {
            Interfaces = new List<PlayerInterface>();
            EventDispatcher.RegisterKeyDown(typeof(EventDispatcher), OnKeyDown);
        }

        private static void OnKeyDown(object Sender, KeyboardKeyEventArgs Args)
        {
            switch (Args.Key)
            {
                case Key.Escape:
                    PlayerInterface.OnEscapeDown(Args);
                    break;
                default:
                    PlayerInterface.ManageOpened(Args);
                    break;
            }
        }

        private static void ManageOpened(KeyboardKeyEventArgs Args)
        {
            for (var i = 0; i < Interfaces.Count; i++)
            {
                if (Interfaces[i].OpeningKey != Args.Key || GameSettings.Paused || GameManager.Player.IsDead ||
                    !GameManager.Player.CanInteract) continue;
                if (_openedInterface == null)
                {
                    Interfaces[i].Show = true;
                    _openedInterface = Interfaces[i];
                }
                else if(_openedInterface == Interfaces[i])
                {
                    Interfaces[i].Show = false;
                    _openedInterface = null;
                }
            }
        }

        private static void OnEscapeDown(KeyboardKeyEventArgs Args)
        {
            if (Args.Key != Key.Escape || GameManager.InStartMenu) return;

            if (_openedInterface != null)
            {
                _openedInterface.Show = false;
                _openedInterface = null;
            }
            else
            {

                if (!GameManager.Player.UI.Menu.Enabled) GameManager.Player.UI.ShowMenu();
                else GameManager.Player.UI.HideMenu();
            }
        }
    }
}
