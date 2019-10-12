using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using OpenToolkit.Mathematics;

namespace HedraTests
{
    public class WorldSpawnMock : SimpleWorldProviderMock
    {
        public int MobsSpawned { get; private set; }
        
        public override SkilledAnimableEntity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed)
        {
            MobsSpawned++;
            return base.SpawnMob(Type, DesiredPosition, MobSeed);
        }
    }
}