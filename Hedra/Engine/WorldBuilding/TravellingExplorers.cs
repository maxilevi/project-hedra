using System;
using System.Collections.Generic;
using System.Numerics;
using Hedra.AISystem.Humanoid;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Mission;

namespace Hedra.Engine.WorldBuilding
{
    public class TravellingExplorers
    {
        public static IHumanoid[] Build(Vector3 Position, Random Rng)
        {
            var friendly = Rng.NextBool();
            var count = Rng.Next(1, 4);
            var explorers = new List<IHumanoid>();
            for (var i = 0; i < count; ++i)
            {
                var explorer = NPCCreator.SpawnBandit(Position + Vector3.UnitZ * i * Chunk.BlockSize * 2, 8,
                    new BanditOptions
                    {
                        Friendly = friendly,
                        ModelType = Rng.Next(0, 5) == 1 ? (HumanType?)HumanType.Skeleton : null
                    });
                if (friendly)
                {
                    if (Rng.Next(0, 3) == 1)
                    {
                        var quest = MissionPool.Random(Position);
                        explorer.AddComponent(new QuestGiverComponent(explorer, quest));
                    }
                    else
                    {
                        explorer.AddComponent(new ExplorerThoughtsComponent(explorer));
                        explorer.AddComponent(new TalkComponent(explorer));
                    }

                    explorer.DamageModifiers += (IEntity Victim, ref float Damage) =>
                    {
                        if (Victim.IsFriendly || Victim is LocalPlayer ||
                            Victim == LocalPlayer.Instance.Companion.Entity)
                            Damage = 0;
                    };
                }

                explorer.AddComponent(new DisposeComponent(explorer,
                    Chunk.Width * GeneralSettings.MaxLoadingRadius + Chunk.Width));
                explorers.Add(explorer);
            }

            explorers.ForEach(E => E.SearchComponent<CombatAIComponent>().Behaviour =
                new ExplorerAIBehaviour(E, explorers.ToArray(), Rng));
            return explorers.ToArray();
        }

        public static IHumanoid BuildAbandonedExplorerWithQuest(Vector3 Position, Random Rng)
        {
            var explorer = NPCCreator.SpawnBandit(Position, 8, new BanditOptions
            {
                Friendly = true,
                ModelType = Rng.Next(0, 5) == 1 ? (HumanType?)HumanType.Skeleton : null
            });
            explorer.AddComponent(new QuestGiverComponent(explorer, MissionPool.Random(Position)));
            explorer.RemoveComponent<CombatAIComponent>();
            return explorer;
        }
    }
}