using System;
using System.Collections.Generic;
using System.Text;
using Hedra.Crafting.Templates;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.Templates;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.Scripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hedra.Engine.Player.Inventory
{
    public static class AttributeFormatter
    {
        private const string ArrowCharacter = "->";//➝
        private static readonly Script Script;
        private static readonly Dictionary<string, Type> Codecs;

        static AttributeFormatter()
        {
            Script = Interpreter.GetScript("TextDisplay.py");
            Codecs = new Dictionary<string, Type>
            {
                { CommonAttributes.Ingredients.ToString(), typeof(IngredientsTemplate) },
                { CommonAttributes.EatEffects.ToString(), typeof(EffectTemplate) }
            };
        }

        public static string Format(AttributeTemplate Template)
        {
            if (AttributeDisplay.Bullets.ToString() == Template.Display)
                return
                    $"{Template.Name.AddSpacesToSentence()}:{Get(Template.Display, Template.Value, Template.Name, 4)}";
            return
                $"{Template.Name.AddSpacesToSentence()}   {ArrowCharacter}   {Get(Template.Display, Template.Value, Template.Name, 0)}";
        }

        private static string GetString(object Value, string Name, int Padding)
        {
            if (Value is JArray asJArray)
            {
                var stringBuilder = new StringBuilder();
                foreach (var obj in asJArray)
                    stringBuilder.Append(
                        $"{Environment.NewLine}{BuildPadding(Padding)}{GetString(obj, Name, Padding)}");
                return stringBuilder.ToString();
            }

            if (Value is JObject asJObject)
                return JsonConvert.DeserializeObject(asJObject.ToString(), Codecs[Name]).ToString();
            return Value.ToString();
        }

        private static string Get(string Display, object Value, string Name, int Padding)
        {
            if (Value is double || Value is float)
                return Script.Get("format").Invoke<string>(Value, Display ?? AttributeDisplay.Flat.ToString());

            if (!(Value is int) && !(Value is long)) return GetString(Value, Name, Padding);
            return (int)Convert.ChangeType(Value, typeof(int)) == int.MaxValue ? "∞" : Value.ToString();
        }

        private static string BuildPadding(int Padding)
        {
            return new string(' ', Padding);
        }
    }
}