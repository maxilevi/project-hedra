namespace HedraTests.Game
{
    public class WeaponBalanceSheet
    {
        public UniqueBalanceEntry BowDamage { get; } = new UniqueBalanceEntry
        {
            Min = 1.5f,
            Max = 2,
        };
        
        public UniqueBalanceEntry KnifeDamage { get; } = new UniqueBalanceEntry
        {
            Min = 1.5f,
            Max = 2,
        };
        
        public UniqueBalanceEntry SwordDamage { get; } = new UniqueBalanceEntry
        {
            Min = 2.5f,
            Max = 3,
        };
        
        public UniqueBalanceEntry AxeDamage { get; } = new UniqueBalanceEntry
        {
            Min = 3f,
            Max = 3.5f,
        };
        
        public UniqueBalanceEntry HammerDamage { get; } = new UniqueBalanceEntry
        {
            Min = 2.75f,
            Max = 3.25f,
        };
        
        public UniqueBalanceEntry KatarDamage { get; } = new UniqueBalanceEntry
        {
            Min = 1.5f,
            Max = 1.85f,
        };
        
        public UniqueBalanceEntry BladesDamage { get; } = new UniqueBalanceEntry
        {
            Min = 1.5f,
            Max = 1.8f,
        };
        
        public UniqueBalanceEntry ClawDamage { get; } = new UniqueBalanceEntry
        {
            Min = 1.5f,
            Max = 2f,
        };
        
        public UniqueBalanceEntry StaffDamage { get; } = new UniqueBalanceEntry
        {
            Min = 1,
            Max = 3,
        };
        
        public UniqueBalanceEntry ItemAttackSpeed { get; } = new UniqueBalanceEntry
        {
            Min = 0.75f,
            Max = 1.5f,
            ScaleWithLevel = false
        };
    }
}