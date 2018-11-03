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
            
            Min50 = 6,
            Max50 = 14,
            
            Min99 = 12,
            Max99 = 16,
        };
        
        public BalanceEntry HumanoidDamageWithNormalWeapons { get; } = new BalanceEntry
        {
            Min1 = 4.5f,
            Max1 = 12,
            
            Min50 = 8,
            Max50 = 14,
            
            Min99 = 11,
            Max99 = 18,
        };
        
        public BalanceEntry HumanoidDamageWithBestWeapons { get; } = new BalanceEntry
        {
            Min1 = 6,
            Max1 = 13,
            
            Min50 = 11,
            Max50 = 18,
            
            Min99 = 14,
            Max99 = 21,
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
            Min1 = 0.55f,
            Max1 = 1.05f,
            
            Min50 = 0.55f,
            Max50 = 1.05f,
            
            Min99 = 0.55f,
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
            
            Min99 = 880,
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