using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;

namespace Hedra.Engine.ModuleSystem
{
    public interface IEnemyFactory
    {
        string Name { get; set; }

        void Apply(Entity Mob);
    }

    internal static class EnemyFactoryExtensions
    {
        public static float DamageFormula(this IEnemyFactory Factory, float Base)
        {
            return Base * World.OverallDifficulty * Math.Max(1, LocalPlayer.Instance.Level * .2f);
        }
    }
}