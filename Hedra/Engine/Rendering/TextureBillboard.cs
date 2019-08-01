using System;
using Hedra.Engine.Rendering.Core;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public class TextureBillboard : BaseBillboard
    {
        public TextureBillboard(float Lifetime, uint TextureId, Vector3 Position, Vector2 Measurements) 
            : this(Lifetime, TextureId, () => Position, Measurements)
        {
        }
        
        public TextureBillboard(float Lifetime, uint TextureId, Func<Vector3> Follow, Vector2 Measurements) : base(Lifetime, Follow)
        {
            TextureRegistry.Use(TextureId);
            this.TextureId = TextureId;
            this.Measurements = Measurements;
        }

        protected override Vector2 Measurements { get; }
    }
}