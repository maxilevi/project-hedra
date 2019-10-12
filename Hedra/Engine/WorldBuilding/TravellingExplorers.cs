using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.AISystem.Humanoid;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.QuestSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Mission;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.WorldBuilding
{
    public class TravellingExplorers
    {
        public static void Build(Vector3 Position, Random Rng)
        {
            var friendly = Rng.NextBool();
            var count = Rng.Next(1, 4);
            var explorers = new List<IHumanoid>();
            for (var i = 0; i < count; ++i)
            {
                var explorer = World.WorldBuilding.SpawnBandit(Position + Vector3.UnitZ * i * Chunk.BlockSize * 2, 8, friendly, Rng.Next(0, 5) == 1);
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
                }
                explorer.AddComponent(new DisposeComponent(explorer, Chunk.Width * GeneralSettings.MaxLoadingRadius + Chunk.Width));
                explorers.Add(explorer);
            }

            explorers.ForEach(E => E.SearchComponent<CombatAIComponent>().Behaviour = 
                new ExplorerAIBehaviour(E, explorers.ToArray(), Rng));
        }
    }
}