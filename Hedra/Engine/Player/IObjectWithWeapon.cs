namespace Hedra.Engine.Player
{
    public interface IObjectWithWeapon
    {
        bool IsAttacking { set; }
        bool WasAttacking { set; }
        bool InAttackStance { set; }
    }
}