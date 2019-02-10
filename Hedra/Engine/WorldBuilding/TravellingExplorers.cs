using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.AISystem.Humanoid;
using Hedra.Components;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public class TravellingExplorers
    {
        public static void Build(Vector3 Position, Random Rng)
        {
            var friendly = true;//Rng.Next(0, 3) != 1;
            var count = Rng.Next(1, 4);
            var explorers = new List<IHumanoid>();
            for (var i = 0; i < count; ++i)
            {
                var explorer = World.WorldBuilding.SpawnBandit(Position + Vector3.UnitZ * i * Chunk.BlockSize * 2, 8, friendly, Rng.Next(0, 5) == 1);
                if (friendly)
                {
                    explorer.AddComponent(new ExplorerThoughtsComponent(explorer));
                    explorer.AddComponent(new TalkComponent(explorer));
                }

                explorers.Add(explorer);
            }

            explorers.ForEach(E => E.SearchComponent<CombatAIComponent>().Behaviour = 
                new ExplorerAIBehaviour(E, explorers.ToArray(), Rng.Next(int.MinValue, int.MaxValue)));
        }
    }
}