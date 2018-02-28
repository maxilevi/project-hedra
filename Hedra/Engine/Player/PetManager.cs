/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 27/01/2017
 * Time: 04:55 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Item;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of PetManager.
	/// </summary>
	public class PetManager
	{
		private LocalPlayer Player;
		public Entity MountEntity;
		private InventoryItem PreviousMount;
		private float DeadTimer = 48;
		
		public PetManager(LocalPlayer Player){
			this.Player = Player;
		}
		
		public void Update(){
			if(Networking.NetworkManager.IsConnected) return;
			
			InventoryItem MountItem = Player.Inventory.Items[Inventory.MountHolder];
			if(MountEntity != null){
				MountEntity.Model.Enabled = Player.Model.Enabled;
				if(MountItem != null)
					MountItem.Info = new ItemInfo(MountItem.Info.MaterialType, MountEntity.MaxHealth);			
			}
			
			if(MountItem != null && MountItem.Type == ItemType.Mount && ( (MountEntity != null && MountEntity.IsDead) || PreviousMount != MountItem || (MountEntity.BlockPosition.Xz - Player.BlockPosition.Xz).LengthSquared > 192*192) && !Player.IsRiding){
				if(MountEntity != null && MountEntity.IsDead){
					DeadTimer -= Time.ScaledFrameTimeSeconds;
					if(DeadTimer > 0)
						return;
				}
				DeadTimer = 4;//60
				if(MountEntity != null){
					if(MountEntity.IsDead)
						MountItem.Info = new ItemInfo(MountItem.Info.MaterialType, MountEntity.MaxHealth);
					MountEntity.Dispose();				
				}
				
				if(MountItem.Info.MaterialType == Material.HorseMount)
					MountEntity = World.SpawnMob(MobType.Horse, Player.BlockPosition + Vector3.UnitX * 12f, new Random(MountItem.Info.ModelSeed));
				
				else if(MountItem.Info.MaterialType == Material.WolfMount)
					MountEntity = World.SpawnMob(MobType.Wolf, Player.BlockPosition + Vector3.UnitX * 12f, new Random(MountItem.Info.ModelSeed));

			    MountEntity.SearchComponent<DamageComponent>().Immune = true;
			    MountEntity.Health = MountEntity.MaxHealth;

                var AIType = MountAIType.WOLF;
				if(MountItem.Info.MaterialType == Material.HorseMount) AIType = MountAIType.HORSE;
				if(MountItem.Info.MaterialType == Material.WolfMount) AIType = MountAIType.WOLF;
				
				MountEntity.Level = 1;
                MountEntity.RemoveComponent( MountEntity.SearchComponent<HealthBarComponent>() );
				MountEntity.AddComponent( new HealthBarComponent(MountEntity, "Mount") );
				MountEntity.SearchComponent<HealthBarComponent>().DistanceFromBase = 3;
				MountEntity.AddComponent(new MountAIComponent(MountEntity, Player, AIType));
				MountEntity.RemoveComponent( MountEntity.SearchComponent<AIComponent>() );
				MountEntity.Removable = false;
				( (QuadrupedModel) MountEntity.Model).IsMountable = true;
				PreviousMount = MountItem;
				
			}else if (MountItem == null){
				PreviousMount = null;
			    MountEntity?.Dispose();
			    MountEntity = null;
			}
		}
	}
}
