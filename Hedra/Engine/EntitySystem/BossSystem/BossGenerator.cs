﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 11/07/2016
 * Time: 08:05 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using OpenTK;

namespace Hedra.Engine.EntitySystem.BossSystem
{
	/// <summary>
	/// Description of BossGenerator.
	/// </summary>
	internal static class BossGenerator
	{
		public static Entity Generate(MobType[] PossibleTypes, Random Rng)
		{
		    var type = PossibleTypes[Rng.Next(0, PossibleTypes.Length)];
		    if (type == MobType.Troll)
		    {
		        type = MobType.Gorilla;
		    }
            var boss = World.SpawnMob(type, Vector3.Zero, Rng);
		    boss.MaxHealth *= (float) (Math.Log(GameManager.Player.Level) + 1);
		    boss.Health = boss.MaxHealth;
		    var dmgComponent = boss.SearchComponent<DamageComponent>();
            var healthBarComponent = new BossHealthBarComponent(boss, NameGenerator.Generate(World.Seed + Rng.Next(0, 999999)));
			
            boss.RemoveComponent(boss.SearchComponent<HealthBarComponent>());
		    dmgComponent.XpToGive += (int) (GameManager.Player.Level * .25f);
            dmgComponent.OnDamageEvent += delegate(DamageEventArgs Args) {
			    if (!(Args.Victim.Health <= 0)) return;

			    GameManager.Player.MessageDispatcher.ShowMessage("YOU EARNED "+(int)dmgComponent.XpToGive + " XP!", 3f, Rendering.UI.Bar.Violet.ToColor());
			    healthBarComponent.Enabled = false;
			};

		    boss.Name = healthBarComponent.Name;
		    boss.IsBoss = true;
			boss.Physics.CanCollide = true;
			
			boss.AddComponent(dmgComponent);
			boss.AddComponent(healthBarComponent);
				
			return boss;
			
		}
	}
}