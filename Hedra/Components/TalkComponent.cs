/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/09/2017
 * Time: 12:30 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Drawing;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.UI;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Components
{
    public delegate void OnTalkEventHandler(IEntity Talkee);

    public class TalkComponent : Component<IHumanoid>, ITickable
    {
        private const int TalkRadius = 12;
        private static uint _talkBackground;
        private static Vector2 _talkBackgroundSize;
        private readonly Animation _talkingAnimation;
        public event OnTalkEventHandler OnTalk;
        private bool _shouldTalk;                                                                                        
        public bool Talking { get; private set; }
        private TextBillboard _board;
        private readonly string _phrase;
        private string _thought;
        private IHumanoid _talker;

        static TalkComponent()
        {
            Executer.ExecuteOnMainThread(delegate
            {
                _talkBackgroundSize = Graphics2D.SizeFromAssets("Assets/Skills/Dialog.png");
                _talkBackground = Graphics2D.LoadFromAssets("Assets/Skills/Dialog.png");
            });
        }
        
        public TalkComponent(IHumanoid Parent, string Text = null) : base(Parent)
        {
            _phrase = Utils.FitString(Text, 25);
            _talkingAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorTalk.dae");
            _talkingAnimation.Loop = false;
            _talkingAnimation.OnAnimationEnd += Sender =>
            {
                if (!Talking) return;
                PlayTalkingAnimation();
            };
            EventDispatcher.RegisterKeyDown(this, (Sender, EventArgs) =>
            {
                if (EventArgs.Key == Controls.Interact && Parent.IsNear(GameManager.Player, TalkRadius) && !Talking)
                    _shouldTalk = true;
            });
        }

        public override void Update()
        {
            bool CanTalk() => 
                (GameManager.Player.Position - Parent.Position).Xz.LengthSquared < TalkRadius * TalkRadius 
                && !Talking && !PlayerInterface.Showing && !Parent.Model.IsMoving;
            if (CanTalk())
            {
                GameManager.Player.MessageDispatcher.ShowMessageWhile(Translations.Get("to_talk", Controls.Interact), Color.White, CanTalk);

                if (_shouldTalk)
                {
                    this.TalkToPlayer();
                }         
            }
            if (Talking && Parent.IsNear(_talker, TalkRadius))
            {
                Parent.RotateTowards(_talker);
            }
        }

        private string SelectThought()
        {
            if (_thought != null) return _thought;
            var thoughtComponent = Parent.SearchComponent<ThoughtsComponent>();
            if (thoughtComponent != null)
                return _thought = thoughtComponent.Thoughts[Utils.Rng.Next(0, thoughtComponent.Thoughts.Length)].Get();
            return _phrase ?? Phrases[Utils.Rng.Next(0, Phrases.Length)];
        }

        public void Simulate(IHumanoid To, float Seconds, Action Callback)
        {
            Talking = true;
            _talker = To;
            PlayTalkingAnimation();
            TaskScheduler.After(Seconds, delegate
            {
                Talking = false;
                _talker = null;
                TaskScheduler.When(
                    () => Parent.Model.AnimationBlending != _talkingAnimation
                    && Parent.Model.AnimationPlaying != _talkingAnimation,
                    Callback
                );
            });
        }

        private void TalkToPlayer()
        {
            SoundPlayer.PlayUISound(SoundType.TalkSound, 1f, .75f);
            var phrase = SelectThought();
            phrase = Utils.FitString(phrase, 30);
            
            var textSize = new GUIText(phrase, Vector2.Zero, Color.White, FontCache.Get(AssetManager.NormalFamily, 10));

            Vector3 FollowFunc()
            {
                return Parent.Position + Vector3.UnitY * 14f;
            }

            var lifetime = 8;
            var backBoard = new TextureBillboard(lifetime, _talkBackground, FollowFunc, _talkBackgroundSize)
            {
                ShouldDisposeId = false
            };

            _board = new TextBillboard(lifetime, string.Empty, Color.White,
                FontCache.Get(AssetManager.NormalFamily, 10), FollowFunc);
            CoroutineManager.StartCoroutine(TalkCoroutine, _board, phrase, lifetime);
            OnTalk?.Invoke(Parent);

            textSize.Dispose();
            _shouldTalk = false;
            Talking = true;
            _talker = GameManager.Player;
            TaskScheduler.When(
                () => backBoard.Disposed,
                delegate
                {
                    _talker = null;
                    Talking = false;
                }
            );
        }

        private void PlayTalkingAnimation()
        {
            Parent.Model.BlendAnimation(_talkingAnimation);
        }

        private IEnumerator TalkCoroutine(object[] Args)
        {
            var billboard = (TextBillboard) Args[0];
            var text = (string) Args[1];
            var iterator = 0;
            var passedTime = 0f;
            PlayTalkingAnimation();

            while (iterator < text.Length+1)
            {
                if (passedTime > .05f)
                {
                    billboard.UpdateText(text.Substring(0, iterator));
                    iterator++;
                    passedTime = 0;
                }
                passedTime += Time.DeltaTime;
                yield return null;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            EventDispatcher.UnregisterKeyDown(this);
        }

        private static readonly string[] Phrases =
        {
            "..."
        };
    }
}
