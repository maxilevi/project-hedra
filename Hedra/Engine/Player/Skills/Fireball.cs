/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/07/2016
 * Time: 11:59 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Rendering;
using System.Drawing;
using System.IO;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Fireball.
	/// </summary>
	public class Fireball : BaseSkill
	{
		private bool LeftHand;
		private float Damage = 12f;
		private int FireballCount = 0;
		private ParticleSystem Particles = new ParticleSystem(Vector3.Zero);
		private Animation FireballAnimation;
		
		public Fireball(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadTexture( new Bitmap( new MemoryStream(AssetManager.ReadBinary("Fireball.png", AssetManager.DataFile3))) );
			base.MaxCooldown = 1.5f;
			base.ManaCost = 15f;
			
			/*FireballAnimation = AnimationLoader.LoadAnimation("Assets/Chr/MageFireball.dae");
			FireballAnimation.OnAnimationEnd += delegate(Animation Sender) {
				
				this.Player.IsCasting = false;
				this.Casting = false;
				
				Sound.SoundManager.PlaySound(Sound.SoundType.SWOOSH_SOUND, Player.Position, false, 0.8f, 1f);
				if(FireballCount == FireballCombo){
					Fireball.BuildFireball(Damage, Level, LeftHand, Particles);
					Fireball.BuildFireball(Damage, Level, LeftHand, Particles);
					Fireball.BuildFireball(Damage, Level, LeftHand, Particles);
				}else{
					Fireball.BuildFireball(Damage, Level, LeftHand, Particles);
				}
			};*/
		}
		
		private int FireballCombo = 5;
		public override void KeyDown(){
			base.MaxCooldown = 1.75f - base.Level * .15f;
			Player.IsCasting = true;
			Casting = true;
			LeftHand = !LeftHand;
			FireballCount++;
			if(FireballCount > FireballCombo)
				FireballCount = 0;
			Player.Model.Model.PlayAnimation(FireballAnimation);
		}
		
		public override void Update(){ }
		
		public static void BuildFireball(float Damage, float Level, bool LeftHand, ParticleSystem Particles){
			LocalPlayer Player = LocalPlayer.Instance;
			float RandomScale = Mathf.Clamp(Utils.Rng.NextFloat() * 2f -1f, 1, 2);
			ParticleProjectile Fire = new ParticleProjectile(Vector3.One + new Vector3(RandomScale, RandomScale, RandomScale) * 0.35f,
			                            ((LeftHand) ? Player.Model.LeftWeaponPosition - Vector3.UnitX * .5f : Player.Model.RightWeaponPosition + Vector3.UnitX * .5f) + Player.Orientation * 2 + Vector3.UnitY * 2f,
			                            2, Player.View.CrossDirection * 4f, Player);
			Fire.UseLight = true;
			
			Fire.Trail = true;
			
			Particles.Position = Fire.Position;
			Particles.Color = Particle3D.FireColor;
			Particles.Direction = Fire.Direction;
			Particles.ParticleLifetime = 4;
			Particles.GravityEffect = 0f;
			Particles.PositionErrorMargin = new Vector3(1f,1f,1f);
			Particles.Scale = Vector3.One * .5f;
			Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);

			for(int i = 0; i < 30; i++){
				Particles.Emit();
			}
			
			Fire.HitEventHandler += delegate(ParticleProjectile Sender, Entity Hit) { 
				float Exp;
				Hit.Damage(Damage * Math.Max(1, Level * .7f), Player, out Exp);
				Player.XP += Exp;
				Particles.Position = Hit.Position; 
				Particles.PositionErrorMargin = new Vector3(1f,1f,1f);
				Particles.ParticleLifetime = 0.5f;
				for(int i = 0; i < 50; i++){
					Vector3 Dir = (Mathf.RandomVector3(Utils.Rng) - Vector3.One * 0.5f);
					Particles.Direction = Dir;
					Particles.Emit();
				}
				Sender.Dispose();
			};
		}
		
		public override string Description {
			get {
				return "Shoot a fireball.";
			}
		}
	}
}
