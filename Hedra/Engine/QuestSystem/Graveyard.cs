/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 11/06/2017
 * Time: 03:58 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Hedra.Engine.EntitySystem;
using OpenTK;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Sound;

namespace Hedra.Engine.QuestSystem
{
    /// <inheritdoc cref="BaseStructure" />
    /// <summary>
    /// Description of Cementary.
    /// </summary>
    public class Graveyard : BaseStructure, IUpdatable
	{
		private readonly ParticleSystem _particles = new ParticleSystem();
		private readonly int _passedTime = 0;
	    private bool _restoreSoundPlayed;
        public Entity[] Enemies;
	    public bool Restored { get; private set; }
        public float Radius { get; set; }


        public Graveyard(Vector3 Position, float Radius){
			this.Position = Position;
            this.Radius = Radius;
			_particles.Position = this.Position;
			UpdateManager.Add(this);
		}
		
		public void Update()
		{

		    if (Enemies != null)
		    {
		        var allDead = true;
		        for (var i = 0; i < Enemies.Length; i++)
		        {
		            if (Enemies[i] != null && !Enemies[i].IsDead && (Enemies[i].BlockPosition - Position).Xz.LengthSquared
		                < Radius * Radius * .9f * .9f)
		            {
		                allDead = false;
		            }
		        }

		        this.Restored = allDead;
		    }

		    if (this.Restored && !_restoreSoundPlayed)
		    {
		        _restoreSoundPlayed = true;
                SoundManager.PlaySound(SoundType.DarkSound, LocalPlayer.Instance.Position);

		    }

            if (!Restored &&  (this.Position - GameManager.Player.Position).Xz.LengthSquared 
			   < Radius * Radius)
            {
			
				if(_passedTime % 2 == 0){
					_particles.Color = Particle3D.AshColor;
					_particles.VariateUniformly = false;
					_particles.Position = GameManager.Player.Position + Vector3.UnitY * 1f;
					_particles.Scale = Vector3.One * .85f;
					_particles.ScaleErrorMargin = new Vector3(.05f,.05f,.05f);
					_particles.Direction = Vector3.UnitY * 0f;
					_particles.ParticleLifetime = 2f;
					_particles.GravityEffect = -0.000f;
					_particles.PositionErrorMargin = new Vector3(64f, 12f, 64f);
					_particles.Grayscale = true;
					
					_particles.Emit();
				}
			}
		}
		
		public override void Dispose(){
            
			UpdateManager.Remove(this);
		}
	}
}
