namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public interface IMinionMastery
    {
        float HealthBonus { get; }
        int SkeletonLevel { get; }
        float AttackResistance { get; }
        float AttackPower { get; }
    }
}