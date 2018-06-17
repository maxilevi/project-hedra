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
using System.Drawing;
using System.Runtime.CompilerServices;
using Hedra.Engine.Events;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Sound;
using OpenTK.Input;

namespace Hedra.Engine.EntitySystem
{
    public delegate void OnTalkEventHandler(Entity Talkee);

	public class TalkComponent : EntityComponent, ITickable
	{
	    public float Duration { get; set; } = 8f;
        public event OnTalkEventHandler OnTalk;
	    private bool _shouldTalk;

        public static string[] Phrases = new string[]{
            "Have you tried selling your items" +
			Environment.NewLine+"in the market?"+Environment.NewLine
			+"They accept every kind of object.",
			
			"Rumor says graveyards hold great"+
			Environment.NewLine+"rewards.",
		
			"I've heard there is a travelling merchant"+
			Environment.NewLine+"wandering around the world."+Environment.NewLine
			+"He sells gliders & special items.",
		
			"You seem like an adventurer. Why don't you"+
			Environment.NewLine+"try doing a quest?",
		
			"Farming is exhausting.",
										
			"Liked the game? Rate it on itch.io!",

            "Am I the only one who hears the sound of glass breaking?",

            "Are you an adventurer? I envy you. Farming gets tiring.",

            "The life of a farmer may be a dull one, but it's what keeps the village going!",

        };
																							
		public bool Talked = false;
		private Billboard _board;
	    private readonly string _phrase;
		
		public TalkComponent(Entity Parent, string Text) : base(Parent)
		{
		    _phrase = Utils.FitString(Text, 25);
		    EventDispatcher.RegisterKeyDown(this, delegate(Object Sender, KeyEventArgs EventArgs)
		    {
		        if (EventArgs.Key == Key.E && (GameManager.Player.Position - Parent.Position).Xz.LengthSquared < 24f * 24f)
                    _shouldTalk = true;
		    });
		}

	    public TalkComponent(Entity Parent) : this(Parent, null){}

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

	    public void Talk(bool Silent){
            if(!Silent) SoundManager.PlayUISound(SoundType.TalkSound, 1f, .75f);
			string phrase = _phrase ?? Phrases[Utils.Rng.Next(0, Phrases.Length)];
			
			var textSize = new GUIText(phrase, Vector2.Zero, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 10));

	        Vector3 FollowFunc()
	        {
	            return Parent.Position + Vector3.UnitY * 12f;
	        }
            var backBoard = new Billboard(Duration, Bar.BarBlueprint, FollowFunc(),
		        textSize.UIText.Scale + new Vector2(textSize.UIText.Scale.Y * .25f, textSize.UIText.Scale.Y * .25f))
		    {
		        FollowFunc = FollowFunc
            };

		    _board = new Billboard(Duration, phrase, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 10), FollowFunc())
		    {
		        FollowFunc = FollowFunc
            };
	        OnTalk?.Invoke(this.Parent);

            textSize.Dispose();
			Talked = true;
		}

	    public override void Dispose()
	    {
	        base.Dispose();
            EventDispatcher.UnregisterKeyDown(this);
	    }
	}
}
