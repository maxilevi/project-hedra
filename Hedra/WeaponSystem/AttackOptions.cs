using Hedra.EntitySystem;

namespace Hedra.WeaponSystem
{
    public class AttackOptions
    {
        public IEntity[] IgnoreEntities { get; set; } = new IEntity[0];
        public float IdleMovespeed { get; set; } = .25f;
        public float RunMovespeed { get; set; } = 0;
        public float Charge { get; set; } = 1f;
        public float DamageModifier { get; set; } = 1f;

        public static AttackOptions Default { get; } = new AttackOptions();
    }
}