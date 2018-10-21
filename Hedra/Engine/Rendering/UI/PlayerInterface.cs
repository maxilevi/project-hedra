using System.Collections.Generic;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using OpenTK.Input;

namespace Hedra.Engine.Rendering.UI
{
    public delegate void OnPlayerInterfaceStateChangeEventHandler(bool Show);

    public abstract class PlayerInterface
    {
        private static readonly List<PlayerInterface> Interfaces;
        private static PlayerInterface _openedInterface;
        public event OnPlayerInterfaceStateChangeEventHandler OnPlayerInterfaceStateChange;
        public static bool Showing => _openedInterface != null;
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

        public bool HasExitAnimation => OnPlayerInterfaceStateChange?.GetInvocationList().Length > 0;

        protected void Invoke(bool Parameter)
        {
            OnPlayerInterfaceStateChange?.Invoke(Parameter);
        }

        private static void OnKeyDown(object Sender, KeyEventArgs Args)
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

        private static void ManageOpened(KeyEventArgs Args)
        {
            for (var i = 0; i < Interfaces.Count; i++)
            {
                if (Interfaces[i].OpeningKey != Args.Key || GameSettings.Paused || GameManager.Player.IsDead ||
                    !GameManager.Player.CanInteract || GameManager.IsLoading) continue;
                if (_openedInterface == null)
                {
                    Interfaces[i].Show = true;
                    if(Interfaces[i].Show) _openedInterface = Interfaces[i];
                }
                else if(_openedInterface == Interfaces[i] && Interfaces[i].Show)
                {
                    Close(Interfaces[i]);
                }
            }
        }

        private static void OnEscapeDown(KeyEventArgs Args)
        {
            if (Args.Key != Key.Escape || GameManager.InStartMenu) return;

            if (_openedInterface != null)
            {
                Close(_openedInterface);
            }
            else
            {
                if (!GameManager.Player.UI.Menu.Enabled) GameManager.Player.UI.ShowMenu();
                else GameManager.Player.UI.HideMenu();
            }
        }

        private static void Close(PlayerInterface Interface)
        {
            if (Interface.HasExitAnimation)
            {
                Interface.Show = false;

                void DelayedAction(bool State)
                {
                    Interface.OnPlayerInterfaceStateChange -= DelayedAction;
                    if (!State)
                    {
                        _openedInterface = null;
                    }
                }
                Interface.OnPlayerInterfaceStateChange += DelayedAction;
            }
            else
            {
                Interface.Show = false;
                _openedInterface = null;
            }
        }
    }
}
