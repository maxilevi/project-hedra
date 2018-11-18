namespace HedraTests.Game
{
    public class ArmorBalanceSheet
    {
        public UniqueBalanceEntry HelmetDefense { get; } = new UniqueBalanceEntry
        {
            Min = 1.5f,
            Max = 2,
        };
        
        public UniqueBalanceEntry HelmetOxygen { get; } = new UniqueBalanceEntry
        {
            Min = 1.5f,
            Max = 2,
        };
        
        public UniqueBalanceEntry ChestplateDefense { get; } = new UniqueBalanceEntry
        {
            Min = 1.5f,
            Max = 2,
        };
        
        public UniqueBalanceEntry PantsDefense { get; } = new UniqueBalanceEntry
        {
            Min = 2.5f,
            Max = 3,
        };
        
        public UniqueBalanceEntry BootsDefense { get; } = new UniqueBalanceEntry
        {
            Min = 3f,
            Max = 3.5f,
        };
        
        public UniqueBalanceEntry BootsSpeed { get; } = new UniqueBalanceEntry
        {
            Min = 3f,
            Max = 3.5f,
        };
    }
}