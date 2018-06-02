/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 11/08/2016
 * Time: 12:22 a.m.
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

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of FlameJump.
	/// </summary>
	public class FlameJump : BaseSkill
	{
		private const float Duration = .75f;
		private bool Emitting = false;
		private ParticleSystem HandParticles;
		
		public FlameJump() : base() {
			base.TexId = Graphics2D.LoadTexture( new Bitmap( new MemoryStream(AssetManager.ReadBinary("FlameJump.png", AssetManager.DataFile3))) );
			HandParticles = new ParticleSystem(Vector3.Zero);
			HandParticles.Scale = new Vector3(.5f,.5f,.5f);
			HandParticles.Color = Particle3D.FireColor;
			HandParticles.ParticleLifetime = .75f;
			HandParticles.Enabled = true;
		}
		
		public override void Use(){
			Emitting = true;
			base.MaxCooldown = 6f - 1f*base.Level;
			base.ManaCost = 30f - 5f * base.Level;
			
			//Player.Movement.JumpingDistance = 12f;
			Player.Movement.Jump();
		}
		
		public override void Update(){
			if(Emitting){
				
				HandParticles.Direction = -Vector3.UnitY * .25f;
				HandParticles.Position = Player.Position;
				for(int i = 0; i < 20; i++){
					HandParticles.Emit();
				}
				
				if(base.Cooldown <= base.MaxCooldown - Duration){
					Emitting = false;
					//Player.Movement.JumpingDistance = 4f;
				}
			}
		}
		
		public override string Description {
			get {
				return "Get propulsed into the sky. Useful for gliders.";
			}
		}
	}
}
