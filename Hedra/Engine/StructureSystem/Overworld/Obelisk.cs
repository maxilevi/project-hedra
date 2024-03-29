/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 16/09/2016
 * Time: 11:42 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Sound;
using SixLabors.ImageSharp;

namespace Hedra.Engine.StructureSystem.Overworld
{
    /// <summary>
    ///     Description of Obelisk.
    /// </summary>
    public sealed class Obelisk : InteractableStructure
    {
        public Obelisk(Vector3 Position) : base(Position)
        {
        }

        public override string Message => Translations.Get("interact_obelisk");
        public override int InteractDistance => 32;
        public ObeliskType Type { get; set; }
        public HighlightedAreaWrapper AreaWrapper { get; set; }
        protected override bool AllowThroughCollider => true;

        protected override void Interact(IHumanoid Humanoid)
        {
            switch (Type)
            {
                case ObeliskType.Xp:
                    const float xpToGive = 4;
                    Humanoid.XP += xpToGive;
                    Humanoid.MessageDispatcher.ShowMessage(Translations.Get("obelisk_xp", xpToGive), 2,
                        Colors.Violet.ToColor());
                    break;
                case ObeliskType.Health:
                    Humanoid.Health = Humanoid.MaxHealth;
                    Humanoid.MessageDispatcher.ShowMessage(Translations.Get("obelisk_health"), 2,
                        Colors.LowHealthRed.ToColor());
                    break;
                case ObeliskType.Mana:
                    Humanoid.Mana = Humanoid.MaxMana;
                    Humanoid.MessageDispatcher.ShowMessage(Translations.Get("obelisk_mana"), 2,
                        Colors.LightBlue.ToColor());
                    break;
                case ObeliskType.Stamina:
                    Humanoid.Stamina = Humanoid.MaxStamina;
                    Humanoid.MessageDispatcher.ShowMessage(Translations.Get("obelisk_stamina"), 2, Color.Bisque);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Obelisk type does not exist.");
            }

            SoundPlayer.PlaySound(SoundType.NotificationSound, Position, false, 1f, 0.6f);
        }

        public static Vector4 GetObeliskColor(ObeliskType Type)
        {
            switch (Type)
            {
                case ObeliskType.Health:
                    return Colors.LowHealthRed * .3f;

                case ObeliskType.Mana:
                    return Colors.LightBlue * .3f;

                case ObeliskType.Xp:
                    return Colors.Violet * .3f;

                case ObeliskType.Stamina:
                    return Color.Coral.AsVector4() * .3f;

                default: throw new ArgumentOutOfRangeException("Obelisk color wasnt found.");
            }
        }

        public static Vector4 GetObeliskStoneColor(Random Rng)
        {
            var randomN = Rng.Next(0, 4);
            switch (randomN)
            {
                case 0:
                    return new Vector4(0.145f, 0.165f, 0.180f, 1.000f);
                case 1:
                    return new Vector4(0.404f, 0.404f, 0.412f, 1.000f);
                case 2:
                    return new Vector4(0.561f, 0.416f, 0.345f, 1.000f);
                case 3:
                    return new Vector4(0.792f, 0.796f, 0.812f, 1.000f);

                default: throw new ArgumentOutOfRangeException("Obelisk color wasnt found.");
            }
        }

        public override void Dispose()
        {
            AreaWrapper?.Dispose();
            base.Dispose();
        }
    }

    public enum ObeliskType
    {
        Xp,
        Health,
        Stamina,
        Mana,
        MaxItems
    }
}