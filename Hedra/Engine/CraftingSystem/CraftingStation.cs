using System;

namespace Hedra.Engine.CraftingSystem
{
    [Flags]
    public enum CraftingStation
    {        
        None = 0,
        Anvil = 1,
        Campfire = 2,
        Well = 4,
        Workbench = 8,
        Loom = 16,
        Water = 32,
    }
}