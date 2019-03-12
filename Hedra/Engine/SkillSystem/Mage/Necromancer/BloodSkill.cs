using System;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public abstract class BloodSkill : SingleAnimationSkill
    {
        protected sealed override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/NecromancerSiphonBlood.dae");
        private readonly Timer _timer = new Timer(.1f);
        private bool _canDo;
        
        protected sealed override void OnAnimationMid()
        {
            base.OnAnimationMid();
            var entity = FindNearestVictim();
            if(entity == null) return;
            OnStart(entity);
            SpawnParticle(entity);
        }

        protected sealed override void OnAnimationEnd() => base.OnAnimationEnd();  

        protected sealed override void OnAnimationStart() => base.OnAnimationStart();

        protected abstract void SpawnParticle(IEntity Victim);

        protected void LaunchParticle(IEntity From, IEntity To, Action<IEntity, IEntity> HitLambda)
        {
            var blood = new BloodProjectile(From, To, From.Position + OpenTK.Vector3.UnitY * 3f)
            {
                UsePhysics = false,
                UseLight = true,
                Speed = 1f,
                IgnoreEntities = World.Entities.Where(E => E.SearchComponent<WarriorMinionComponent>()?.Owner == Player || E == Player.Pet.Pet).ToArray()
            };
            blood.LandEventHandler += _ => HitLambda(From, To);
            blood.HitEventHandler += (_, __) => HitLambda(From, To);
            World.AddWorldObject(blood);
        }

        protected abstract void OnStart(IEntity Victim);
        
        private IEntity FindNearestVictim()
        {
            return SkillUtils.GetNearest(Player, Player.LookingDirection, .5f, MaxRadius, E =>
            {
                if (E == Player.Pet.Pet || E.SearchComponent<WarriorMinionComponent>()?.Owner == Player) return false;
                return true;
            });
        }

        public override void Update()
        {
            base.Update();
            if (_timer.Tick())
            {
                _canDo = FindNearestVictim() != null;
            }
        }
        
        protected override bool ShouldDisable => !_canDo;
        protected virtual float MaxRadius => 96;
    }
}