using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine.CraftingSystem.Templates;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.EntitySystem;
using Hedra.Items;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hedra.Engine.ItemSystem.FoodSystem
{
    public class FoodHandler
    {
        public static void ApplyEffects(Item Food, IHumanoid Humanoid)
        {
            if (!Food.HasAttribute(CommonAttributes.EatEffects)) return;
            var effects = ParseEffects(Food);
            for (var i = 0; i < effects.Length; ++i)
            {
                if (Utils.Rng.NextFloat() < effects[i].Chance / 100)
                    Humanoid.AddComponent(EffectFactory.Instance.Build(effects[i], Humanoid));
            }
        }

        private static EffectTemplate[] ParseEffects(Item Food)
        {
            var asJArray = Food.GetAttribute<JArray>(CommonAttributes.EatEffects);
            var list = new List<EffectTemplate>();
            foreach (var jObject in asJArray)
            {
                list.Add(JsonConvert.DeserializeObject<EffectTemplate>(jObject.ToString()));
            }
            return list.ToArray();
        }
    }
}