/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/02/2017
 * Time: 11:58 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Player;
using System.Collections.Generic;
using Hedra.Engine.AISystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;

namespace Hedra.Engine.Networking
{
	/// <summary>
	/// Entities Packet
	/// </summary>
	[Serializable]
	public class Packet0x10
	{
		public const int EntityRate = 16;
		public ushort[] Ids, Seeds;
		public byte[] MobTypes, Animation;
		public float[] Health;
		public Vector3[] Position;
		
		public static Packet0x10 FromHuman(Humanoid Human, int Base){
			Packet0x10 Packet = new Packet0x10();
			
			List<ushort> Ids = new List<ushort>(), Seeds = new List<ushort>(), AttackTargets = new List<ushort>();
			List<byte> MobTypes = new List<byte>(), Animation = new List<byte>();
			List<float> Health = new List<float>();
			List<Vector3> Position = new List<Vector3>();
			
			lock(World.Entities){
				int Count = 0;
				for(int i = World.Entities.Count-1; i > -1; i--){
					if(World.Entities[i].MobId != 0){
						Count++;
						if(Count <= Base) continue;
						
						Ids.Add( (ushort) World.Entities[i].MobId);
						MobTypes.Add( (byte) World.Entities[i].MobType);
						Health.Add( World.Entities[i].Health);

					    BasicAIComponent AI = World.Entities[i].SearchComponent<BasicAIComponent>();
						if( AI != null ){
							//Position.Add( AI.TargetPosition );
							//Animation.Add( (byte) (AI.IsAttacking ? 0x1 : 0x0) );
						}else{
							Animation.Add( 0x0 );
						}
						Position.Add( World.Entities[i].BlockPosition);
						Seeds.Add( (ushort) World.Entities[i].MobSeed);
						
					}
					if( (World.Entities.Count-1 - Base - i) >= Packet0x10.EntityRate)
						break;
				}
			}
			Packet.Ids = Ids.ToArray();
			Packet.MobTypes = MobTypes.ToArray();
			Packet.Health = Health.ToArray();
			Packet.Position = Position.ToArray();
			Packet.Seeds = Seeds.ToArray();
			Packet.Animation = Animation.ToArray();
			return Packet;
		}
		
		public static void SetValues(Humanoid Human, Packet0x10 Packet){
			lock(World.Entities){
				for(int i = 0; i < Packet.Ids.Length; i++){
					bool MobFound = false;
					Entity Mob = null;
					for(int j = World.Entities.Count-1; j > -1; j--){
						if(World.Entities[j].MobId == Packet.Ids[i]){
							MobFound = true;
							Mob = World.Entities[j];
							break;
						}
					}
					if(MobFound){
						//Update
						Mob.Health = Packet.Health[i];
					    BasicAIComponent AI = Mob.SearchComponent<BasicAIComponent>();
						
						if(AI != null){
							//AI.TargetPosition = Packet.Position[i];
							//AI.MoveToPoint(AI.TargetPosition, delegate{} );
							
							if(Packet.Animation[i] == 0x1){
								Mob.SearchComponent<DamageComponent>().Immune = true;
								//Mob.Model.Attack(Mob, 0f); // Attack itself to do the animation
								Mob.SearchComponent<DamageComponent>().Immune = false;
							}
						}
					}else{
						//Create & Update
						Entity MobEnt = World.SpawnMob( (MobType) Packet.MobTypes[i], Packet.Position[i], (int) Packet.Seeds[i]);
						MobEnt.Health = Packet.Health[i];
						MobEnt.MobId = Packet.Ids[i];
						
						if( MobEnt.SearchComponent<DropComponent>() != null )
							MobEnt.RemoveComponent( MobEnt.SearchComponent<DropComponent>() );
						BasicAIComponent AI = MobEnt.SearchComponent<BasicAIComponent>();
						if(AI != null)
							AI.Enabled = false;
					}
				}
				//Remove old entities
				for(int j = World.Entities.Count-1; j > -1; j--){
					if(World.Entities[j].MobId != 0){
						bool Found = false;
						for(int i = 0; i < Packet.Ids.Length; i++){
							if(Packet.Ids[i] == World.Entities[j].MobId){
								Found = true;
								break;
							}
						}
						if(!Found) World.Entities[j].Dispose();
					}
				}
			}
		}
		
		
		
	}
}
