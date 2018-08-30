/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 17/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player.Skills;
using OpenTK;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Bash.
	/// </summary>
	public class Bash : BaseSkill
	{	
		private float Damage = 20f;
		private Animation BashAnimation;
		
		public Bash() : base() {
			base.TextureId = Graphics2D.LoadFromAssets("Assets/Skills/Bash.png");
			base.ManaCost = 15f;
			base.MaxCooldown = 3f;
			
			this.BashAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorBash.dae");
			this.BashAnimation.Loop = false;
		    this.BashAnimation.OnAnimationStart += delegate
		    {
		        Sound.SoundManager.PlaySound(Sound.SoundType.SlashSound, Player.Position);
		    };
            this.BashAnimation.OnAnimationEnd += delegate { 
				Player.IsCasting = false;
				Casting = false;
				Player.WasAttacking = false;
			};
			
			this.BashAnimation.OnAnimationMid += delegate { 
				for(int i = 0; i< World.Entities.Count; i++){
					if(World.Entities[i] == Player)
						continue;
					
					Vector3 ToEntity = (World.Entities[i].Position - Player.Position).NormalizedFast();
					float Dot = Mathf.DotProduct(ToEntity, Player.Orientation);
					if(Dot >= .65f && (World.Entities[i].Position - Player.Position).LengthSquared < 9f*9f){
						float Exp;
						World.Entities[i].Damage(this.Damage * Dot * 1.25f, Player, out Exp, true);
						Player.XP += Exp;
					}
				}
				
			};
		}
		
		
		public override void Use(){
			base.MaxCooldown = Math.Max(4f - base.Level * .25f, 1.5f);
			Damage = 20f * base.Level * .6f + 5f;
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = false;
			Player.WasAttacking = false;
			Player.LeftWeapon.InAttackStance = false;
			Player.Model.PlayAnimation(BashAnimation);
		}
		
		public override void Update(){
			if(Player.IsCasting && Casting){
				Player.Movement.Orientate();
			}
		}
		
		public override string Description => "A powerful smashing blow.";
	}
}
