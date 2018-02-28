using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player
{
    public class VisualMessageDispatcher
    {
        public LocalPlayer Player;
        public const float MessageSpeed = .1f;
        public static Color DefaultColor = Color.White;
        private readonly GUIText _mainText;
        private readonly GUIText _playerText;
        private bool _messageRunningWhile;
        private float _messageSeconds;
        private Func<bool> _currentCondition;
        private bool _isRunning;
        private readonly GUIText _notificationText;
        private readonly List<MessageItem> _messageQueue;

        public VisualMessageDispatcher(LocalPlayer Player)
        {
            this.Player = Player;
            _messageQueue = new List<MessageItem>();

            _mainText = new GUIText("", new Vector2(0, .7f), Color.FromArgb(255, 39, 39, 39), FontCache.Get(AssetManager.Fonts.Families[0], 32, FontStyle.Bold));
            _playerText = new GUIText("", new Vector2(0, 0), Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 14, FontStyle.Bold));

            _notificationText = new GUIText("", new Vector2(0.7f, -0.9f), Color.FromArgb(255, 39, 39, 39), FontCache.Get(UserInterface.Fonts.Families[0], 12));
            _notificationText.UiText.Opacity = 0f;

            Player.UI.GamePanel.AddElement(_mainText);
            Player.UI.GamePanel.AddElement(_playerText);

            CoroutineManager.StartCoroutine(this.ProcessMessages);
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

                if (_messageQueue.Count == 0)
                {
                    yield return null;
                    continue;
                }

                var msg = _messageQueue[0];
                processing = true;

                Action callback = delegate{
                    processing = false;
                    _messageQueue.RemoveAt(0);
                };

                if (msg.Type == MessageType.Title)
                    this.ProcessTitleMessage(msg, callback);

                else if (msg.Type == MessageType.Notification)
                    this.ProcessNotificationMessage(msg, callback);

                else if (msg.Type == MessageType.Normal)
                    this.ProcessNormalMessage(msg, callback);

                else if (msg.Type == MessageType.While)
                    this.ProcessWhileMessage(msg, callback);


                yield return null;
            }
        }

        public void ShowTitleMessage(string Message, float Seconds)
        {
            this.ShowTitleMessage(Message, Seconds, Color.FromArgb(255, 39, 39, 39));
        }

        public void ShowTitleMessage(string Message, float Seconds, Color TextColor)
        {
            if (_messageQueue.Any(Item => Item.Content == Message.ToUpperInvariant())) return;
            var item = new MessageItem
            {
                Type = MessageType.Title,
                Content = Message.ToUpperInvariant(),
                Time = Seconds,
                Color = TextColor
            };

            _messageQueue.Add( item );
        }

        private void ProcessTitleMessage(MessageItem Item, Action Callback)
        {
            _mainText.FontColor = Item.Color;
            _mainText.Text = Item.Content;
            _mainText.UiText.Opacity = 0;

            if (Player.UI.GamePanel.Enabled)
            {
                TaskManager.Asynchronous(delegate
                {
                    _mainText.UiText.Opacity = 0.0001f;
                    float factor = MessageSpeed;
                    while (_mainText.UiText.Opacity > 0)
                    {
                        if (_mainText.UiText.Opacity >= 1f)
                        {
                            Thread.Sleep((int) (Item.Time * 1000));
                            factor = -factor;
                        }
                        _mainText.UiText.Opacity += factor;
                        _mainText.UiText.Opacity = Mathf.Clamp(_mainText.UiText.Opacity, 0, 1);
                        Thread.Sleep((int) (((factor < 0) ? -factor : factor) * 1000));
                    }
                    _mainText.UiText.Opacity = 0;
                    Callback();
                });
            }
            else
            {
                Callback();
            }
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
            _playerText.FontColor = Item.Color;
            _playerText.Text = Item.Content;
            _playerText.UiText.Opacity = 0;

            if (Player.UI.GamePanel.Enabled)
            {
                TaskManager.Asynchronous(delegate
                {
                    _playerText.UiText.Opacity = 0.0001f;
                    var factor = MessageSpeed;
                    while (_playerText.UiText.Opacity > 0)
                    {
                        if (_playerText.UiText.Opacity >= 1f)
                        {
                            Thread.Sleep((int)(Item.Time * 1000));
                            factor = -factor;
                        }
                        _playerText.UiText.Opacity += factor;
                        _playerText.UiText.Opacity = Mathf.Clamp(_playerText.UiText.Opacity, 0, 1);
                        Thread.Sleep((int)((factor < 0 ? -factor : factor) * 1000));
                    }
                    _playerText.UiText.Opacity = 0;
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
            if (_messageQueue.Any(Item => Item.Content == Message.ToUpperInvariant())) return;
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
            _playerText.FontColor = Item.Color;
            _playerText.Text = Item.Content;
            _playerText.UiText.Opacity = 0;

            if (Player.UI.GamePanel.Enabled)
            {
                TaskManager.Asynchronous(delegate
                {
                    _playerText.UiText.Opacity = 0.0001f;
                    var factor = MessageSpeed;
                    while (_playerText.UiText.Opacity > 0)
                    {
                        if (_playerText.UiText.Opacity >= 1f)
                        {
                            while (Item.Condition()) { }
                            factor = -factor;
                        }
                        _playerText.UiText.Opacity += factor;
                        _playerText.UiText.Opacity = Mathf.Clamp(_playerText.UiText.Opacity, 0, 1);
                        Thread.Sleep((int)((factor < 0 ? -factor : factor) * 1000));
                    }
                    _playerText.UiText.Opacity = 0;
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
                PlaySound = PlaySound
            };

            _messageQueue.Add(item);
        }

        private void ProcessNotificationMessage(MessageItem Item, Action Callback)
        {
            _notificationText.FontColor = Item.Color;
            _notificationText.Text = Item.Content;
            if (Item.PlaySound)
                Sound.SoundManager.PlaySound(Sound.SoundType.OnOff, Player.Position, false, 1f, 1.05f);

            TaskManager.Asynchronous(delegate
            {
                _notificationText.UiText.Opacity = 0.0001f;
                var factor = MessageSpeed;
                while (_notificationText.UiText.Opacity > 0)
                {
                    if (_notificationText.UiText.Opacity >= 1f)
                    {
                        Thread.Sleep((int)(Item.Time * 1000));
                        factor = -factor;
                    }
                    _notificationText.UiText.Opacity += factor;
                    _notificationText.UiText.Opacity = Mathf.Clamp(_notificationText.UiText.Opacity, 0, 1);
                    Thread.Sleep((int)((factor < 0 ? -factor : factor) * 400));
                }
                _notificationText.UiText.Opacity = 0;
                Callback();
            });         
        }
    }

    internal class MessageItem
    {
        public MessageType Type;
        public string Content;
        public Color Color;
        public float Time;
        public bool PlaySound;
        public Func<bool> Condition;
    }

    internal enum MessageType{
        Title,
        Notification,
        Normal,
        While
    }
}
