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
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.EntitySystem.BossSystem
{
    /// <summary>
    /// Description of BossGenerator.
    /// </summary>
    public static class BossGenerator
    {
        public static Entity Generate(MobType[] PossibleTypes, Vector3 Position, Random Rng)
        {
            var type = PossibleTypes[Rng.Next(0, PossibleTypes.Length)];
            if (type == MobType.Troll)
            {
                type = MobType.Gorilla;
            }
            var boss = World.SpawnMob(type, Vector3.Zero, Rng);
            boss.Position = Position;
            boss.SearchComponent<IGuardAIComponent>().GuardPosition = Position;
            var dmgComponent = boss.SearchComponent<DamageComponent>();
            dmgComponent.Immune = true;
            var healthBarComponent = new BossHealthBarComponent(boss, NameGenerator.Generate(World.Seed + Rng.Next(0, 999999)));
            
            boss.RemoveComponent(boss.SearchComponent<HealthBarComponent>());
            dmgComponent.OnDamageEvent += delegate(DamageEventArgs Args)
            {
                if (!(Args.Victim.Health <= 0)) return;

                GameManager.Player.MessageDispatcher.ShowMessage("YOU EARNED "+(int)dmgComponent.XpToGive + " XP!", 3f, Colors.Violet.ToColor());
                healthBarComponent.Enabled = false;
            };
            boss.AddComponent(new SpawnComponent(boss, Position, () => dmgComponent.Immune = false));
            boss.Name = healthBarComponent.Name;
            boss.IsBoss = true;
            boss.Physics.CollidesWithStructures = true;
            boss.Removable = false;
            
            boss.AddComponent(dmgComponent);
            boss.AddComponent(healthBarComponent);
            
            return boss;
            
        }
    }
}
