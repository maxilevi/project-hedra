using System.Numerics;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Localization;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class PunchingBag : BaseStructure, IUpdatable
    {
        private readonly IEntity _bag;
        private readonly Vector3 _positi;

        public PunchingBag(Vector3 Position, Box Dimensions) : base(Position)
        {
            var bag = new Entity();
            bag.Model = new DummyModel(bag, Dimensions);
            bag.PlaySpawningAnimation = false;
            bag.MaxHealth = 1000;
            bag.Health = bag.MaxHealth;
            bag.Physics.UsePhysics = false;
            bag.Physics.ContactResponse = false;
            bag.Physics.DisableCollisionIfNoContactResponse = false;
            bag.Position = Position - Vector3.UnitY * bag.Model.Dimensions.Size.Y * .5f;
            bag.Removable = false;
            _positi = bag.Position;

            var damageComponent = new DamageComponent(bag);
            damageComponent.OnDamageEvent += OnDamage;
            damageComponent.MissChance = 0;
            damageComponent.PushOnHit = false;
            bag.AddComponent(damageComponent);

            var healthBarComponent =
                new HealthBarComponent(bag, Translations.Get("punching_bag_name"), HealthBarType.Neutral)
                {
                    Height = bag.Model.Dimensions.Size.Y * 1.5f
                };
            bag.AddComponent(healthBarComponent);

            World.AddEntity(_bag = bag);
            BackgroundUpdater.Add(this);
        }

        public void Update()
        {
            if (_bag.Disposed) return;
            if (_bag.Position.Y < _positi.Y)
            {
                var physx = _bag.Physics;
                var z = 0;
            }
        }

        private void OnDamage(DamageEventArgs Args)
        {
            _bag.Health = _bag.MaxHealth;
        }

        public override void Dispose()
        {
            base.Dispose();
            _bag.Dispose();
            World.RemoveEntity(_bag);
        }
    }
}