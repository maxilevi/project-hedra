/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 31/12/2016
 * Time: 05:33 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Drawing;
using Hedra.Engine.Events;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Input;
using Hedra.Engine.Player;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of RideComponent.
	/// </summary>
	public class RideComponent : EntityComponent, ITickable
    {
		public Humanoid Rider;
		public bool HasRider;
		public bool UnRidable = false;
		public float HeightAddon = 0;
		private AIComponent AI;
		private HealthBarComponent _healthBar;
		
		public RideComponent(Entity Parent) : base(Parent) {
			AI = Parent.SearchComponent<AIComponent>();
			_healthBar = Parent.SearchComponent<HealthBarComponent>();
		}
		
		public override void Update(){
			//Player
			Parent.Model.Tint = new Vector4(1f,1f,1f,1);
			LocalPlayer player = Scenes.SceneManager.Game.Player;
			if( !HasRider && (player.BlockPosition - Parent.BlockPosition).LengthSquared < 12*12 && !player.IsRiding && !player.IsCasting
			    && Vector3.Dot( (Parent.BlockPosition - player.BlockPosition).NormalizedFast(), player.View.LookAtPoint.NormalizedFast()) > .6f)
			{
			    var model = Parent.Model as IMountable;
			    if(model != null && model.IsMountable && !Parent.IsUnderwater && !Parent.Knocked){					
					player.MessageDispatcher.ShowMessage("[E] TO MOUNT", .5f, Color.White);
					Parent.Model.Tint = new Vector4(1.5f,1.5f,1.5f,1);
                    if(EventDispatcher.LastKeyDown == Key.E)
					    this.Ride(player);					
				}
			}
			if(AI == null) AI = Parent.SearchComponent<AIComponent>();
			if(_healthBar == null) _healthBar = Parent.SearchComponent<HealthBarComponent>();

			if(HasRider && Rider.IsRiding)
			{
				if(!Rider.IsMoving)
					Parent.Model.Idle();
				else
					Parent.Model.Run();

			}
			if( HasRider && Rider is LocalPlayer && EventDispatcher.LastKeyDown == Key.ShiftLeft || HasRider && Rider.IsDead )
				Rider.IsRiding = false;


			if(HasRider && !Rider.IsRiding){
			    var ridePlayer = Rider as LocalPlayer;
			    if(ridePlayer != null){
			        ridePlayer.Model.LeftWeapon.MainMesh.DontCull = false;
					for(int i = 0; i < ridePlayer.Model.LeftWeapon.Meshes.Length; i++){
						ridePlayer.Model.LeftWeapon.Meshes[i].DontCull = false;
					}
				}
				Parent.Position = Rider.Position;
			    Rider.Model.MountModel.AlignWithTerrain = true;
                Rider.Model.MountModel = null;
				Rider = null;
				HasRider = false;
				if(AI != null) AI.DoLogic = true;
				if(_healthBar != null) _healthBar.Hide = false;
				
				
				Parent.Physics.UsePhysics = true;
				Parent.Physics.HasCollision = true;
				Parent.SearchComponent<DamageComponent>().Immune = false;
				
			}
		}
		
		public void Ride(Humanoid Entity){
			if(HasRider || UnRidable || Entity.IsRiding)return;
			
			Rider = Entity;
			Rider.ComponentManager.AddComponentWhile(new SpeedBonusComponent(Rider, -Rider.Speed + Parent.Speed * .5f), () => Rider.IsRiding);
			HasRider = true;
			Rider.IsRiding = true;
			Rider.Model.MountModel = Parent.Model as QuadrupedModel;
		    Rider.Model.MountModel.AlignWithTerrain = false;
			Parent.Physics.UsePhysics = false;
			Parent.Physics.HasCollision = false;
			Parent.SearchComponent<DamageComponent>().Immune = true;
		    var player = Entity as LocalPlayer;
		    if(player != null){
		        player.Model.LeftWeapon.MainMesh.DontCull = true;
				for(int i = 0; i < player.Model.LeftWeapon.Meshes.Length; i++){
					player.Model.LeftWeapon.Meshes[i].DontCull = true;
				}
			}

		    if (AI == null) AI = Parent.SearchComponent<AIComponent>();
		    if (_healthBar == null) _healthBar = Parent.SearchComponent<HealthBarComponent>();


            if (AI != null) AI.DoLogic = false;
			if(_healthBar != null && Rider is LocalPlayer) _healthBar.Hide = true;
		}
	}
}
