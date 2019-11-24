/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 11/07/2016
 * Time: 08:05 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.AISystem;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Engine.ModuleSystem;
using Hedra.Numerics;

namespace Hedra.Engine.EntitySystem.BossSystem
{
    /// <summary>
    /// Description of BossGenerator.
    /// </summary>
    public static class BossGenerator
    {
        public static IEntity Generate(MobType[] PossibleTypes, Vector3 Position, Random Rng)
        {
            var type = PossibleTypes[Rng.Next(0, PossibleTypes.Length)];
            var boss = World.SpawnMob(type, Vector3.Zero, Rng);
            boss.Position = Position;
            var template = World.MobFactory.GetFactory(type.ToString());
            MakeBoss(boss, Position, template.XP);
            return boss;
        }

        public static void MakeBoss(IEntity Entity, Vector3 Position, float XP)
        {
            if(Entity.SearchComponent<IGuardAIComponent>() != null)
                Entity.SearchComponent<IGuardAIComponent>().GuardPosition = Position;
            Entity.SearchComponent<ITraverseAIComponent>().GridSize = new Vector2(32, 32);
            var dmgComponent = Entity.SearchComponent<DamageComponent>();
            dmgComponent.XpToGive = XP;
            var healthBarComponent = new BossHealthBarComponent(Entity, Entity.Name);
            Entity.RemoveComponent(Entity.SearchComponent<HealthBarComponent>());
            Entity.AddComponent(new BossXPMessageComponent(Entity));
            Entity.Name = healthBarComponent.Name;
            Entity.Physics.CollidesWithStructures = true;
            Entity.AddComponent(healthBarComponent); 
        }
    }
}
