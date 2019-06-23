using System;
using System.Collections.Generic;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Game;
using OpenTK.Input;

namespace Hedra.Engine.Rendering.UI
{
    public delegate void OnPlayerInterfaceStateChangeEventHandler(bool Show);

    public abstract class PlayerInterface : IDisposable
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

        protected abstract bool HasExitAnimation { get; }

        protected void Invoke(bool Parameter)
        {
            OnPlayerInterfaceStateChange?.Invoke(Parameter);
        }

        private static void OnKeyDown(object Sender, KeyEventArgs Args)
        {
            switch (Args.Key)
            {
                case Key.Escape:
                    OnEscapeDown(Args);
                    break;
                default:
                    ManageOpened(Args);
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
                    Interfaces[i].MarkAsShown();
                }
                else if(_openedInterface == Interfaces[i] && Interfaces[i].Show)
                {
                    Close(Interfaces[i]);
                }
            }
        }

        protected void MarkAsShown()
        {
            Show = true;
            if(Show) _openedInterface = this;
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
                    if (!State)
                    {
                        Interface.OnPlayerInterfaceStateChange -= DelayedAction;
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

        public static void CloseCurrent()
        {
            _openedInterface.Show = false;
            _openedInterface = null;
        }

        public void Dispose()
        {
            EventDispatcher.UnregisterKeyDown(typeof(EventDispatcher));
        }
    }
}
