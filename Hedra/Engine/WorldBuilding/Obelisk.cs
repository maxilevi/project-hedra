/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 16/09/2016
 * Time: 11:42 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Player;
using OpenTK;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;

namespace Hedra.Engine.WorldBuilding
{
	/// <summary>
	/// Description of Obelisk.
	/// </summary>
	
	public class Obelisk : InteractableStructure
	{
	    public override string Message => "INTERACT WITH THE OBELISK";
	    public override int InteractDistance => 32;
        public ObeliskType Type { get; set; }
		
		public override void Interact(IPlayer Interactee)
        {
            switch (Type)
			{
			    case ObeliskType.Xp:
			        float xpToGive =  4 * Interactee.Level;
			        Interactee.XP += xpToGive;
			        Interactee.MessageDispatcher.ShowMessage($"YOU EARNED {xpToGive} XP", 2, Colors.Violet.ToColor());
			        break;
			    case ObeliskType.Health:
			        Interactee.Health += 16 * Interactee.Level;
			        Interactee.MessageDispatcher.ShowMessage("YOUR HEALTH FEELS REFRESHED", 2, Colors.LowHealthRed.ToColor());
			        break;
			    case ObeliskType.Mana:
			        Interactee.Mana += 32 * Interactee.Level;
			        Interactee.MessageDispatcher.ShowMessage("YOUR MANA FEELS REFRESHED", 2, Colors.LightBlue.ToColor());
			        break;
			    case ObeliskType.Mobs:
			        int count = Utils.Rng.Next(1, 4);
			        for(var i = 0; i < count; i++)
			        {
			            var desiredPosition = this.Position + new Vector3(Utils.Rng.NextFloat() * 64f * Chunk.BlockSize - 32f * Chunk.BlockSize, 0, Utils.Rng.NextFloat() * 64f * Chunk.BlockSize - 32f * Chunk.BlockSize);
			            desiredPosition = new Vector3(desiredPosition.X, Physics.HeightAtPosition(desiredPosition.X, desiredPosition.Z),desiredPosition.Z);
					
			            World.SpawnMob(MobType.Spider, desiredPosition, Utils.Rng);
			        }
				    Interactee.MessageDispatcher.ShowMessage("BE CAREFUL", 2, Colors.Gray.ToColor());
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
					
				case ObeliskType.Mobs:
					return Colors.FullHealthGreen * .3f;
					
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
	}
	
	public enum ObeliskType
    {
		Xp,
		Health,
		Mana,
		Mobs,
		MaxItems
	}
}
