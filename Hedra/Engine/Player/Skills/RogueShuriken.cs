/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
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
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class Shuriken : BaseSkill
	{
		private readonly Animation _throwAnimation;
		private static readonly VertexData ShurikenData = AssetManager.PLYLoader("Assets/Items/Shuriken.ply", new Vector3(1, 2, 1));

        public Shuriken() : base() {
			base.TextureId = Graphics2D.LoadFromAssets("Assets/Skills/Shuriken.png");
			base.ManaCost = 35f;
			base.MaxCooldown = 8.5f;
			
			_throwAnimation = AnimationLoader.LoadAnimation("Assets/Chr/RogueShurikenThrow.dae");
			_throwAnimation.Loop = false;
			_throwAnimation.OnAnimationMid += delegate{
			    Shuriken.ShootShuriken(Player, Player.View.CrossDirection.NormalizedFast(), 20f * base.Level, 8);
			};
			_throwAnimation.OnAnimationEnd += delegate{
				Player.IsCasting = false;
				Casting = false;
				Player.IsAttacking = false;
			};
		}

		public static void ShootShuriken(IHumanoid Human, Vector3 Direction, float Damage, int KnockChance = -1){
			VertexData weaponData = ShurikenData.Clone();
			weaponData.Scale(Vector3.One * 1.75f);

		    var startingLocation = Human.Model.LeftWeaponPosition + Human.Orientation * .5f +
		                           Vector3.UnitY * 2f;

		    var weaponProj = new Projectile(Human, startingLocation, weaponData)
		    {
		        Propulsion = Direction * 2f,
		        Lifetime = 5f
		    };
		    weaponProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit) {
		        Hit.Damage(Damage, Human, out float exp, true);
				Human.XP += exp;
		        if (KnockChance == -1) return;
		        if(Utils.Rng.Next(0, KnockChance) == 0)
		            Hit.KnockForSeconds(3);
		    };
			Sound.SoundManager.PlaySound(Sound.SoundType.BowSound, Human.Position, false,  1f + Utils.Rng.NextFloat() * .2f - .1f, 2.5f);
		}
		
		public override void Use(){
			base.MaxCooldown = 8.5f - Math.Min(5f, base.Level * .5f);
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = true;
			Player.Model.LeftWeapon.InAttackStance = false;
			Player.Model.Model.Animator.StopBlend();
			Player.Model.Model.PlayAnimation(_throwAnimation);
			Player.Movement.Orientate();
		}
		
		public override void Update(){
			if(Player.IsCasting && Casting){
				
				World.Particles.Color = new Vector4(1,1,1,1);
				World.Particles.ParticleLifetime = 1f;
				World.Particles.GravityEffect = .0f;
				World.Particles.Direction = Vector3.Zero;
				World.Particles.Scale = new Vector3(.25f,.25f,.25f);
				World.Particles.Position = Player.Model.Model.TransformFromJoint(Player.Model.Model.JointDefaultPosition(Player.Model.LeftWeaponJoint)
				                                                                             + Vector3.UnitZ *0f, Player.Model.LeftWeaponJoint);
				World.Particles.PositionErrorMargin = Vector3.One * 0.75f;
				
				for(int i = 0; i < 1; i++)
					World.Particles.Emit();
			}
		}
		
		public override string Description {
			get {
				return "Throw a shuriken at your foes.";
			}
		}
	}
}