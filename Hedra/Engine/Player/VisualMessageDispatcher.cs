using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.Player
{
    public class VisualMessageDispatcher : IMessageDispatcher
    {
        private const float FadeSpeed = 2f;
        private readonly LocalPlayer _player;
        private static readonly Color DefaultColor = Color.White;
        private const float MessageSpeed = .1f;
        private readonly GUIText _mainText;
        private readonly GUIText _playerText;
        private bool _messageRunningWhile;
        private float _messageSeconds;
        private Func<bool> _currentCondition;
        private bool _isRunning;
        private readonly GUIText _notificationText;
        private readonly Plaque _plaqueText;
        private readonly List<MessageItem> _messageQueue;

        public VisualMessageDispatcher(LocalPlayer Player)
        {
            this._player = Player;
            _messageQueue = new List<MessageItem>();

            _mainText = new GUIText(string.Empty, new Vector2(0, .7f), Color.White, FontCache.Get(AssetManager.BoldFamily, 32, FontStyle.Bold));
            _mainText.Stroke = true;
            _playerText = new GUIText(string.Empty, new Vector2(0, 0), Color.White, FontCache.Get(AssetManager.BoldFamily, 13, FontStyle.Bold));

            _notificationText = new GUIText(string.Empty, new Vector2(0.7f, -0.8f), Color.FromArgb(255, 39, 39, 39),
                FontCache.Get(AssetManager.NormalFamily, 14))
            {
                UIText =
                {
                    Opacity = 0f
                }
            };

            _plaqueText = new Plaque(Vector2.UnitY * -.55f + Vector2.UnitX * .75f);
            _plaqueText.Disable();

            Player.UI.GamePanel.AddElement(_mainText);
            Player.UI.GamePanel.AddElement(_playerText);

            RoutineManager.StartRoutine(this.ProcessMessages);
        }

        private IEnumerator ProcessMessages()
        {
            int prevSeed = World.Seed;
            var processing = false;

            while (Program.GameWindow.Exists)
            {
                if (processing)
                {
                    yield return null;
                    continue;
                }

                if (World.Seed != prevSeed)
                {
                    _messageQueue.Clear();
                    prevSeed = World.Seed;
                }

                if (_messageQueue.Count == 0 || GameManager.IsLoading)
                {
                    yield return null;
                    continue;
                }

                var msg = _messageQueue[0];
                processing = true;

                void Callback()
                {
                    processing = false;
                    _messageQueue.RemoveAt(0);
                }

                switch (msg.Type)
                {
                    case MessageType.Title:
                        ProcessTitleMessage(msg, Callback);
                        break;
                    case MessageType.Notification:
                        ProcessNotificationMessage(msg, Callback);
                        break;
                    case MessageType.Normal:
                        ProcessNormalMessage(msg, Callback);
                        break;
                    case MessageType.While:
                        ProcessWhileMessage(msg, Callback);
                        break;
                    case MessageType.Plaque:
                        ProcessPlaqueMessage(msg, Callback);
                        break;
                }

                yield return null;
            }
        }

        public void ShowPlaque(string Message, float Seconds, bool PlaySound = true)
        {
            if (_messageQueue.Any(Item => Item.Content == Message.ToUpperInvariant())) return;
            var item = new MessageItem
            {
                Type = MessageType.Plaque,
                Content = Message.ToUpperInvariant(),
                Time = Seconds,
                PlaySound = PlaySound,
                UIObject = _plaqueText
            };
            _messageQueue.Add(item);
        }
 
        private void ProcessPlaqueMessage(MessageItem Item, Action Callback)
        {
            _plaqueText.Text = Item.Content;
            _plaqueText.Opacity = 0;
            _plaqueText.Enable();
            if (Item.PlaySound)
                SoundPlayer.PlaySound(SoundType.NotificationSound, _player.Position);
            
            FadeAndShow(Item, Callback);
        }

        public bool HasTitleMessages => _messageQueue.Any(M => M.Type == MessageType.Title);
        
        public void ShowTitleMessage(string Message, float Seconds)
        {
            this.ShowTitleMessage(Message, Seconds, Color.White);
        }

        private void ShowTitleMessage(string Message, float Seconds, Color TextColor)
        {
            if (_messageQueue.Any(Item => Item.Content == Message.ToUpperInvariant())) return;
            var item = new MessageItem
            {
                Type = MessageType.Title,
                Content = Message.ToUpperInvariant(),
                Time = Seconds,
                Color = TextColor,
                UIObject = _mainText
            };

            _messageQueue.Add( item );
        }

        private void ProcessTitleMessage(MessageItem Item, Action Callback)
        {
            _mainText.TextColor = Item.Color;
            _mainText.Text = Item.Content;
            _mainText.UIText.Opacity = 0;

            FadeAndShow(Item, Callback);
        }

        public void ShowMessage(string Message, float Seconds)
        {
            this.ShowMessage(Message, Seconds, DefaultColor);
        }

        public void ShowMessage(string Message, float Seconds, Color TextColor)
        {
            if(_messageQueue.Any( Item => Item.Content == Message.ToUpperInvariant() )) return;
            var item = new MessageItem
            {
                Type = MessageType.Normal,
                Content = Message.ToUpperInvariant(),
                Time = Seconds,
                Color = TextColor
            };

            _messageQueue.Add(item);
        }

        private void ProcessNormalMessage(MessageItem Item, Action Callback)
        {
            _playerText.TextColor = Item.Color;
            _playerText.Text = Item.Content;
            
            if (_player.UI.GamePanel.Enabled)
            {
                TaskScheduler.Asynchronous(delegate
                {
                    _playerText.UIText.Opacity = 1;
                    Thread.Sleep( (int) (Item.Time * 1000) );
                    _playerText.UIText.Opacity = 0;
                    Callback();
                });
            }
            else
            {
                Callback();
            }
        }

        public void ShowMessageWhile(string Message, Func<bool> Condition)
        {
            this.ShowMessageWhile(Message, Color.White, Condition);
        }

        public void ShowMessageWhile(string Message, Color TextColor, Func<bool> Condition)
        {
            if(!Condition()) return;
            try
            {
                if (_messageQueue.Any(Item => Item.Content == Message.ToUpperInvariant())) return;
            }
            catch (InvalidOperationException e)
            {
                Log.WriteLine(e.Message);
                return;
            }
            var item = new MessageItem
            {
                Type = MessageType.While,
                Content = Message.ToUpperInvariant(),
                Color = TextColor,
                Condition = Condition
            };

            _messageQueue.Add(item);
        }

        private void ProcessWhileMessage(MessageItem Item, Action Callback)
        {
            _playerText.TextColor = Item.Color;
            _playerText.Text = Item.Content;

            if (_player.UI.GamePanel.Enabled)
            {
                TaskScheduler.Asynchronous(delegate
                {
                    _playerText.UIText.Opacity = 1;
                    while (Item.Condition())
                    {
                        Thread.Sleep(5);
                    }
                    Thread.Sleep((int)(Item.Time * 1000));           
                    _playerText.UIText.Opacity = 0;
                    Callback();
                });
            }
            else
            {
                Callback();
            }
        }

        public void ShowNotification(string Message, Color FontColor, float Seconds)
        {
            this.ShowNotification(Message, FontColor, Seconds, true);
        }

        public void ShowNotification(string Message, Color FontColor, float Seconds, bool PlaySound)
        {
            if (_messageQueue.Any(Item => Item.Content == Message.ToUpperInvariant())) return;
            var item = new MessageItem
            {
                Type = MessageType.Notification,
                Content = Message.ToUpperInvariant(),
                Time = Seconds,
                Color = FontColor,
                PlaySound = PlaySound,
                UIObject = _notificationText
            };
            _messageQueue.Add(item);
        }

        private void ProcessNotificationMessage(MessageItem Item, Action Callback)
        {
            _notificationText.TextColor = Item.Color;
            _notificationText.Text = Item.Content;
            _notificationText.Enable();
            if (Item.PlaySound)
                SoundPlayer.PlayUISound(SoundType.ButtonHover);
            FadeAndShow(Item, Callback);
        }
        
        private void FadeAndShow(MessageItem Item, Action Callback)
        {
            if (_player.UI.GamePanel.Enabled || _player.UI.InMenu)
            {
                RoutineManager.StartRoutine(
                    FadeOverTimeCoroutine,
                    Item.UIObject,
                    Item.Time,
                    Callback
                );
            }
            else
            {
                Callback();
            }
        }
        
        private IEnumerator FadeOverTimeCoroutine(object[] Params)
        {
            var time = 0f;
            var element = (ITransparent) Params[0];
            var seconds = (float) Params[1];
            var callback = (Action) Params[2];
            while (element.Opacity < 1 && (_player.UI.GamePanel.Enabled || _player.UI.InMenu))
            {
                element.Opacity += Time.IndependantDeltaTime * FadeSpeed;
                yield return null;
            }
            while (time < seconds && (_player.UI.GamePanel.Enabled || _player.UI.InMenu))
            {
                time += Time.IndependantDeltaTime;
                yield return null;
            }
            while (element.Opacity > 0 && (_player.UI.GamePanel.Enabled || _player.UI.InMenu))
            {
                element.Opacity -= Time.IndependantDeltaTime * FadeSpeed;
                yield return null;
            }

            element.Opacity = 0;
            callback();
        }
    }

    public class MessageItem
    {
        public MessageType Type;
        public ITransparent UIObject;
        public string Content;
        public Color Color;
        public float Time;
        public bool PlaySound;
        public Func<bool> Condition;
    }

    public enum MessageType{
        Title,
        Notification,
        Normal,
        While,
        Plaque
    }
}
