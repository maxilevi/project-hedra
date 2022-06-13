/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 11/07/2016
 * Time: 08:05 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Numerics;
using Hedra.AISystem;
using Hedra.API;
using Hedra.Components;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Numerics;

namespace Hedra.Engine.EntitySystem.BossSystem
{
    /// <summary>
    ///     Description of BossGenerator.
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
            if (Entity.SearchComponent<IGuardAIComponent>() != null)
                Entity.SearchComponent<IGuardAIComponent>().GuardPosition = Position;
            Entity.SearchComponent<ITraverseAIComponent>().GridSize = new Vector2(32, 32);
            var dmgComponent = Entity.SearchComponent<DamageComponent>();
            dmgComponent.XpToGive = XP;
            var healthBarComponent = new BossHealthBarComponent(Entity, Entity.Name);
            Entity.RemoveComponent(Entity.SearchComponent<HealthBarComponent>());
            Entity.Name = healthBarComponent.Name;
            Entity.Physics.CollidesWithStructures = true;
            Entity.AddComponent(healthBarComponent);
        }
        
        public static IEntity CreateBeasthunterBoss(Vector3 Position, int Level, Random Rng)
        {
            const HumanType type = HumanType.BeasthunterSpirit;
            var boss = NPCCreator.SpawnBandit(Position, Level,
                new BanditOptions
                {
                    ModelType = type,
                    Friendly = false,
                    PossibleClasses = Class.Warrior | Class.Rogue | Class.Mage
                });
            boss.Position = Position;
            var template = HumanoidLoader.HumanoidTemplater[type];
            MakeBoss(boss, Position, template.XP);
            boss.BonusHealth = boss.MaxHealth * (1.5f + Rng.NextFloat());
            boss.Health = boss.MaxHealth;
            var currentWeapon = boss.MainWeapon;
            boss.MainWeapon = ItemPool.Grab(new ItemPoolSettings(ItemTier.Unique, currentWeapon.EquipmentType)
            {
                RandomizeTier = false
            });
            return boss;
        }
        
        public static IEntity CreateSkeletonKingBoss(Vector3 Position, int Level, Random Rng)
        {
            var boss = Generate(new[] { MobType.SkeletonKing }, Position, Rng);
            boss.Level = Level;
            boss.Position = Position;
            return boss;
        }
        
        public static IEntity CreateGolemBoss(Vector3 Position, int Level, Random Rng)
        {
            var boss = Generate(new[] { MobType.Golem }, Position, Rng);
            boss.Level = Level;
            boss.Position = Position;
            return boss;
        }
        
        public static IEntity CreateGhostBoss(Vector3 Position, int Level, Random Rng)
        {
            //var boss = Generate(new[] { MobType. }, Position, Rng);
            //boss.Level = Level;
            //boss.Position = Position;
            return null;
        }
    }
}