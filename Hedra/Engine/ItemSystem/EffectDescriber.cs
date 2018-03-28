using System;
using Hedra.Engine.Generation;
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

        public static EffectType EffectTypeFromItem(Item Item)
        {
            return Item.HasAttribute(CommonAttributes.EffectType)
                ? (EffectType)Enum.Parse(typeof(EffectType), Item.GetAttribute<string>(CommonAttributes.EffectType))
                : EffectType.None;
        }

        public static Vector4 EffectColorFromItem(Item Item)
        {
            return EffectColorFromItem(EffectTypeFromItem(Item));
        }

        public static Vector4 EffectColorFromItem(EffectType Type)
        {
            switch (Type)
            {
                case EffectType.Fire: return Particle3D.FireColor;
                case EffectType.Bleed: return Particle3D.BloodColor;
                case EffectType.Freeze: return Particle3D.IceColor;
                case EffectType.Poison: return Particle3D.PoisonColor * .5f;
                case EffectType.Slow: return -Particle3D.AshColor * 2f;
                case EffectType.Speed: return Vector4.One*1f;
                default: return Vector4.Zero;
            }
        }
    }
}
