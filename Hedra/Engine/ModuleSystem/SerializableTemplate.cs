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

        public static T FromJSON(string Data)
        {
            return JsonConvert.DeserializeObject<T>(Data);
        }
    }
}