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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.UI;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using Hedra.Sound;
using OpenToolkit.Mathematics;


namespace Hedra.Components
{
    public delegate void OnTalkEventHandler(IEntity Talker);

    public class TalkComponent : Component<IHumanoid>
    {
        public event OnTalkEventHandler OnTalkingEnded;
        public event OnTalkEventHandler OnTalkingStarted;
        public bool Talking { get; private set; }
        public bool CanTalk { get; set; } = true;

        private const float CharacterThreshold = .05f;
        private const int TalkRadius = 12;
        private static uint _talkBackground;
        private static Vector2 _talkBackgroundSize;
        private readonly List<Translation> _lines;
        private readonly Animation _talkingAnimation;
        private bool _shouldTalk;
        private bool _dialogCreated;
        private TextBillboard _board;
        private Translation _thought;
        private IHumanoid _talker;
        private bool _wasAvailableToTalk;

        static TalkComponent()
        {
            Executer.ExecuteOnMainThread(delegate
            {
                _talkBackgroundSize = Graphics2D.SizeFromAssets("Assets/Skills/Dialog.png");
                _talkBackground = Graphics2D.LoadFromAssets("Assets/Skills/Dialog.png");
                TextureRegistry.MarkStatic(_talkBackground);
            });
        }

        public TalkComponent(IHumanoid Parent, Translation LiveTranslation = null) : base(Parent)
        {
            _lines = new List<Translation>();
            if(LiveTranslation != null) _lines.Add(LiveTranslation);
            _talkingAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorTalk.dae");
            _talkingAnimation.Loop = false;
            _talkingAnimation.OnAnimationEnd += Sender =>
            {
                if (!Talking) return;
                PlayTalkingAnimation();
            };
            EventDispatcher.RegisterKeyDown(this, (Sender, EventArgs) =>
            {
                if (EventArgs.Key == Controls.Interact && IsAvailableToTalk())
                    _shouldTalk = true;
            });
        }

        private bool IsAvailableToTalk()
        {
            var earlyExit = Parent.IsNear(GameManager.Player, TalkRadius)
                   && !Talking
                   && GameManager.Player.CanInteract
                   && !PlayerInterface.Showing
                   && !Parent.Model.IsMoving
                   && CanTalk;
            if (!earlyExit) return false;
            return GameManager.Player.IsLookingAt(Parent, .5f);
        }

        public void AddDialogLine(Translation Dialog)
        {
            _lines.Add(Dialog);
        }
        
        public override void Update()
        {
            var availableToTalk = IsAvailableToTalk();
            if (availableToTalk)
            {
                GameManager.Player.MessageDispatcher.ShowMessageWhile(Translations.Get("to_talk", Controls.Interact), Color.White, IsAvailableToTalk);
                Parent.Model.Tint = Vector4.One * 3f;

                if (_shouldTalk)
                {
                    this.TalkToPlayer();
                }         
            }
            if (Talking && Parent.IsNear(_talker, TalkRadius))
            {
                Parent.LookAt(_talker);
            }
            if(_wasAvailableToTalk && !availableToTalk)
            {
                Parent.Model.Tint = Vector4.One;
            }
            _wasAvailableToTalk = availableToTalk;
        }

        private Translation SelectMainThought()
        {
            if (_thought != null) return _thought;
            if (ThoughtComponent != null)
                return _thought = ThoughtComponent.Thoughts[Utils.Rng.Next(0, ThoughtComponent.Thoughts.Length)];
            return null;
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
                    () =>
                    {
                        OnTalkingEnded?.Invoke(_talker);
                        Callback();
                    }
                );
            });
        }

        private void SetupDialog()
        {
            if (_dialogCreated) return;
            _dialogCreated = true;
            
            var dialog = SelectMainThought();
            _lines.AddRange(GetBeforeLines());
            if(dialog != null) _lines.Insert(0, dialog);
            _lines.AddRange(GetAfterLines());
            
            if (_lines.Count == 0) _lines.Add(Phrases[Utils.Rng.Next(0, Phrases.Length)]);
        }

        private Translation[] GetBeforeLines()
        {
            if (ThoughtComponent != null)
                return ThoughtComponent.BeforeDialog;
            return new Translation[0];
        }
        
        private Translation[] GetAfterLines()
        {
            if (ThoughtComponent != null)
                return ThoughtComponent.AfterDialog;
            return new Translation[0];
        }

        private void TalkToPlayer()
        {
            _talker = GameManager.Player;
            OnTalkingStarted?.Invoke(_talker);
            SetupDialog();
            SoundPlayer.PlayUISound(SoundType.TalkSound, 1f, .75f);

            Vector3 FollowFunc()
            {
                return Parent.Position + Vector3.UnitY * 14f;
            }

            var lifetime = _lines.Sum(S => TextProvider.StripFormat(S.Get()).Length * CharacterThreshold * 3f);
            var backBoard = new TextureBillboard(lifetime, _talkBackground, FollowFunc, _talkBackgroundSize);

            _board = new TextBillboard(lifetime, string.Empty, Color.White,
                FontCache.GetNormal(10), FollowFunc);
            
            RoutineManager.StartRoutine(TalkRoutine, _board);

            _shouldTalk = false;
            Talking = true;
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

        private IEnumerator TalkRoutine(params object[] Args)
        {
            var billboard = (TextBillboard) Args[0];
            PlayTalkingAnimation();
            for (var i = 0; i < _lines.Count; ++i)
            {
                var routine = SingleLineRoutine(billboard, TextProvider.Wrap(_lines[i].Get(), 28));
                while (routine.MoveNext()) yield return null;
                var waitRoutine = RoutineManager.WaitForSeconds(2f);
                while (waitRoutine.MoveNext()) yield return null;
            }
            OnTalkingEnded?.Invoke(_talker);
        }
        
        private static IEnumerator SingleLineRoutine(TextBillboard Billboard, string Text)
        {
            var strippedText = TextProvider.StripFormat(Text);
            var iterator = 0;
            var passedTime = 0f;

            while (iterator < strippedText.Length + 1)
            {
                if (passedTime > CharacterThreshold)
                {
                    Billboard.UpdateText(TextProvider.Substr(Text, iterator));
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

        private static readonly Translation[] Phrases =
        {
            Translation.Default("...")
        };
        
        private ThoughtsComponent ThoughtComponent => Parent.SearchComponent<ThoughtsComponent>();
    }
}
