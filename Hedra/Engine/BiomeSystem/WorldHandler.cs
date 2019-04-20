using System;
using System.Collections.Generic;

namespace Hedra.Engine.BiomeSystem
{
    public abstract class WorldHandler : IDisposable
    {
        private static readonly Dictionary<WorldType, Type> Map = new Dictionary<WorldType, Type>
        {
            {WorldType.Overworld, typeof(OverworldHandler)},
            {WorldType.GhostTown, typeof(GhostTownHandler)}
        };
        
        public abstract void Update();
        public abstract void Dispose();

        public static WorldHandler Create(WorldType Type)
        {
            return (WorldHandler) Activator.CreateInstance(Map[Type]);
        }
    }
}