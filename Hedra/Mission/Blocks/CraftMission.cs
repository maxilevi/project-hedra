using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Crafting;
using Hedra.Engine.ItemSystem;
using Hedra.Localization;

namespace Hedra.Mission.Blocks
{
    public class CraftMission : ItemMission
    {
        public override void Setup()
        {
        }

        public override string ShortDescription => Translations.Get("quest_craft_short", Items.First().ToString().ToUpperInvariant());

        public override string Description
        {
            get
            {
                var arguments = new List<object>(new object[]
                {
                    Giver.Name,
                    Items.Select(I => I.ToString(Owner)).Aggregate((S1, S2) => $"{S1}{Environment.NewLine}{S2}")
                });
                var hasStation = Station != CraftingStation.None;
                if (hasStation) arguments.Add(Translations.Get(Station.ToString().ToLowerInvariant()).ToUpperInvariant());
                return Translations.Get(
                    hasStation
                        ? "quest_craft_description"
                        : "quest_craft_anywhere_description",
                    arguments.ToArray()
                );
            }
        }
        
        public CraftingStation Station { get; set; }
    }
}