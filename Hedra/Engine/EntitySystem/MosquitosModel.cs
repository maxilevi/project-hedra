/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 26/09/2016
 * Time: 12:07 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Rendering;
using Hedra.Engine.Generation;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Particles;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of MosquitosModel.
	/// </summary>
	public class MosquitosModel : Model
	{
		public Entity Parent;
		public ParticleSystem Particles;
		public override Vector3 TargetRotation {get; set;}

		public MosquitosModel(Vector3 Scale, Entity Parent, Random Rng)
		{
			this.Scale = Scale;
			this.Parent = Parent;
			
			Particles = new ParticleSystem(Vector3.Zero);
			Particles.Color = new Vector4(0.1f, 0.1f, 0.1f, 1.000f);
			Particles.Grayscale = true;
			Particles.PositionErrorMargin = Vector3.One * 4;
			Particles.GravityEffect = 0f;
			Particles.ScaleErrorMargin = Vector3.One * .25f;
			Particles.RandomRotation = true;
			Particles.Scale = Vector3.One * .15f;
			Particles.ParticleLifetime = .05f;
			Particles.Shape = ParticleShape.SPHERE;
			
			this.Height = 2.5f * Scale.Average();
			this.Size = Scale;
		}
				
		private float AttackCooldown = 0;
		public override void Attack(Entity Damager, float Damage){
			if(AttackCooldown < 0){
				float Exp;
				Damager.Damage(Damage, Damager, out Exp, true);
				AttackCooldown = 3;
			}
			AttackCooldown -= Time.FrameTimeSeconds;
		}
		
		public override void Update(){
			Particles.Position = this.Position;
			
			for(int i = 0; i < 20; i++){
				Particles.Emit();
			}
		}
		
		public override void Run(){}
		public override void Idle(){}
	}
}
