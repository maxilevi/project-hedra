namespace HedraTests.Game
{
    public class HumanoidBalanceSheet
    {
        public BalanceEntry BaseHumanoidDamage { get; } = new BalanceEntry
        {
            Min1 = 2,
            Max1 = 7,
            
            Min50 = 4,
            Max50 = 12,
            
            Min99 = 8,
            Max99 = 16,
        };
        
        public BalanceEntry HumanoidDamageWithCommonWeapons { get; } = new BalanceEntry
        {
            Min1 = 3,
            Max1 = 9,
            
            Min50 = 7,
            Max50 = 15,
            
            Min99 = 12,
            Max99 = 16,
        };
        
        public BalanceEntry HumanoidDamageWithNormalWeapons { get; } = new BalanceEntry
        {
            Min1 = 4.5f,
            Max1 = 12,
            
            Min50 = 10,
            Max50 = 16,
            
            Min99 = 13,
            Max99 = 20,
        };
        
        public BalanceEntry HumanoidDamageWithBestWeapons { get; } = new BalanceEntry
        {
            Min1 = 8,
            Max1 = 15,
            
            Min50 = 12,
            Max50 = 19,
            
            Min99 = 17,
            Max99 = 24,
        };
        
        public BalanceEntry HumanoidSpeed { get; } = new BalanceEntry
        {
            Min1 = 1.25f,
            Max1 = 1.40f,
            
            Min50 = 1.25f,
            Max50 = 1.40f,
            
            Min99 = 1.25f,
            Max99 = 1.40f,
        };
        
        public BalanceEntry HumanoidResistance { get; } = new BalanceEntry
        {
            Min1 = 0.85f,
            Max1 = 1.05f,
            
            Min50 = 0.75f,
            Max50 = 0.90f,
            
            Min99 = 0.65f,
            Max99 = 0.75f,
        };
        
        public BalanceEntry HumanoidAttackSpeed { get; } = new BalanceEntry
        {
            Min1 = 0.525f,
            Max1 = 1.05f,
            
            Min50 = 0.525f,
            Max50 = 1.05f,
            
            Min99 = 0.525f,
            Max99 = 1.05f,
        };
        
        public BalanceEntry HumanoidStamina { get; } = new BalanceEntry
        {
            Min1 = 100,
            Max1 = 125,
            
            Min50 = 100,
            Max50 = 125,
            
            Min99 = 100,
            Max99 = 125,
        };
        
        public BalanceEntry HumanoidHealth { get; } = new BalanceEntry
        {
            Min1 = 90,
            Max1 = 120,
            
            Min50 = 460,
            Max50 = 640,
            
            Min99 = 830,
            Max99 = 1148,
        };
        
        public BalanceEntry HumanoidMana { get; } = new BalanceEntry
        {
            Min1 = 180,
            Max1 = 240,
            
            Min50 = 500,
            Max50 = 900,
            
            Min99 = 800,
            Max99 = 1800,
        };
        
        public BalanceEntry HumanoidXp { get; } = new BalanceEntry
        {
            Min1 = 38,
            Max1 = 50,
            
            Min50 = 500,
            Max50 = 600,
            
            Min99 = 1000,
            Max99 = 1100,
        };
    }
}