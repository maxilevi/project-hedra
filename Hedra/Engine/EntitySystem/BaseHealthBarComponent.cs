using System.Drawing;
using Hedra.Core;
using Hedra.Engine.Core;
using Hedra.Engine.Rendering;
using Hedra.Engine.Windowing;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering;


namespace Hedra.Engine.EntitySystem
{
    public abstract class BaseHealthBarComponent : EntityComponent
    {
        protected static readonly Color Friendly = (Colors.FullHealthGreen * .75f).ToColor();
        protected static readonly Color Hostile = (Colors.LowHealthRed * .75f).ToColor();
        protected static readonly Color Neutral = Color.White;

        protected BaseHealthBarComponent(IEntity Entity) : base(Entity)
        {
        }

        protected static uint BuildTexture(Bitmap Blueprint, Color Paint, string Name)
        {
            return Graphics2D.LoadTexture(new BitmapObject
            {
                Bitmap = Graphics2D.ReplaceColor(
                        Blueprint,
                        Color.FromArgb(255, 255, 255, 255),
                        Paint
                    ),
                    Path = $"UI:Color:HealthBarComponent:{Name}"
                }, TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.ClampToEdge
            );
        }
        
        protected float GetRatio()
        {
            return Parent.Health / Parent.MaxHealth;
        }
    }
}