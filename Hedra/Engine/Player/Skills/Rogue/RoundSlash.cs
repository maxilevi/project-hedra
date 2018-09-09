/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.Player.Skills.Rogue
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class RoundSlash : BaseSkill
	{
		private readonly Animation RoundSlashAnimation;
	    private float _frameCounter;
		
		public RoundSlash()
		{
			base.TextureId = Graphics2D.LoadFromAssets("Assets/Skills/RoundSlash.png");
			base.ManaCost = 80f;
			base.MaxCooldown = 8.5f;
			
			RoundSlashAnimation = AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeRoundAttack.dae");
			RoundSlashAnimation.Loop = false;
			RoundSlashAnimation.OnAnimationStart += delegate { 
				Sound.SoundManager.PlaySound(Sound.SoundType.SwooshSound, Player.Position, false, 0.8f, 1f);
			};RoundSlashAnimation.OnAnimationEnd += delegate {
				Player.IsCasting = false;
				Casting = false;
				Player.IsAttacking = false;
				Player.LeftWeapon.InAttackStance = false;
			};
		}

		public override void Use(){
			base.MaxCooldown = 9f - Math.Min(5f, Level * .5f);
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = true;
			Player.Model.PlayAnimation(RoundSlashAnimation);
			Player.Movement.Orientate();
			Player.LeftWeapon.InAttackStance = true;
		}
		
		public override void Update(){
			if(Player.IsCasting && Casting){
				
				World.Particles.Color = new Vector4(1,1,1,1);
				World.Particles.ParticleLifetime = 1f;
				World.Particles.GravityEffect = .0f;
				World.Particles.Direction = Vector3.Zero;
				World.Particles.Scale = new Vector3(.15f,.15f,.15f);
				World.Particles.Position = Player.Model.LeftWeaponPosition;
				World.Particles.PositionErrorMargin = Vector3.One * 0.75f;
				World.Particles.Emit();

			    if (_frameCounter >= .25f)
			    {

			        for (var i = World.Entities.Count - 1; i > 0; i--)
			        {
			            if (!Player.InAttackRange(World.Entities[i])) continue;

			            float dmg = Player.DamageEquation * .2f * 2f * (1 + base.Level * .1f);
			            World.Entities[i].Damage(dmg, Player, out float exp, true);
			            Player.XP += exp;
			        }
			        _frameCounter = 0;
			    }
				_frameCounter += Time.DeltaTime;
			}
		}
		
		public override string Description => "Cast a special attack which damages surrounding enemies.";
	}
}