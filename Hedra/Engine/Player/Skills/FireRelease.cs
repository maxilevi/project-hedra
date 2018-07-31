/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 05/07/2016
 * Time: 10:06 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using Hedra.Engine.Rendering;
using System.Drawing;
using System.IO;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of FireRelease.
	/// </summary>
	public class FireRelease : BaseSkill
	{
		
		private float Damage = 50f;
		private PointLight Light;
		private ParticleSystem Particles;
		private Animation FireReleaseAnimation;
		
		public FireRelease() : base() {
			base.TexId = Graphics2D.LoadTexture( new Bitmap( new MemoryStream(AssetManager.ReadBinary("FireRelease.png", AssetManager.DataFile3))) );
			base.MaxCooldown = .5f;
			base.ManaCost = 5f;
			this.Particles = new ParticleSystem(Vector3.Zero);
			
			/*this.FireReleaseAnimation = AnimationLoader.LoadAnimation("Assets/Chr/MageFireRelease.dae");
			
			this.FireReleaseAnimation.OnAnimationMid += delegate { 
				Sound.SoundManager.PlaySound(Sound.SoundType.SWOOSH_SOUND, Player.Position, false, 0.8f, 1f);
			};*/
		}
		
		public override void KeyUp(){
			this.Continue = false;
		}
		
		public override void Use(){
			Player.IsCasting = true;
			Casting = true;
			Player.Movement.CaptureMovement = false;
			Continue = true;
			if(Light == null){
				Light = ShaderManager.GetAvailableLight();
				Light.Color = new Vector3(1f, .2f, .2f);
				Light.Position = Player.Position + Player.Orientation * 2f;
				ShaderManager.UpdateLight(Light);
			}
			this.Player.Model.Model.PlayAnimation(FireReleaseAnimation);
		}
		
		private bool Continue;
		public override void Update(){
			if(Casting && Player.IsCasting){

				for(int i = 0; i < 5; i++)
					Particles.Emit();
				this.DamageEntities();
				
				if(Player.Mana < base.ManaCost){
					Continue = false;
				}
				
				Player.Mana -= Engine.Time.IndependantDeltaTime * 40f;
				
				Player.Movement.Orientate();
				SetupParticles();
				
				if(!Continue){
					Player.IsCasting = false;
					Casting = false;
					Player.Movement.CaptureMovement = true;
				}
				
			}else if(!Casting && Light != null){
				Light.Locked = false;
				Light.Position = Vector3.Zero;
				ShaderManager.UpdateLight(Light);
				Light = null;
			}
		}
		
		private void DamageEntities(){
			for(int i = 0; i< World.Entities.Count; i++){
				if(World.Entities[i] == Player)
					continue;
				
				Vector3 ToEntity = (World.Entities[i].Position - Player.Position).NormalizedFast();
				float Dot = Mathf.DotProduct(ToEntity, Player.Orientation);
				if(Dot >= .75f && (World.Entities[i].Position - Player.Position).LengthSquared < 320){
					float Exp;
					World.Entities[i].Damage(this.Damage * Dot * (float) Engine.Time.DeltaTime * (base.Level * 0.6f), Player, out Exp, false);
					Player.XP += Exp;
				}
			}
		}
		
		private void SetupParticles(){
			Particles.Position = Player.Position + Player.Orientation * 2;
			Particles.Direction = Player.Orientation;
			Particles.Color = Particle3D.FireColor;
			Particles.GravityEffect = 0f;
			Particles.Scale = Vector3.One * .5f;
			Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
			Particles.Shape = ParticleShape.Cone;
			Particles.ConeAngle = -95f;
			Particles.ParticleLifetime = 2.25f;
		}
		
		public override string Description {
			get {
				return "A cone of flames to burn your enemies.";
			}
		}	
	}
}
