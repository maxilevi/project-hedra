/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 12/08/2016
 * Time: 01:33 p.m.
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
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Generation;
using Hedra.Engine.Player.Skills;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of FlowingMagma.
	/// </summary>
	public class FlowingMagma : BaseSkill
	{
		private float Damage = 17.5f;
		private float DurationTime = 6f;
		private float Time;
		private Vector3 PreviousPosition;
		public override string DisplayName => "Flame Rush";
		
		public FlowingMagma() : base() {
			base.TextureId = Graphics2D.LoadFromAssets("Assets/Skills/Conflagaration.png");
			base.MaxCooldown = 14f;
			base.ManaCost = 75f;
		}
		
		public override void Use(){
			Player.IsCasting = true;
			Casting = true;
			base.MaxCooldown = 16f - Math.Min(4, base.Level * .75f);
			this.DurationTime = 4f + Math.Min(4, base.Level * .75f);
			this.Damage = 12f + Math.Min(8, base.Level);
			Time = DurationTime;
		}
		
		public override string Description {
			get {
				return "Leave a fire trail at your feet to damage your foes.";
			}
		}
		
		
		public override void Update(){
			if(Casting && Player.IsCasting){
				Time -= Engine.Time.IndependantDeltaTime;
				
				if(Player.IsGrounded && PreviousPosition != Player.BlockPosition){
					var B = World.GetHighestBlockAt( (int) this.Player.Position.X, (int) this.Player.Position.Z);
					World.Particles.VariateUniformly = true;
					World.Particles.Color = Particle3D.FireColor;
					World.Particles.Position = this.Player.Position - Vector3.UnitY;
					World.Particles.Scale = Vector3.One * .5f;
					World.Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
					World.Particles.Direction = (-this.Player.Orientation + Vector3.UnitY * 2.75f) * .15f;
					World.Particles.ParticleLifetime = 1;
					World.Particles.GravityEffect = .1f;
					World.Particles.PositionErrorMargin = new Vector3(.75f, .75f, .75f);
					
					for(int i = 0; i < 4; i++){
						World.Particles.Emit();
					}
					PreviousPosition = Player.BlockPosition;
				}
					
				if(Time <= 0){
					Player.IsCasting = false;
					Casting = false;
				} 
			}
		}
	}
}
