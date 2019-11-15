using System;

namespace Hedra.API
{
    [Flags]
    public enum Class
    {
        Warrior = 1,
        Mage = 2,
        Archer = 4,
        Rogue = 8,
        Druid = 16,
        Assassin = 32,
        Hunter = 64,
        Scout = 128,
        Ninja = 256,
        Necromancer = 512,
        Paladin = 1024,
        Berserker = 2048,    
        None = 4096
    }
}