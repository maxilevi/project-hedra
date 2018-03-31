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
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Sound;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of TalkComponent.
	/// </summary>
	public class TalkComponent : EntityComponent, ITickable
	{
		public static string[] Phrases = new string[]{
            "Have you tried selling your items" +
			Environment.NewLine+"in the market?"+Environment.NewLine
			+"They accept every kind of object.",
			
			"Rumor says cementeries hold great"+
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
	    private string _phrase;
		
		public TalkComponent(Entity Parent, string Text) : base(Parent)
		{
		    _phrase = Utils.FitString(Text, 45);
		}

	    public TalkComponent(Entity Parent) : this(Parent, null){}

        public override void Update(){}

	    public void Talk()
	    {
            this.Talk(false);
	    }

	    public void Talk(bool Silent){
            if(!Silent)
			    SoundManager.PlayUISound(SoundType.NotificationSound, 1f, .75f);
			string phrase = _phrase ?? Phrases[Utils.Rng.Next(0, Phrases.Length)];
			
			var textSize = new GUIText(phrase, Vector2.Zero, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 10));

		    var backBoard = new Billboard(8f, Bar.BarBlueprint, Vector3.Zero,
		        textSize.UIText.Scale + new Vector2(textSize.UIText.Scale.Y * .25f, textSize.UIText.Scale.Y * .25f))
		    {
		        FollowFunc = () => Parent.Position + Vector3.UnitY * 10f
		    };


		    _board = new Billboard(8f, phrase, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 10), Vector3.Zero)
		    {
		        FollowFunc = () => Parent.Position + Vector3.UnitY * 10f
		    };

		    textSize.Dispose();
			Talked = true;
		}
	}
}
