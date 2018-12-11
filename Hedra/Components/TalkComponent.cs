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
        private static uint _talkBackground;
        private static Vector2 _talkBackgroundSize;
        private Animation _talkingAnimation;
        public event OnTalkEventHandler OnTalk;
        private bool _shouldTalk;                                                                                        
        public bool Talked;
        private bool _isTalking;
        private Billboard _board;
        private readonly string _phrase;

        static TalkComponent()
        {
            Executer.ExecuteOnMainThread(delegate
            {
                _talkBackgroundSize = Graphics2D.SizeFromAssets("Assets/Skills/Dialog.png");
                _talkBackground = Graphics2D.LoadFromAssets("Assets/Skills/Dialog.png");
            });
        }
        
        public TalkComponent(IHumanoid Parent, string Text) : base(Parent)
        {
            _phrase = Utils.FitString(Text, 25);
            _talkingAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorTalk.dae");
            _talkingAnimation.Loop = false;
            _talkingAnimation.OnAnimationEnd += Sender =>
            {
                if (!_isTalking) return;
                PlayTalkingAnimation();
            };
            EventDispatcher.RegisterKeyDown(this, (Sender, EventArgs) =>
            {
                if (EventArgs.Key == Key.E && (GameManager.Player.Position - Parent.Position).Xz.LengthSquared < 24f * 24f)
                    _shouldTalk = true;
            });
        }

        public TalkComponent(IHumanoid Parent) : this(Parent, null)
        {           
        }

        public override void Update()
        {
            if (GameManager.Player.CanInteract && !GameManager.Player.IsDead && !GameSettings.Paused && !Talked && !PlayerInterface.Showing)
            {
                GameManager.Player.MessageDispatcher.ShowMessageWhile("[E] TO TALK", Color.White,
                    () => (GameManager.Player.Position - Parent.Position).Xz.LengthSquared < 24f * 24f && !Talked);

                if (_shouldTalk)
                {
                    this.Talk();
                }         
            }
            if (Talked && (GameManager.Player.Position - Parent.Position).Xz.LengthSquared < 24f * 24f)
            {
                Physics.LookAt(this.Parent, GameManager.Player);
            }
        }

        private string SelectThought()
        {
            var thoughtComponent = Parent.SearchComponent<ThoughtsComponent>();
            if (thoughtComponent != null)
                return thoughtComponent.Thoughts[Utils.Rng.Next(0, thoughtComponent.Thoughts.Length)].Get();
            return _phrase ?? Phrases[Utils.Rng.Next(0, Phrases.Length)];
        }

        public void Talk(bool Silent = false)
        {
            if(!Silent) SoundPlayer.PlayUISound(SoundType.TalkSound, 1f, .75f);
            var phrase = SelectThought();
            phrase = Utils.FitString(phrase, 30);
            
            var textSize = new GUIText(phrase, Vector2.Zero, Color.White, FontCache.Get(AssetManager.NormalFamily, 10));

            Vector3 FollowFunc()
            {
                return Parent.Position + Vector3.UnitY * 14f;
            }

            var lifetime = 8;
            var backBoard = new Billboard(lifetime, _talkBackground, FollowFunc(), _talkBackgroundSize)
            {
                FollowFunc = FollowFunc,
                DisposeTextureId = false
            };

            _board = new Billboard(lifetime, string.Empty, Color.White, FontCache.Get(AssetManager.NormalFamily, 10), FollowFunc())
            {
                FollowFunc = FollowFunc
            };
            CoroutineManager.StartCoroutine(TalkCoroutine, _board, phrase, lifetime);
            OnTalk?.Invoke(this.Parent);

            textSize.Dispose();
            Talked = true;
        }

        private void PlayTalkingAnimation()
        {
            Parent.Model.Play(_talkingAnimation);
        }

        private IEnumerator TalkCoroutine(object[] Args)
        {
            var billboard = (Billboard) Args[0];
            var text = (string) Args[1];
            var textElement = billboard.Texture as GUIText;
            var iterator = 0;
            var passedTime = 0f;
            _isTalking = true;
            PlayTalkingAnimation();
            if (textElement == null) yield break;

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
            _isTalking = false;
        }

        public override void Dispose()
        {
            base.Dispose();
            EventDispatcher.UnregisterKeyDown(this);
        }

        private static string[] Phrases =
        {
            "..."
        };
    }
}
