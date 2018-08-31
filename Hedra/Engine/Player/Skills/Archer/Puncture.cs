/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Player.Skills.Archer
{
	/// <summary>
	/// Description of Resistance.
	/// </summary>
	public class Puncture : LearningSkill
	{
		public override uint TextureId => Graphics2D.LoadFromAssets("Assets/Skills/PierceArrows.png");

		public override void Update()
		{
			
		}
		
		protected override void Learn()
		{
			Console.WriteLine("Test");
		}

		private void PierceModifier(Projectile ArrowProj)
		{
			ArrowProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit)
			{
				if(Utils.Rng.Next(0, 5) == 0)
				{
					Hit.AddComponent( new BleedingComponent(Hit, Player, 3f, 20f) );
				}
			};
		}
		
		public override string Description => "Arrows have a high chance to cause bleeding.";
	}
}
