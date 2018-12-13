using System;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public class TextureBillboard : BaseBillboard
    {
        public bool ShouldDisposeId { get; set; } = true;

        public TextureBillboard(float Lifetime, uint TextureId, Vector3 Position, Vector2 Measurements) 
            : this(Lifetime, TextureId, () => Position, Measurements)
        {       
        }
        
        public TextureBillboard(float Lifetime, uint TextureId, Func<Vector3> Follow, Vector2 Measurements) : base(Lifetime, Follow)
        {
            this.Id = TextureId;
            this.Measurements = Measurements;
        }

        protected override uint Id { get; }

        protected override Vector2 Measurements { get; }

        public override void Dispose()
        {
            base.Dispose();
            if(ShouldDisposeId) Renderer.DeleteTexture(Id);
        }
    }
}