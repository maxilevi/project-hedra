using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.WeaponSystem
{
    public delegate void OnProjectileEvent(Projectile Arrow);
    
    public abstract class RangedWeapon : Weapon
    {
        public override bool IsMelee => false;
        protected override bool ShouldPlaySound => false;
        protected override bool ContinousAttack => true;
        public event OnProjectileEvent BowModifiers;
        public event OnProjectileEvent Hit;
        public event OnProjectileEvent Miss;
        
        protected RangedWeapon(VertexData MeshData) : base(MeshData)
        {
        }

                
        protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.Mid) return;
            var player = Owner as IPlayer;
            var direction = player?.View.CrossDirection ?? Owner.Orientation;
            Shoot(direction, Options, player?.Pet?.Pet);
        }
        
        public abstract void Shoot(Vector3 Direction, AttackOptions Options, params IEntity[] ToIgnore);
        
        protected void AddModifiers(Projectile ArrowProj)
        {
            BowModifiers?.Invoke(ArrowProj);
            /* This is because some arrows can penetrate through enemies */
            var gotHit = false;
            ArrowProj.LandEventHandler += S =>
            {
                Miss?.Invoke(ArrowProj);
                if(!gotHit)
                    Owner.ProcessHit(false);
            };
            ArrowProj.HitEventHandler += (S,V) =>
            {
                Hit?.Invoke(ArrowProj);
                Owner.ProcessHit(gotHit = true);
            };
        }
    }
}
