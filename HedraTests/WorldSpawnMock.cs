using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using System.Numerics;
using Hedra.Engine.SkillSystem;

namespace HedraTests
{
    public class WorldSpawnMock : SimpleWorldProviderMock
    {
        public int MobsSpawned { get; private set; }
        
        public override ISkilledAnimableEntity SpawnMob(string Type, Vector3 DesiredPosition, int MobSeed)
        {
            MobsSpawned++;
            return base.SpawnMob(Type, DesiredPosition, MobSeed);
        }
    }
}