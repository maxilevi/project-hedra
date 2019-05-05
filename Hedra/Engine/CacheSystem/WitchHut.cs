using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class WitchHut : CacheType
    {
        public override CacheItem Type => CacheItem.WitchHut;

        public WitchHut()
        {
            var model = AssetManager.LoadPLYWithLODs("Assets/Env/Structures/WitchHut0.ply", Vector3.One * 4);
            model.Translate(Vector3.UnitY);
            AddModel(model);

            var shapes = AssetManager.LoadCollisionShapes("Assets/Env/Structures/WitchHut0.ply", Vector3.One * 4);
            shapes.ForEach(S => S.Transform(Vector3.UnitY));
            AddShapes(shapes);
        }
    }
}