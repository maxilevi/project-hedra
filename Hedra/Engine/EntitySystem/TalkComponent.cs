/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/09/2017
 * Time: 12:30 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Sound;
using OpenTK.Input;

namespace Hedra.Engine.EntitySystem
{
    public delegate void OnTalkEventHandler(IEntity Talkee);

    public class TalkComponent : EntityComponent, ITickable
    {
        private static uint _talkBackground;
        private static Vector2 _talkBackgroundSize;
        public float Duration { get; set; } = 8f;
        public event OnTalkEventHandler OnTalk;
        private bool _shouldTalk;                                                                                        
        public bool Talked = false;
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
        
        public TalkComponent(IEntity Parent, string Text) : base(Parent)
        {
            _phrase = Utils.FitString(Text, 25);
            EventDispatcher.RegisterKeyDown(this, delegate(Object Sender, KeyEventArgs EventArgs)
            {
                if (EventArgs.Key == Key.E && (GameManager.Player.Position - Parent.Position).Xz.LengthSquared < 24f * 24f)
                    _shouldTalk = true;
            });
        }

        public TalkComponent(IEntity Parent) : this(Parent, null)
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

        public void Talk()
        {
            this.Talk(false);
        }

        public void Talk(bool Silent)
        {
            if(!Silent) SoundManager.PlayUISound(SoundType.TalkSound, 1f, .75f);
            string phrase = _phrase ?? Phrases[Utils.Rng.Next(0, Phrases.Length)];
            phrase = Utils.FitString(phrase, 30);
            
            var textSize = new GUIText(phrase, Vector2.Zero, Color.White, FontCache.Get(AssetManager.NormalFamily, 10));

            Vector3 FollowFunc()
            {
                return Parent.Position + Vector3.UnitY * 14f;
            }
            var backBoard = new Billboard(Duration, _talkBackground, FollowFunc(), _talkBackgroundSize)
            {
                FollowFunc = FollowFunc,
                DisposeTextureId = false
            };

            _board = new Billboard(Duration, string.Empty, Color.White, FontCache.Get(AssetManager.NormalFamily, 10), FollowFunc())
            {
                FollowFunc = FollowFunc
            };
            CoroutineManager.StartCoroutine(TalkCoroutine, _board, phrase, Duration);
            OnTalk?.Invoke(this.Parent);

            textSize.Dispose();
            Talked = true;
        }

        private static IEnumerator TalkCoroutine(object[] Args)
        {
            var billboard = (Billboard) Args[0];
            var text = (string) Args[1];
            var duration = (float) Args[2];
            var textElement = billboard.Texture as GUIText;
            var iterator = 0;
            var passedTime = 0f;
            if(textElement == null) yield break;

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

        public static string[] Phrases = {
            "Have you tried selling your items in the market? They accept every kind of object.",
            "Rumor says graveyards hold great rewards.",
            "I've heard there is a travelling merchant wandering around the world. He sells gliders & special items.",
            "You seem like an adventurer. Why don't you try doing a quest?",
            "Farming is exhausting.",
            "Liked the game? Rate it on itch.io!",
            "Am I the only one who hears the sound of glass breaking?",
            "Are you an adventurer? I envy you. Farming gets tiring.",
            "The life of a farmer may be a dull one, but it's what keeps the village going!",

        };
    }
}
