using Hedra.Engine.ModuleSystem;

namespace HedraTests.Game
{
    public class MobBalanceSheet
    {
        
        public UniqueBalanceEntry MobXp { get; } = new UniqueBalanceEntry
        {
            Min = CustomFactory.MinXpFactor,
            Max = CustomFactory.MaxXpFactor,
        };
        
        public UniqueBalanceEntry MobHealth { get; } = new UniqueBalanceEntry
        {
            Min = 16,
            Max = 26,
        };

        public UniqueBalanceEntry MobAttackCooldown { get; } = new UniqueBalanceEntry
        {
            Min = 1.0f,
            Max = 3.0f,
            ScaleWithLevel = false
        };
        
        public UniqueBalanceEntry MobAttackDamage { get; } = new UniqueBalanceEntry
        {
            Min = .5f,
            Max = 1,
        };
        
        public UniqueBalanceEntry MobSpeed { get; } = new UniqueBalanceEntry
        {
            Min = 1.0f,
            Max = 1.50f,
            ScaleWithLevel = false
        };
    }
}