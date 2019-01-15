using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Hedra.Engine.CraftingSystem.Templates;
using Hedra.Engine.IO;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.Templates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hedra.Engine.Player.Inventory
{
    public static class AttributeFormatter
    {
        private static readonly List<Type> Codecs;

        static AttributeFormatter()
        {
            Codecs = new List<Type>
            {
                typeof(IngredientsTemplate)
            };
        }

        public static string Format(AttributeTemplate Template)
        {
            if (AttributeDisplay.Bullets.ToString() == Template.Display)
            {
                return $"{Template.Name.AddSpacesToSentence()}:{Get(Template.Display, Template.Value, 4)}";
            }
            return $"{Template.Name.AddSpacesToSentence()}   ➝   {Get(Template.Display, Template.Value, 0)}";
        }

        private static string GetString(object Value, int Padding)
        {
            if (Value is JArray asJArray)
            {
                var stringBuilder = new StringBuilder();
                foreach (var obj in asJArray)
                {
                    stringBuilder.Append($"{Environment.NewLine}{BuildPadding(Padding)}{GetString(obj, Padding)}");
                }
                return stringBuilder.ToString();
            } 
            else if (Value is JObject asJObject)
            {
                for (var i = 0; i < Codecs.Count; ++i)
                {
                    try
                    {
                        return JsonConvert.DeserializeObject(asJObject.ToString(), Codecs[i]).ToString();
                    }
                    catch (JsonSerializationException e)
                    {
                        Log.WriteLine(e);
                    }
                }
            }
            return Value.ToString();         
        }
        
        private static string Get(string Display, object Value, int Padding)
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
            if (!(Value is int) && !(Value is long)) return GetString(Value, Padding);
            return (int)Convert.ChangeType(Value, typeof(int)) == int.MaxValue ? "∞" : Value.ToString();
        }

        private static string BuildPadding(int Padding)
        {
            return new string(' ', Padding);
        }
    }
}