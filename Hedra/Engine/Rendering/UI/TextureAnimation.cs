using OpenTK;

namespace Hedra.Engine.Rendering.UI
{
    public abstract class TextureAnimation<T>  where T : class, ISimpleTexture
    {
        public float Intensity { get; set; } = 1;
        public bool Active => Handle != null;
        private T _handle;

        public void Update()
        {
            if(Handle == null) return;
            Process(this.Handle, this.State);
        }

        public void Play(T Texture)
        {
            this.Handle = Texture;
        }

        public void Stop()
        {
            this.Handle = null;
        }
        
        protected virtual void Start(T Texture, TextureState State)
        {
        }

        protected abstract void Process(T Texture, TextureState State);

        protected virtual void End(T Texture, TextureState State)
        {
            Texture.Position = State.Position;
            Texture.Scale = State.Scale;
        }

        private TextureState State { get; set; }
        
        private T Handle
        {
            get => _handle;
            set
            {
                if(_handle != null) this.End(_handle, State);
                _handle = value;
                State = _handle != null ? TextureState.From(_handle) : null;
                if(_handle != null) this.Start(_handle, State);
            }
        }
    }

    public class TextureState
    {
        public Vector2 Position { get; private set; }
        public Vector2 Scale { get; private set; }
        public bool Flipped { get; private set; }
        public float Opacity { get; private set; }
        public bool Enabled {get; private set;}
        public Vector4 Tint { get; private set; }
        public bool Grayscale { get; private set; }
        public float Angle { get; private set; }

        public static TextureState From(ISimpleTexture Texture)
        {
            return new TextureState
            {
                Position = Texture.Position,
                Scale = Texture.Scale
            };
        }

        public static TextureState From(GUITexture Texture)
        {
            return new TextureState
            {
                Angle = Texture.Angle,
                Enabled = Texture.Enabled,
                Flipped = Texture.Flipped,
                Grayscale = Texture.Grayscale,
                Opacity = Texture.Opacity,
                Position = Texture.Position,
                Scale = Texture.Scale,
                Tint = Texture.Tint
            };
        }
    }
}