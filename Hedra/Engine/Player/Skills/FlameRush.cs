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
		
		public FlowingMagma(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadTexture( new Bitmap( new MemoryStream(AssetManager.ReadBinary("Conflagaration.png", AssetManager.DataFile3))) );
			base.MaxCooldown = 14f;
			base.ManaCost = 75f;
		}
		
		public override void KeyDown(){
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
				Time -= Engine.Time.FrameTimeSeconds;
				
				if(Player.IsGrounded && PreviousPosition != Player.BlockPosition){
					Block B = World.GetHighestBlockAt( (int) this.Player.Position.X, (int) this.Player.Position.Z);
					World.WorldParticles.VariateUniformly = true;
					World.WorldParticles.Color = Particle3D.FireColor;
					World.WorldParticles.Position = this.Player.Position - Vector3.UnitY;
					World.WorldParticles.Scale = Vector3.One * .5f;
					World.WorldParticles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
					World.WorldParticles.Direction = (-this.Player.Orientation + Vector3.UnitY * 2.75f) * .15f;
					World.WorldParticles.ParticleLifetime = 1;
					World.WorldParticles.GravityEffect = .1f;
					World.WorldParticles.PositionErrorMargin = new Vector3(.75f, .75f, .75f);
					
					for(int i = 0; i < 4; i++){
						World.WorldParticles.Emit();
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
