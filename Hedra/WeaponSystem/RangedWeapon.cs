using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.WeaponSystem
{
    public abstract class RangedWeapon : Weapon
    {
        public override bool IsMelee => false;
        protected override bool ShouldPlaySound => false;
        protected override bool ContinousAttack => true;

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
    }
}
