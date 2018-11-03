/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 16/09/2016
 * Time: 11:42 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    /// <summary>
    /// Description of Obelisk.
    /// </summary>
    
    public sealed class Obelisk : InteractableStructure
    {
        public override string Message => "INTERACT";
        public override int InteractDistance => 32;
        public ObeliskType Type { get; set; }
        public HighlightedAreaWrapper AreaWrapper { get; set; }

        public Obelisk(Vector3 Position) : base(Position)
        {
        }

        protected override void Interact(IPlayer Interactee)
        {
            switch (Type)
            {
                case ObeliskType.Xp:
                    const float xpToGive = 4;
                    Interactee.XP += xpToGive;
                    Interactee.MessageDispatcher.ShowMessage($"YOU EARNED {xpToGive} XP", 2, Colors.Violet.ToColor());
                    break;
                case ObeliskType.Health:
                    Interactee.Health = Interactee.MaxHealth;
                    Interactee.MessageDispatcher.ShowMessage("YOUR HEALTH FEELS REFRESHED", 2, Colors.LowHealthRed.ToColor());
                    break;
                case ObeliskType.Mana:
                    Interactee.Mana = Interactee.MaxMana;
                    Interactee.MessageDispatcher.ShowMessage("YOUR MANA FEELS REFRESHED", 2, Colors.LightBlue.ToColor());
                    break;
                case ObeliskType.Stamina:
                    Interactee.Stamina = Interactee.MaxStamina;
                    Interactee.MessageDispatcher.ShowMessage("YOUR STAMINA FEELS REFRESHED", 2, Color.Bisque);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Obelisk type does not exist.");
            }
            
            SoundManager.PlaySound(SoundType.NotificationSound, this.Position, false, 1f, 0.6f);
        }
        
        public static Vector4 GetObeliskColor(ObeliskType Type)
        {
            switch(Type){
                case ObeliskType.Health:
                    return Colors.LowHealthRed * .3f;
                    
                case ObeliskType.Mana:
                    return Colors.LightBlue * .3f;
                
                case ObeliskType.Xp:
                    return Colors.Violet * .3f;
                    
                case ObeliskType.Stamina:
                    return Color.Coral.ToVector4() * .3f;
                    
                default: throw new ArgumentOutOfRangeException($"Obelisk color wasnt found.");
            }
        }
        
        public static Vector4 GetObeliskStoneColor(Random Rng)
        {
            int randomN = Rng.Next(0, 4);
            switch(randomN){
                case 0:
                    return new Vector4(0.145f, 0.165f, 0.180f, 1.000f);
                case 1:
                    return new Vector4(0.404f, 0.404f, 0.412f, 1.000f);
                case 2:
                    return new Vector4(0.561f, 0.416f, 0.345f, 1.000f);
                case 3:
                    return new Vector4(0.792f, 0.796f, 0.812f, 1.000f);
                    
                default: throw new ArgumentOutOfRangeException($"Obelisk color wasnt found.");
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
