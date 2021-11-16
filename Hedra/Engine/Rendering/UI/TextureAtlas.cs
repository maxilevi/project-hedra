using System.IO;

namespace Hedra.Engine.Rendering.UI
{
    public class TextureAtlas
    {
        public static TextureAtlas[] LoadFromFolder(string Folder)
        {
            var atlas = Directory.GetFiles(Folder);
            //var factory = JsonConvert.DeserializeObject<T>(Data);
            return null;
        }
    }
}