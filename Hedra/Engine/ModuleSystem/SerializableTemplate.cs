using System;
using Hedra.Engine.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hedra.Engine.ModuleSystem
{
    public class SerializableTemplate<T>
    {
        public static string ToJson(T Template)
        {
            return JObject.FromObject(Template).ToString(Formatting.None);
        }

        public static T FromJson(string Data)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(Data);
            } catch(Exception e)
            {
                Log.WriteLine(e);
                Log.WriteLine($"Failed to parse {Data}");
                return default(T);
            }
        }
    }
}