/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 11/07/2016
 * Time: 08:05 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;
using OpenTK;


namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of BossGenerator.
	/// </summary>
	public static class BossGenerator
	{
		public static MobType[] NormalBossType = { MobType.Spider, MobType.Turtle, MobType.Boar, MobType.Rat, MobType.Bee, MobType.Wasp };
		
		public static Entity Generate(Random Rng, out MobType Type){

			LocalPlayer player = SceneManager.Game.LPlayer;

		    var bossType = BossType.NORMAL;
		    Entity boss = null;

            var mobType = NormalBossType[Rng.Next(0, NormalBossType.Length)];
		    boss = World.SpawnMob(mobType, Vector3.Zero, Rng);
		    boss.RemoveComponent(boss.SearchComponent<AIComponent>());

            var dmgComponent = new DamageComponent(boss);
		    var healthBarComponent = new BossHealthBarComponent(boss, NameGenerator.Generate(World.Seed+Rng.Next(0,999999)));
			BossAIComponent aiComponent = null;
			
			dmgComponent.XpToGive = 12f + 2 * player.Level + Rng.NextFloat() * 5;
			dmgComponent.OnDamageEvent += delegate(DamageEventArgs Args) {
			    if (!(Args.Victim.Health <= 0)) return;

			    player.MessageDispatcher.ShowMessage("YOU EARNED "+(int)dmgComponent.XpToGive + " XP!", 3f, Rendering.UI.Bar.Violet.ToColor());
			    healthBarComponent.Enabled = false;
			};

			switch(bossType){
				
				case BossType.NORMAL:

					switch (boss.MobType)
					{
					    case MobType.Boar:
					        aiComponent = new BoarBossAIComponent(boss)
					        {
					            AttackDamage = 7
					        };
					        break;

					    case MobType.Turtle:
					        aiComponent = new TurtleBossAIComponent(boss)
					        {
					            AttackDamage = 7
					        };
					        break;

					    case MobType.Spider:
					    case MobType.Bee:
					        aiComponent = new SpiderBossAIComponent(boss)
					        {
					            AttackDamage = 7
					        };

					        var poison = new PoisonousComponent(boss)
					        {
					            Chance = 30,
					            TotalStrength = player.Level * .35f * 30
					        };
					        boss.AddComponent(poison);
					        break;

					    default:
					        aiComponent = new SpiderBossAIComponent(boss)
					        {
					            AttackDamage = 6
					        };
					        break;
					}
					break;
				
				default:break;
			}

			Type = boss.MobType;
		    var quadModel = boss.Model as QuadrupedModel;

		    if(quadModel != null){
		        float scale = 3;

				if(boss.MobType == MobType.Bee)
					scale = 6;

		        if (boss.MobType == MobType.Rat)
		            scale = 4;

                quadModel.Resize(Vector3.One * scale);
				
				float size = (boss.DefaultBox.Max - boss.DefaultBox.Min).Length+4;
		        aiComponent.Radius = size * size;
		    }

			boss.Level = Math.Max(1, (int) (.65f * player.Level) );
			aiComponent.AttackDamage *= boss.Level; 
			boss.Speed = 3f;
		    boss.Name = healthBarComponent.Name;
			boss.MaxHealth = 500f + Rng.NextFloat() * 80 * 2f -80f +500 * player.Level * .3f;
			boss.Health = boss.MaxHealth;
		    boss.IsBoss = true;
			boss.Physics.CanCollide = true;
			
			boss.AddComponent(dmgComponent);
			boss.AddComponent(healthBarComponent);
			boss.AddComponent(aiComponent);
				
			return boss;
			
		}
		
		
		enum BossType{
			NORMAL,
			HUMAN,
			SNOWMAN,
			MAX_TYPES
		}
	}
}
