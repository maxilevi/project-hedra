using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using OpenTK;

namespace Hedra.Engine.ItemSystem
{
    public class EffectDescriber
    {
        public EffectType Type { get; set; }
        public Vector4 EffectColor { get; set; }

        public static EffectDescriber FromItem(Item Item)
        {
            return new EffectDescriber
            {
                Type = EffectDescriber.EffectTypeFromItem(Item),
                EffectColor = EffectDescriber.EffectColorFromItem(Item),
            };
        }

        private static EffectType EffectTypeFromItem(Item Item)
        {
            return Item.HasAttribute(CommonAttributes.EffectType)
                ? (EffectType)Enum.Parse(typeof(EffectType), Item.GetAttribute<string>(CommonAttributes.EffectType))
                : EffectType.None;
        }

        public static Vector4 EffectColorFromItem(Item Item)
        {
            return EffectColorFromItem(EffectTypeFromItem(Item));
        }

        private static Vector4 EffectColorFromItem(EffectType Type)
        {
            switch (Type)
            {
                case EffectType.Fire: return Colors.FromArgb(255, 255, 82, 2) * 1.5f;
                case EffectType.Bleed: return Colors.FromArgb(255, 168, 3, 3);
                case EffectType.Freeze: return Colors.FromArgb(255, 145, 191, 255) * 2f;
                case EffectType.Poison: return Colors.FromArgb(255, 94, 165, 103) * 1.75f;
                case EffectType.Slow: return Colors.FromArgb(255, 128, 128, 128);
                case EffectType.Speed: return Vector4.One * 2f;
                default: return Vector4.Zero;
            }
        }
    }
}
