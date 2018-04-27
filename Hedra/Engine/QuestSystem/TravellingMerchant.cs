using System;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Player;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of Campfire.
	/// </summary>
	public class TravellingMerchant : BaseStructure, IUpdatable
	{
		private long _passedTime;
		private readonly ParticleSystem _particles;
		private PointLight _light;
		
		
		public TravellingMerchant(Vector3 Position) {
			_particles = new ParticleSystem(Vector3.Zero);		
			base.Position = Position;

			UpdateManager.Add(this);
		}
		
		public void Update(){
			_passedTime++;

		    try
		    {
		        for (int i = World.Entities.Count - 1; i > -1; i--)
		        {
		            if ((World.Entities[i].Position - Position).LengthSquared < 4 * 4)
		            {
		                if (World.Entities[i].SearchComponent<BurningComponent>() == null)
		                {
		                    World.Entities[i].AddComponent(new BurningComponent(World.Entities[i], 5f, 40f));
		                }
		            }
		        }
		    }
		    catch (IndexOutOfRangeException e)
		    {
		        Log.WriteLine(e);
		    }

		    if(_light == null && (Position - LocalPlayer.Instance.Position).LengthSquared < ShaderManager.LightDistance * ShaderManager.LightDistance * 2f){
				_light = ShaderManager.GetAvailableLight();
				if(_light != null){
					_light.Color = new Vector3(1f, 0.6f, 0.6f);
					_light.Position = Position;
					_light.Radius = 30f;
					ShaderManager.UpdateLight(_light);
				}
			}
			if(_light != null && (_light.Position - LocalPlayer.Instance.Position).LengthSquared > ShaderManager.LightDistance * ShaderManager.LightDistance * 2f){
				_light.Position = Vector3.Zero;
				_light.Locked = false;
				_light.Radius = 20f;
				ShaderManager.UpdateLight(_light);
				_light = null;
			}
			
			if(_passedTime % 2 == 0){
				_particles.Color = Particle3D.FireColor;
				_particles.VariateUniformly = false;
				_particles.Position = this.Position + Vector3.UnitY * 1f;
				_particles.Scale = Vector3.One * .85f;
				_particles.ScaleErrorMargin = new Vector3(.05f,.05f,.05f);
				_particles.Direction = Vector3.UnitY * 0f;
				_particles.ParticleLifetime = 1.65f;
				_particles.GravityEffect = -0.01f;
				_particles.PositionErrorMargin = new Vector3(1f, 0f, 1f);
				
				_particles.Emit();
			}
		}
		
		public override void Dispose(){
			base.Dispose();
			if(_light != null){
				_light.Color = Vector3.Zero;
				_light.Position = Vector3.Zero;
				ShaderManager.UpdateLight(_light);
				_light.Locked = false;
			}
		}
	}
}
