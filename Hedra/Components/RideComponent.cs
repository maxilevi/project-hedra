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
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Localization;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Components
{
    /// <summary>
    /// Description of RideComponent.
    /// </summary>
    public class RideComponent : EntityComponent
    {
        public const float SpeedMultiplier = 1.75f;
        private IHumanoid _rider;
        private BasicAIComponent _ai;
        private HealthBarComponent _healthBar;
        private bool _hasRider;
        private bool _shouldRide;
        private bool _shouldUnride;
        private bool _canRide;
        private bool _canUnride;
        private readonly float _normalizedHeightOffset;

        public RideComponent(IEntity Parent, float NormalizedHeightOffset) : base(Parent)
        {
            _normalizedHeightOffset = NormalizedHeightOffset;
            EventDispatcher.RegisterKeyDown(this, delegate(object Object, KeyEventArgs EventArgs)
            {
                _shouldRide = EventArgs.Key == Controls.Interact && _canRide;
                _shouldUnride = EventArgs.Key == Controls.Descend && _canUnride;
            });
        }
        
        public override void Update()
        {
            var player = GameManager.Player;
            if (!_hasRider && (player.Position - Parent.Position).LengthSquared() < 12 * 12 && !player.IsRiding &&
                !player.IsCasting
                && Vector3.Dot((Parent.Position - player.Position).NormalizedFast(), player.View.LookingDirection) >
                .6f)
            {
                if (Parent.Model is IMountable model)
                {
                    if (model.IsMountable && !Parent.IsUnderwater && !Parent.IsKnocked)
                    {
                        player.MessageDispatcher.ShowMessage(Translations.Get("to_mount", Controls.Interact), .5f,
                            Color.White);
                        Parent.Model.Tint = new Vector4(2.0f, 2.0f, 2.0f, 1f);
                        _canRide = true;
                        if (_shouldRide) this.Ride(player);
                    }
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

            if (_hasRider && _rider.IsRiding)
            {
                _canUnride = true;
            }
            else
            {
                _canUnride = false;
            }
            if( _hasRider && _rider is LocalPlayer && _shouldUnride || _hasRider && _rider.IsDead )
                _rider.IsRiding = false;


            if(_hasRider && !_rider.IsRiding)
            {
                Quit();
            }
        }

        private void Ride(IHumanoid Entity)
        {
            if(_hasRider || Entity.IsRiding) return;
            
            _rider = Entity;
            _rider.ComponentManager.AddComponentWhile(new SpeedBonusComponent(_rider, -_rider.Speed + Parent.Speed * SpeedMultiplier), () => _rider != null && _rider.IsRiding);
            _hasRider = true;
            var model = (QuadrupedModel) Parent.Model;
            _rider.IsRiding = true;
            _rider.Model.RidingOffset = Vector3.UnitY * (_normalizedHeightOffset * model.Scale.Y - 1.0f);
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
            _hasRider = false;
            _ai.Enabled = true;
            _healthBar.Hide = false;
            
            Parent.Physics.UsePhysics = true;
            Parent.Physics.CollidesWithEntities = true;
            Parent.SearchComponent<DamageComponent>().Immune = false;
        }

        public override void Dispose()
        {
            EventDispatcher.UnregisterKeyDown(this);
            if (!_hasRider) return;
            this.Quit();
        }
    }
}
