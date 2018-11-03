namespace HedraTests.Game
{
    public class ItemBalanceSheet
    {
        public UniqueBalanceEntry BowDamage { get; } = new UniqueBalanceEntry
        {
            Min = 1,
            Max = 2,
        };
        
        public UniqueBalanceEntry KnifeDamage { get; } = new UniqueBalanceEntry
        {
            Min = 1.5f,
            Max = 2,
        };
        
        public UniqueBalanceEntry SwordDamage { get; } = new UniqueBalanceEntry
        {
            Min = 2,
            Max = 3,
        };
        
        public UniqueBalanceEntry AxeDamage { get; } = new UniqueBalanceEntry
        {
            Min = 2.5f,
            Max = 3.5f,
        };
        
        public UniqueBalanceEntry HammerDamage { get; } = new UniqueBalanceEntry
        {
            Min = 2.33f,
            Max = 3.33f,
        };
        
        public UniqueBalanceEntry KatarDamage { get; } = new UniqueBalanceEntry
        {
            Min = 1,
            Max = 1.35f,
        };
        
        public UniqueBalanceEntry BladesDamage { get; } = new UniqueBalanceEntry
        {
            Min = 1,
            Max = 1.3f,
        };
        
        public UniqueBalanceEntry ClawDamage { get; } = new UniqueBalanceEntry
        {
            Min = 1,
            Max = 1.5f,
        };
        
        public UniqueBalanceEntry StaffDamage { get; } = new UniqueBalanceEntry
        {
            Min = 1,
            Max = 3,
        };
        
        public UniqueBalanceEntry ItemAttackSpeed { get; } = new UniqueBalanceEntry
        {
            Min = 1,
            Max = 3,
        };
    }
}