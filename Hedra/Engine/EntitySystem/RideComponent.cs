/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 31/12/2016
 * Time: 05:33 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Drawing;
using Hedra.Engine.AISystem;
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
		public IHumanoid Rider;
		public bool HasRider;
		public bool UnRidable = false;
		public float HeightAddon = 0;
		private BasicAIComponent AI;
		private HealthBarComponent _healthBar;
        private bool _shouldRide;
        private bool _shouldUnride;
        private bool _canRide;
        private bool _canUnride;

        public RideComponent(Entity Parent) : base(Parent) {
			AI = Parent.SearchComponent<BasicAIComponent>();
			_healthBar = Parent.SearchComponent<HealthBarComponent>();
            EventDispatcher.RegisterKeyDown(this, delegate(object Object, KeyEventArgs EventArgs)
            {
                _shouldRide = EventArgs.Key == Key.E && _canRide;
                _shouldUnride = EventArgs.Key == Key.ShiftLeft && _canUnride;
            });
		}
		
		public override void Update(){
			var player = GameManager.Player;
		    if (!HasRider && (player.BlockPosition - Parent.BlockPosition).LengthSquared < 12 * 12 && !player.IsRiding &&
		        !player.IsCasting
		        && Vector3.Dot((Parent.BlockPosition - player.BlockPosition).NormalizedFast(), player.View.LookingDirection) >
		        .6f)
		    {
		        if (Parent.Model is IMountable model && model.IsMountable && !Parent.IsUnderwater && !Parent.IsKnocked)
		        {
		            player.MessageDispatcher.ShowMessage("[E] TO MOUNT", .5f, Color.White);
		            Parent.Model.Tint = new Vector4(2.0f, 2.0f, 2.0f, 1f);
		            _canRide = true;
                    if (_shouldRide) this.Ride(player);
		        }
		        else
		        {		            
                    _canRide = false;
		        }
		    }
		    else
		    {
		        _canRide = false;
		    }
			if(AI == null) AI = Parent.SearchComponent<BasicAIComponent>();
			if(_healthBar == null) _healthBar = Parent.SearchComponent<HealthBarComponent>();

		    if (HasRider && Rider.IsRiding)
		    {
		        _canUnride = true;
		    }
		    else
		    {
		        _canUnride = false;
		    }
			if( HasRider && Rider is LocalPlayer && _shouldUnride || HasRider && Rider.IsDead )
				Rider.IsRiding = false;


			if(HasRider && !Rider.IsRiding){
				Parent.Position = Rider.Position;
			    Rider.Model.MountModel.AlignWithTerrain = true;
                Rider.Model.MountModel = null;
				Rider = null;
				HasRider = false;
				if(AI != null) AI.Enabled = true;
				if(_healthBar != null) _healthBar.Hide = false;
				
				
				Parent.Physics.UsePhysics = true;
				Parent.Physics.HasCollision = true;
				Parent.SearchComponent<DamageComponent>().Immune = false;				
			}
		}
		
		public void Ride(IHumanoid Entity){
			if(HasRider || UnRidable || Entity.IsRiding)return;
			
			Rider = Entity;
			Rider.ComponentManager.AddComponentWhile(new SpeedBonusComponent(Rider, -Rider.Speed + Parent.Speed * .5f), () => Rider != null && Rider.IsRiding);
			HasRider = true;
			Rider.IsRiding = true;
			Rider.Model.MountModel = (QuadrupedModel) Parent.Model;
		    Rider.Model.MountModel.AlignWithTerrain = false;
			Parent.Physics.UsePhysics = false;
			Parent.Physics.HasCollision = false;
			Parent.SearchComponent<DamageComponent>().Immune = true;

		    if (AI == null) AI = Parent.SearchComponent<BasicAIComponent>();
		    if (_healthBar == null) _healthBar = Parent.SearchComponent<HealthBarComponent>();


            if (AI != null) AI.Enabled = false;
			if(_healthBar != null && Rider is LocalPlayer) _healthBar.Hide = true;
		}
	}
}
