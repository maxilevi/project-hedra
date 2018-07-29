/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 16/09/2016
 * Time: 11:42 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using OpenTK;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.WorldBuilding
{
	/// <summary>
	/// Description of Obelisk.
	/// </summary>
	
	internal class Obelisk : InteractableStructure
	{
	    public override string Message => "INTERACT WITH THE OBELISK";
	    public override int InteractDistance => 32;
        public ObeliskType Type;
		private bool _used;
		
		public override void Interact(LocalPlayer Interactee)
        {
            switch (Type)
			{
			    case ObeliskType.Xp:
			        float xpToGive =  4 * Interactee.Level;
			        Interactee.XP += xpToGive;
			        Interactee.MessageDispatcher.ShowMessage($"YOU EARNED {xpToGive} XP", 2, Bar.Violet.ToColor());
			        break;
			    case ObeliskType.Health:
			        Interactee.Health += 16 * Interactee.Level;
			        Interactee.MessageDispatcher.ShowMessage("YOUR HEALTH FEELS REFRESHED", 2, Bar.Low.ToColor());
			        break;
			    case ObeliskType.Mana:
			        Interactee.Mana += 32 * Interactee.Level;
			        Interactee.MessageDispatcher.ShowMessage("YOUR MANA FEELS REFRESHED", 2, Bar.Blue.ToColor());
			        break;
			    case ObeliskType.Mobs:
			        int count = Utils.Rng.Next(1, 4);
			        for(var i = 0; i < count; i++)
			        {
			            Vector3 desiredPosition = this.Position + new Vector3(Utils.Rng.NextFloat() * 40f * Chunk.BlockSize - 20f * Chunk.BlockSize, 0, Utils.Rng.NextFloat() * 40f * Chunk.BlockSize - 20f * Chunk.BlockSize);
			            desiredPosition = new Vector3(desiredPosition.X, Physics.HeightAtPosition(desiredPosition.X, desiredPosition.Z),desiredPosition.Z);
					
			            World.SpawnMob(MobType.Spider, desiredPosition, Utils.Rng);
			        }
			        break;
			    default:
			        throw new ArgumentOutOfRangeException($"Obelisk type does not exist.");
			}
			
			Sound.SoundManager.PlaySound(Sound.SoundType.NotificationSound, this.Position, false, 1f, 0.6f);
        }
		
		public static Vector4 GetObeliskColor(ObeliskType Type)
        {
			switch(Type){
				case ObeliskType.Health:
					return Bar.Low * .3f;
					
				case ObeliskType.Mana:
					return Bar.Blue * .3f;
				
				case ObeliskType.Xp:
					return Bar.Violet * .3f;
					
				case ObeliskType.Mobs:
					return Bar.Full * .3f;
					
				default: return new Vector4(1,1,1,1);
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
					
				default: return new Vector4(1,1,1,1);
			}
		}
	}
	
	public enum ShrineType
    {
		Obelisk,
		Altar,
		Monolith
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
