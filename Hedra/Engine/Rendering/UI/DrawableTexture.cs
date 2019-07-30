using Hedra.Engine.Rendering.Core;

namespace Hedra.Engine.Rendering.UI
{
    public abstract class DrawableTexture
    {
        public bool UseTextureCache { get; set; }
        private uint _id;
        
        public virtual uint TextureId
        {
            get => _id;
            set
            {
                if (_id == value) return;
                /* If its the first time a texture is added, dont try to delete it */
                if(UseTextureCache)
                    TextureRegistry.Remove(_id);
                _id = value;
                if(UseTextureCache)
                    TextureRegistry.Use(_id);
            }
        }    
    }
}