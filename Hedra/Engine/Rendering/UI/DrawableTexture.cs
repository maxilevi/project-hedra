namespace Hedra.Engine.Rendering.UI
{
    public abstract class DrawableTexture
    {
        private uint _id;
        
        public virtual uint TextureId
        {
            get => _id;
            set
            {
                if (_id == value) return;
                /* If its the first time a texture is added, dont try to delete it */
                TextureRegistry.Remove(_id);
                _id = value;
                TextureRegistry.Use(_id);
            }
        }    
    }
}