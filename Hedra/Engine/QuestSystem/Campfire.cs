/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 12/12/2016
 * Time: 04:54 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using  System.Linq;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Player;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of Campfire.
	/// </summary>
	internal class Campfire : BaseStructure, IUpdatable
	{
	    private static ParticleSystem _fireParticles;
        private long _passedTime;
		private PointLight _light;
	    private SoundItem _sound;
		public Vector3 Position { get; set; }
		
		public Campfire(Vector3 Position) : base() {
			if(_fireParticles == null)
				_fireParticles = new ParticleSystem(Vector3.Zero);
		    _fireParticles.HasMultipleOutputs = true;

            this.Position = Position;
            UpdateManager.Add(this);
		}
		
		public void Update(){
			this._passedTime++;

		    if (this._passedTime % 2 == 0)
		    {
		        _fireParticles.Color = Particle3D.FireColor;
		        _fireParticles.VariateUniformly = false;
		        _fireParticles.Position = this.Position + Vector3.UnitY * 1f;
		        _fireParticles.Scale = Vector3.One * .85f;
		        _fireParticles.ScaleErrorMargin = new Vector3(.05f, .05f, .05f);
		        _fireParticles.Direction = Vector3.UnitY * 0f;
		        _fireParticles.ParticleLifetime = 1.65f;
		        _fireParticles.GravityEffect = -0.01f;
		        _fireParticles.PositionErrorMargin = new Vector3(1f, 0f, 1f);

		        _fireParticles.Emit();
		    }

		    try
		    {
		        for (int i = World.Entities.Count - 1; i > -1; i--)
		        {
		            if (World.Entities[i] == null) continue;
		            if ((World.Entities[i].Position - Position).LengthSquared < 4 * 4)
		            {
		                if (World.Entities[i].SearchComponent<BurningComponent>() == null)
		                {
		                    World.Entities[i].AddComponent(new BurningComponent(World.Entities[i], 5f, 40f));
		                }
		            }
		        }
		    }
		    catch (ArgumentOutOfRangeException)
		    {
		        Log.WriteLine("ArgumentException while looping though entities.");
		    }

		    if( (this._light == null) && (this.Position - LocalPlayer.Instance.Position).LengthSquared < ShaderManager.LightDistance * ShaderManager.LightDistance * 2f){

                this._light = ShaderManager.GetAvailableLight();

				if(this._light != null){
					this._light.Color = new Vector3(1f, 0.4f, 0.4f);
					this._light.Position = Position;
					ShaderManager.UpdateLight(this._light);
				}
			}

		    if (this._sound == null && (this.Position - LocalPlayer.Instance.Position).LengthSquared < 32f*32f*2f)
		    {
		        this._sound = SoundManager.GetAvailableSource();
            }

		    if (this._sound != null)
		    {

		        if (!this._sound.Source.IsPlaying)
		            this._sound.Source.Play(SoundManager.GetBuffer(SoundType.Fireplace), this.Position, 1f, 1f, true);

		        float gain = System.Math.Max(0, 1 - (this.Position - SoundManager.ListenerPosition).LengthFast / 32f);
		        this._sound.Source.Volume = gain;
		    }


            if ( this._light != null && (this.Position - LocalPlayer.Instance.Position).LengthSquared >
                ShaderManager.LightDistance * ShaderManager.LightDistance * 2f){

                this._light.Position = Vector3.Zero;
			    this._light.Locked = false;
			    ShaderManager.UpdateLight(this._light);
			    this._light = null;
			    
            }

		    if (this._sound != null && (this.Position - LocalPlayer.Instance.Position).LengthSquared > 32f * 32f * 2f)
		    {
		        this._sound.Source.Stop();
		        this._sound.Locked = false;
		        this._sound = null;
            }
		}

	    public override void Dispose()
	    {
	        base.Dispose();
	        if (this._light != null)
	        {
	            this._light.Color = Vector3.Zero;
	            this._light.Position = Vector3.Zero;
	            ShaderManager.UpdateLight(this._light);
	            this._light.Locked = false;
	            this._light = null;
	        }
	        if (this._sound != null)
	        {
	            this._sound.Locked = false;
	            this._sound = null;
	        }
	    }
	}
}
