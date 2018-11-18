using System;
using System.Collections.Generic;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using NUnit.Framework;

namespace HedraTests.Game
{
    public interface IBalanceTestCompliant
    {
        bool Complies(Item Equipment);
    }
    
    public abstract class BaseEquipmentTest<T> where T : class, IBalanceTestCompliant, new()
    {
        public abstract bool Complies(Item Equipment);
        
        protected static void AssertComplies(Item Item, UniqueBalanceEntry Entry, Func<float> Lambda)
        {
            var multiplier = Entry.ScaleWithLevel ? ((int) Item.Tier) * .5f + 1 : 1;
            var val = Lambda();
            var max = Entry.Max * multiplier;
            var min = Entry.Min * multiplier;
            var msg =
                $"{Item.Name} with rarity '{Item.Tier}' should have {min} < {val} < {max}";
            Assert.GreaterOrEqual(max, val, msg);
            Assert.LessOrEqual(min, val, msg);
            Assert.Pass(msg);
        }

        /* Called via reflection by NUnit */
        protected static IEnumerable<Item> Some(EquipmentType Match)
        {
            var collection = All();
            foreach (var weapon in collection)
            {
                if(weapon.EquipmentType != Match.ToString()) continue;
                yield return weapon;
            }
        }       
        
        /* Called via reflection by NUnit */
        protected static IEnumerable<Item> All()
        {
            var obj = new T();
            AssetManager.Provider = new SimpleAssetProvider();
            ItemFactory.LoadModules(GameLoader.AppPath);
            var templates = ItemFactory.Templater.Templates;
            for (var i = 0; i < templates.Length; i++)
            {
                var newItem = Item.FromTemplate(templates[i]);
                if (!obj.Complies(newItem)) continue;
                yield return newItem;
            }
        }
    }
}