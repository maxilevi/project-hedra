/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 31/12/2016
 * Time: 05:33 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Drawing;
using Hedra.AISystem;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Input;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Localization;

namespace Hedra.Engine.EntitySystem
{
    /// <summary>
    /// Description of RideComponent.
    /// </summary>
    public class RideComponent : EntityComponent, ITickable
    {
        public bool HasRider { get; private set; }
        private IHumanoid _rider;
        private BasicAIComponent _ai;
        private HealthBarComponent _healthBar;
        private bool _shouldRide;
        private bool _shouldUnride;
        private bool _canRide;
        private bool _canUnride;

        public RideComponent(IEntity Parent) : base(Parent)
        {
            EventDispatcher.RegisterKeyDown(this, delegate(object Object, KeyEventArgs EventArgs)
            {
                _shouldRide = EventArgs.Key == Controls.Interact && _canRide;
                _shouldUnride = EventArgs.Key == Controls.Descend && _canUnride;
            });
        }
        
        public override void Update()
        {
            var player = GameManager.Player;
            if (!HasRider && (player.BlockPosition - Parent.BlockPosition).LengthSquared < 12 * 12 && !player.IsRiding &&
                !player.IsCasting
                && Vector3.Dot((Parent.BlockPosition - player.BlockPosition).NormalizedFast(), player.View.LookingDirection) >
                .6f)
            {
                if (Parent.Model is IMountable model && model.IsMountable && !Parent.IsUnderwater && !Parent.IsKnocked)
                {
                    player.MessageDispatcher.ShowMessage(Translations.Get("to_mount", Controls.Interact), .5f, Color.White);
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

            if (HasRider && _rider.IsRiding)
            {
                _canUnride = true;
            }
            else
            {
                _canUnride = false;
            }
            if( HasRider && _rider is LocalPlayer && _shouldUnride || HasRider && _rider.IsDead )
                _rider.IsRiding = false;


            if(HasRider && !_rider.IsRiding)
            {
                Quit();
            }
        }

        private void Ride(IHumanoid Entity)
        {
            if(HasRider || Entity.IsRiding) return;
            
            _rider = Entity;
            _rider.ComponentManager.AddComponentWhile(new SpeedBonusComponent(_rider, -_rider.Speed + 2.25f), () => _rider != null && _rider.IsRiding);
            HasRider = true;
            var model = (QuadrupedModel) Parent.Model;
            _rider.IsRiding = true;
            _rider.Model.RidingOffset = model.Height * Vector3.UnitY * .5f;
            model.AlignWithTerrain = false;
            model.Rider = _rider;
            Parent.Physics.UsePhysics = false;
            Parent.Physics.CollidesWithEntities = false;
            Parent.SearchComponent<DamageComponent>().Immune = true;

            _ai = Parent.SearchComponent<BasicAIComponent>();
            _ai.Enabled = false;
            _healthBar = Parent.SearchComponent<HealthBarComponent>();
            _healthBar.Hide = _rider is LocalPlayer || _healthBar.Hide;
        }

        private void Quit()
        {
            Parent.Position = _rider.Position;
            var model = (QuadrupedModel) Parent.Model;
            model.AlignWithTerrain = true;
            model.Rider = null;
            _rider.IsRiding = false;
            _rider.Model.RidingOffset = Vector3.Zero;
            HasRider = false;
            _ai.Enabled = true;
            _healthBar.Hide = false;


            Parent.Physics.UsePhysics = true;
            Parent.Physics.CollidesWithEntities = true;
            Parent.SearchComponent<DamageComponent>().Immune = false;
        }

        public override void Dispose()
        {
            EventDispatcher.UnregisterKeyDown(this);
            if (!HasRider) return;
            this.Quit();
        }
    }
}
