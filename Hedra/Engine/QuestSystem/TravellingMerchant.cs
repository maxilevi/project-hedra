using System;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Player;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of Campfire.
	/// </summary>
	public class TravellingMerchant : BaseStructure, IUpdatable
	{
		private long PassedTime;
		private static ParticleSystem FireParticles;
		private PointLight Light;
		
		
		public TravellingMerchant(Vector3 Position) : base() {
			if(FireParticles == null)
				FireParticles = new ParticleSystem(Vector3.Zero);
			
			base.Position = Position;

			UpdateManager.Add(this);
		}
		
		public void Update(){
			PassedTime++;
			
			for(int i = World.Entities.Count-1; i > -1; i--){
				if( (World.Entities[i].Position - Position).LengthSquared < 4*4){
					if(World.Entities[i].SearchComponent<BurningComponent>() == null){
						World.Entities[i].AddComponent( new BurningComponent(World.Entities[i], 5f, 40f) );
					}
				}
			}
			
			if(Light == null && (Position - LocalPlayer.Instance.Position).LengthSquared < ShaderManager.LightDistance * ShaderManager.LightDistance * 2f){
				Light = ShaderManager.GetAvailableLight();
				if(Light != null){
					Light.Color = new Vector3(1f, 0.6f, 0.6f);
					Light.Position = Position;
					Light.Radius = 30f;
					ShaderManager.UpdateLight(Light);
				}
			}
			if(Light != null && (Light.Position - LocalPlayer.Instance.Position).LengthSquared > ShaderManager.LightDistance * ShaderManager.LightDistance * 2f){
				Light.Position = Vector3.Zero;
				Light.Locked = false;
				Light.Radius = 20f;
				ShaderManager.UpdateLight(Light);
				Light = null;
			}
			
			if(PassedTime % 2 == 0){
				FireParticles.Color = Particle3D.FireColor;
				FireParticles.VariateUniformly = false;
				FireParticles.Position = this.Position + Vector3.UnitY * 1f;
				FireParticles.Scale = Vector3.One * .85f;
				FireParticles.ScaleErrorMargin = new Vector3(.05f,.05f,.05f);
				FireParticles.Direction = Vector3.UnitY * 0f;
				FireParticles.ParticleLifetime = 1.65f;
				FireParticles.GravityEffect = -0.01f;
				FireParticles.PositionErrorMargin = new Vector3(1f, 0f, 1f);
				
				FireParticles.Emit();
			}
		}
		
		public override void Dispose(){
			base.Dispose();
			if(Light != null){
				Light.Color = Vector3.Zero;
				Light.Position = Vector3.Zero;
				ShaderManager.UpdateLight(Light);
				Light.Locked = false;
			}
		}
	}
}
