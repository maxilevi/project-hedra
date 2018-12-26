using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Hedra.Engine.CraftingSystem.Templates;
using Hedra.Engine.IO;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.Templates;
using Hedra.Engine.ModuleSystem.Templates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hedra.Engine.Player.Inventory
{
    public static class AttributeFormatter
    {
        private static readonly Dictionary<string, Type> Codecs;

        static AttributeFormatter()
        {
            Codecs = new Dictionary<string, Type>
            {
                {CommonAttributes.Ingredients.ToString(), typeof(IngredientsTemplate)},
                {CommonAttributes.EatEffects.ToString(), typeof(EffectTemplate)}
            };
        }

        public static string Format(AttributeTemplate Template)
        {
            if (AttributeDisplay.Bullets.ToString() == Template.Display)
            {
                return $"{Template.Name.AddSpacesToSentence()}:{Get(Template.Display, Template.Value, Template.Name, 4)}";
            }
            return $"{Template.Name.AddSpacesToSentence()}   ➝   {Get(Template.Display, Template.Value, Template.Name, 0)}";
        }

        private static string GetString(object Value, string Name, int Padding)
        {
            if (Value is JArray asJArray)
            {
                var stringBuilder = new StringBuilder();
                foreach (var obj in asJArray)
                {
                    stringBuilder.Append($"{Environment.NewLine}{BuildPadding(Padding)}{GetString(obj, Name, Padding)}");
                }
                return stringBuilder.ToString();
            } 
            if (Value is JObject asJObject)
            {
                return JsonConvert.DeserializeObject(asJObject.ToString(), Codecs[Name]).ToString();          
            }
            return Value.ToString();         
        }
        
        private static string Get(string Display, object Value, string Name, int Padding)
        {
            if (Value is double || Value is float)
            {
                var asNumber = (float) Convert.ChangeType(Value, typeof(float));
                if (Display == null) return asNumber.ToString("0.00", CultureInfo.InvariantCulture);
                switch ((AttributeDisplay) Enum.Parse(typeof(AttributeDisplay), Display))
                {
                    case AttributeDisplay.Percentage:
                        return $"{(asNumber > 0 ? "+" : asNumber == 0 ? string.Empty : "-")}{ (int) (asNumber * 100f)}%";
                    case AttributeDisplay.Flat:
                        return asNumber.ToString("0.00", CultureInfo.InvariantCulture);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (!(Value is int) && !(Value is long)) return GetString(Value, Name, Padding);
            return (int)Convert.ChangeType(Value, typeof(int)) == int.MaxValue ? "∞" : Value.ToString();
        }

        private static string BuildPadding(int Padding)
        {
            return new string(' ', Padding);
        }
    }
}