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
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering.Particles;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of FlameDisk.
	/// </summary>
	public class FlameDisk : BaseSkill
	{
		private const float Duration = 8f;
		private bool Emitting = false;
		private ParticleSystem HandParticles;
		
		public FlameDisk() : base() {
			base.TextureId = Graphics2D.LoadTexture( new Bitmap( new MemoryStream(AssetManager.ReadBinary("FlameDisk.png", AssetManager.DataFile3))) );
			base.MaxCooldown = 16.5f;
			base.ManaCost = 30f;
			HandParticles = new ParticleSystem(Vector3.Zero);
			HandParticles.Scale = new Vector3(.5f,.5f,.5f);
			HandParticles.Color = Particle3D.FireColor;
			HandParticles.ParticleLifetime = .5f;
		}
		
		public override void Use(){
			Emitting = true;
			Color = new Vector4(.25f,0,0,1);
		}
		
		private Vector4 Color;
		public override void Update(){
			if(Emitting){
				
				Player.Model.Tint = Color;
				
				for(int i = 0; i< World.Entities.Count; i++){
					if( (World.Entities[i].Position - Player.Position).LengthSquared <= 100 && World.Entities[i] != Player){
						
						float Exp;
						World.Entities[i].Damage(4 * (float) Time.DeltaTime * Player.Level * 0.4f, Player, out Exp, false);
						Player.XP += Exp;
					}
				}
				
				if(base.Cooldown <= base.MaxCooldown - Duration){
					Player.Model.Tint = new Vector4(0f,0f,0,1);
					Emitting = false;
				}
			}
		}
		public override string Description {
			get {
				return "Creates a fire aura that damages your enemies around you.";
			}
		}
	}
}
