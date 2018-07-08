/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/07/2016
 * Time: 11:26 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Text;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using System.IO;
using System.Collections;
using System.Linq;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.QuestSystem;


namespace Hedra.Engine.Networking
{
	/// <summary>
	/// Description of NetworkManager.
	/// </summary>
	internal static class NetworkManager
	{
		public const int DefaultPort = 23075, DefaultTimeout = 2000;
		public const int PacketRate = 100;//In Milliseconds
		private static UdpClient Socket = new UdpClient();
		private static Dictionary<IPEndPoint, PeerData> Peers = new Dictionary<IPEndPoint, PeerData>();
		public static bool IsConnected;
		public static byte ClientID;
		public static readonly IPEndPoint ConnectorIP;
		public static IPEndPoint AnyIP, ServerIP = null;
		public static bool IsHost = false, CanSend = false;
		public static string JoinCode = string.Empty;
		public static int WorldSeed = -1;
		public static float WorldTime = -1;
		private static Thread ListeningThread, SendingThread;
		
		static NetworkManager(){
			ConnectorIP = Utils.CreateIPEndPoint("192.99.71.242:23075");
		}
		
		
		//KeepAlive packets
		private static void SendHostState(){
			byte[] Data = new byte[]{0x2, (byte) Peers.Count};
			Socket.Send(Data, Data.Length, ConnectorIP);
		}
		
		public static bool Host(){
			try{
				//Registering as host
				NetworkManager.Disconnect();
				IsHost = true;
				
				Socket.Send(new byte[]{0x1}, 1, ConnectorIP);
				
				try{
					Socket.Client.ReceiveTimeout = NetworkManager.DefaultTimeout; 
					byte[] Code = Socket.Receive(ref AnyIP);				
					JoinCode = Encoding.ASCII.GetString(Code);
				}catch(Exception e){
					return false;
				}
				Executer.ExecuteOnMainThread( () => LocalPlayer.Instance.Chat.AddLine("Co-Op Game created using ID "+JoinCode) );

				IsConnected = true;				
				SendingThread = new Thread(Send);
				SendingThread.Start();
				ListeningThread = new Thread(Listen);
				ListeningThread.Start();

				Socket.Client.ReceiveTimeout = 0; 
				return true;
			}catch(Exception e){
				Log.WriteLine( e.ToString() );
				return false;
			}
		}
		
		public static bool Join(string Code){
			try{
				NetworkManager.Disconnect();
				IsConnected = true;
				
				PacketBuilder P0x4 = new PacketBuilder();
				P0x4.ID = 0x4;
				P0x4.Data = Encoding.ASCII.GetBytes(Code);
				
				Socket.Send(P0x4.Bytes, P0x4.Count, ConnectorIP);
				
				try{
					Socket.Client.ReceiveTimeout = NetworkManager.DefaultTimeout;
					IPEndPoint EndPoint = new IPEndPoint(ConnectorIP.Address, ConnectorIP.Port);
					byte[] Buffer = Socket.Receive(ref EndPoint);	
					ServerIP = Utils.CreateIPEndPoint(Encoding.ASCII.GetString(Buffer) );
				}catch(Exception e){
					return false;
				}
				
				PeerData NewPeer = new PeerData();
				NewPeer.Human = PeerData.NewHuman(ServerIP);
				Peers.Add(ServerIP, NewPeer);

				ListeningThread = new Thread(Listen);
				ListeningThread.Start();
				SendingThread = new Thread(Send);
				SendingThread.Start();
				
				Socket.Client.ReceiveTimeout = 0;
				return true;
			}catch(Exception e){
				Log.WriteLine(e.ToString());
				return false;
			}
		}
		
		public static void Listen(){
			BinaryFormatter Formatter = new BinaryFormatter();
			while(IsConnected && Program.GameWindow.Exists){
				try{
					byte[] Buffer = Socket.Receive(ref AnyIP);
					
					//0x2 is KeepAlive
					//if(Buffer[0] != 0x2 && Buffer[0] != 0x0)
					//	Log.WriteLine("Packet of ID "+Buffer[0] + " Received");
					byte PacketID = Buffer[0];
					byte[] Data = new byte[ (int) Math.Max(Buffer.Length-1,0) ];
					Array.Copy(Buffer,1,Data,0, (int) Math.Max(Buffer.Length-1,0));
					
					MemoryStream Ms = null;
					switch(PacketID){
						//Join Code
						case 0x0:
							if(AnyIP.ToString() != ConnectorIP.ToString())
								Peers[AnyIP].LastPing = 0;
							break;
							
						case 0x1://For hosts only
							string Address = Encoding.ASCII.GetString(Data);
							IPEndPoint EndPoint = Utils.CreateIPEndPoint(Address);
							PeerData NewPeer = new PeerData();
							NewPeer.Human = PeerData.NewHuman(ServerIP);
							Peers.Add(EndPoint, NewPeer);
							Executer.ExecuteOnMainThread( () =>  LocalPlayer.Instance.Chat.AddLine(NewPeer.Human.Name+" joined.") );
							SendHostState();
							break;

						case 0x2:
							MemoryStream NewMs = new MemoryStream(ZipManager.UnZipBytes(Data));
							IPEndPoint UseIp = Utils.CreateIPEndPoint(AnyIP.ToString());
							Executer.ExecuteOnMainThread( delegate{ Packet0x2.SetValues(Peers[UseIp].Human, Formatter.Deserialize(NewMs) as Packet0x2 ); NewMs.Dispose();} );
							Socket.Send(new byte[]{0x3}, 1, AnyIP);
							break;
						
						case 0x3:
							Peers[AnyIP].Packet0x2Sent = true;
							break;
							
						case 0x4:
							Ms = new MemoryStream(ZipManager.UnZipBytes(Data));
							Packet0x4.SetValues(Peers[AnyIP].Human, Formatter.Deserialize(Ms) as Packet0x4 );
							break;
							
						case 0x6://World Packet
							Ms = new MemoryStream(ZipManager.UnZipBytes(Data));
							Packet0x6.SetValues(Peers[AnyIP].Human, Formatter.Deserialize(Ms) as Packet0x6 );
							Socket.Send(new byte[]{0x7}, 1, AnyIP);
							break;
						
						case 0x7:
							Peers[AnyIP].Packet0x6Sent = true;
							break;
							
						case 0x8:
							Ms = new MemoryStream(ZipManager.UnZipBytes(Data));
							Packet0x8.SetValues(Peers[AnyIP].Human, Formatter.Deserialize(Ms) as Packet0x8 );
							break;
						case 0x9:
							Ms = new MemoryStream(ZipManager.UnZipBytes(Data));
							Executer.ExecuteOnMainThread( () => LocalPlayer.Instance.Chat.AddLine(Encoding.ASCII.GetString	(Ms.ToArray())) );
							break;
						case 0x10:
							MemoryStream Ms0x10 = new MemoryStream(ZipManager.UnZipBytes(Data));
							Packet0x10.SetValues(Peers[AnyIP].Human, Formatter.Deserialize(Ms0x10) as Packet0x10);
							Ms0x10.Dispose();
							break;
						case 0x11:
							Peers[AnyIP].LastPing = 45; // Disconnect him!
							break;
						case 0x13:
							Ms = new MemoryStream(ZipManager.UnZipBytes(Data));
							Packet0x13.SetValues(Peers[AnyIP].Human, Formatter.Deserialize(Ms) as Packet0x13, AnyIP);
							break;
						case 0x14:
							Ms = new MemoryStream(ZipManager.UnZipBytes(Data));
							Packet0x14.SetValues(LocalPlayer.Instance, Formatter.Deserialize(Ms) as Packet0x14);
							break;
						case 0x15:
							Ms = new MemoryStream(ZipManager.UnZipBytes(Data));
							Packet0x15.SetValues(LocalPlayer.Instance, Formatter.Deserialize(Ms) as Packet0x15);
							break;
						case 0x18:
							string Type = Encoding.ASCII.GetString(ZipManager.UnZipBytes(Data));
							if(World.QuestManager.Quest.GetType().ToString() == Type)
								Executer.ExecuteOnMainThread( () => World.QuestManager.Quest.NextObjective() );
							break;
						default:
							break;
					
					}
					if(Ms != null) Ms.Dispose();
					
				}catch(Exception e){
					if(e.GetType() == typeof(ThreadAbortException))
						return;
		     		Log.WriteLine(e.ToString());
		     	}
			}
		}
		
		public static void Send(){
			BinaryFormatter Formatter = new BinaryFormatter();
			MemoryStream Ms = new MemoryStream();
			LocalPlayer Player = GameManager.Player;
			int TimePassed = 0, EntityRate = 0, ItemRate = 0;
			
			while(IsConnected && Program.GameWindow.Exists){
				try{
					if(Player == null)
						continue;
					
					Thread.Sleep(NetworkManager.PacketRate);
					TimePassed += NetworkManager.PacketRate;
					EntityRate += NetworkManager.PacketRate;
					ItemRate += NetworkManager.PacketRate;
					foreach(IPEndPoint Peer in Peers.Keys){
						if(!Peers[Peer].Packet0x2Sent)
							NetworkManager.SendPacket0x2(Formatter, Ms, Peer);	
						
						if(!Peers[Peer].Packet0x6Sent && IsHost)
							NetworkManager.SendPacket0x6(Formatter, Ms, AnyIP);
						
						if(IsHost && EntityRate >= 200){
							NetworkManager.SendPacket0x10(Formatter, Ms, Peer);
							EntityRate = 0;
						}
						if(IsHost && ItemRate >= 1000){
							ItemRate = 0;
						}
						
						NetworkManager.UpdatePacket0x4(Formatter, Ms, Peer);
						NetworkManager.SendPacket0x8(Formatter, Ms, Peer);
					}
					//Ping every 5 seconds
					if(TimePassed >= 5000){
						List<IPEndPoint> DeadPeers = new List<IPEndPoint>();
						                     
						foreach(IPEndPoint Peer in Peers.Keys){
							if(Peers[Peer].LastPing >= 15){
								IPEndPoint PeerIP = Utils.CreateIPEndPoint(Peer.ToString());
								Executer.ExecuteOnMainThread( delegate{
								                                  	LocalPlayer.Instance.Chat.AddLine(Peers[Peer].Human.Name+" disconnected.");
								                                  	Peers[PeerIP].Human.Dispose();
								                                  	Peers.Remove(PeerIP);
								                                  	if(ServerIP.ToString() == PeerIP.ToString())
								                                  		NetworkManager.Disconnect(true);
								                                  } );
							}
							Peers[Peer].LastPing += 5;
							Socket.Send(new byte[]{0x0}, 1, Peer);
						}
						
						for(int i = 0; i < DeadPeers.Count; i++){
							Peers[DeadPeers[i]].Human.Dispose();
							Peers.Remove(DeadPeers[i]);
						}
						for(int i = 0; i < DeadPeers.Count; i++){
							if(DeadPeers[i] == ServerIP)
								Executer.ExecuteOnMainThread( () => NetworkManager.Disconnect(true) );
						}
						DeadPeers.Clear();
						
						Socket.Send(new byte[]{0x0}, 1, ConnectorIP);
						TimePassed = 0;
					}
				}catch(Exception e){
					if(e.GetType() == typeof(ThreadAbortException))
						return;
					Log.WriteLine(e.ToString());
				}
			}
		}
		
		public static void UpdateTime(){
			foreach(IPEndPoint Peer in Peers.Keys){
				Peers[Peer].Packet0x6Sent = false;
			}
		}
		
		public static PeerData PeerDataFromIP(IPEndPoint IP){
			if(Peers.ContainsKey(IP))
				return Peers[IP];
			else
				return null;
		}
		
		public static void RegisterAttack(Entity Victim, float Amount){
			Packet0x13 Packet = Packet0x13.FromEntity(Victim, Amount);
			BinaryFormatter Formatter = new BinaryFormatter();
			using( MemoryStream Ms = new MemoryStream() ){
				Formatter.Serialize(Ms, Packet);
				PacketBuilder P0x13 = new PacketBuilder();
				P0x13.ID = 0x13;
				P0x13.Data = ZipManager.ZipBytes( Ms.ToArray() );
				Socket.Send( P0x13.Bytes, P0x13.Count, ServerIP);
			}
		}
		
		public static void SendPacket0x15(float Damage, IPEndPoint IP){
			BinaryFormatter Formatter = new BinaryFormatter();
			using( MemoryStream Ms = new MemoryStream() ){
				Packet0x15 Packet = Packet0x15.From(Damage);
				Formatter.Serialize(Ms, Packet);
				PacketBuilder P0x15 = new PacketBuilder();
				P0x15.ID = 0x15;
				P0x15.Data = ZipManager.ZipBytes( Ms.ToArray() );
				Socket.Send( P0x15.Bytes, P0x15.Count, ServerIP);
			}
		}
		
		public static void SendPacket0x14(float XP, IPEndPoint IP){
			BinaryFormatter Formatter = new BinaryFormatter();
			using( MemoryStream Ms = new MemoryStream() ){
				Packet0x14 Packet = Packet0x14.From(XP);
				Formatter.Serialize(Ms, Packet);
				
				PacketBuilder P0x14 = new PacketBuilder();
				P0x14.ID = 0x14;
				P0x14.Data = ZipManager.ZipBytes( Ms.ToArray() );
				Socket.Send( P0x14.Bytes, P0x14.Count, IP);
			}
		}
		
		public static void SendPacket0x17(int ItemId){
			BinaryFormatter Formatter = new BinaryFormatter();
			using( MemoryStream Ms = new MemoryStream() ){
				PacketBuilder P0x17 = new PacketBuilder();
				P0x17.ID = 0x17;
				P0x17.Data = ZipManager.ZipBytes( Ms.ToArray() );
				Socket.Send( P0x17.Bytes, P0x17.Count, ServerIP);
			}
		}
		
		public static void SendQuestCompleted(string Type){
			PacketBuilder Packet = new PacketBuilder();
			Packet.ID = 0x18;
			Packet.Data = ZipManager.ZipBytes(Encoding.ASCII.GetBytes(Type));
			foreach(IPEndPoint Peer in Peers.Keys){
				Socket.Send(Packet.Bytes, Packet.Count, Peer);
			}
		}
		
		private static void UpdatePacket0x4(BinaryFormatter Formatter, MemoryStream Ms, IPEndPoint IP){
			if( Packet0x4.PrevPosition != LocalPlayer.Instance.Position || Packet0x4.PrevRotation != LocalPlayer.Instance.Rotation){
				Packet0x4.PrevPosition = LocalPlayer.Instance.Position;
				Packet0x4.PrevRotation = LocalPlayer.Instance.Rotation;
				
				Packet0x4 Packet = Packet0x4.FromHuman(LocalPlayer.Instance);
				
				Ms.Dispose();
				Ms = new MemoryStream();
				Formatter.Serialize(Ms, Packet);
				PacketBuilder Builder = new PacketBuilder();
				Builder.ID = 0x4;
				Builder.Data = ZipManager.ZipBytes(Ms.ToArray());
				
				Socket.Send(Builder.Bytes, Builder.Count, IP);
			}
		}
		
		private static void SendPacket0x10(BinaryFormatter Formatter, MemoryStream Ms, IPEndPoint IP){
			int EstimatedPacketCount = 1;
			int Index = 0;
			for(int i = World.Entities.Count-1; i > -1; i--){
				if(World.Entities[i].MobId != 0){
					Index++;
					if(Index >= Packet0x10.EntityRate){
						Index = 0;
						EstimatedPacketCount++;
					}
				}
			}
			
			for(int i = 0; i < EstimatedPacketCount; i++){
				Packet0x10 Packet = Packet0x10.FromHuman(LocalPlayer.Instance, i * Packet0x10.EntityRate);
				
				Ms.Dispose();
				Ms = new MemoryStream();
				Formatter.Serialize(Ms, Packet);
				PacketBuilder Builder = new PacketBuilder();
				Builder.ID = 0x10;
				Builder.Data = ZipManager.ZipBytes(Ms.ToArray());
				
				Socket.Send(Builder.Bytes, Builder.Count, IP);
			}
		}
		
		private static void SendPacket0x8(BinaryFormatter Formatter, MemoryStream Ms, IPEndPoint IP){
			Packet0x8 Packet = Packet0x8.FromHuman(LocalPlayer.Instance);
			
			Ms.Dispose();
			Ms = new MemoryStream();
			Formatter.Serialize(Ms, Packet);
			PacketBuilder Builder = new PacketBuilder();
			Builder.ID = 0x8;
			Builder.Data = ZipManager.ZipBytes(Ms.ToArray());
			
			Socket.Send(Builder.Bytes, Builder.Count, IP);
		}
		
		private static void SendPacket0x6(BinaryFormatter Formatter, MemoryStream Ms, IPEndPoint IP){
			Packet0x6 Packet = Packet0x6.FromHuman(LocalPlayer.Instance);
			
			Ms.Dispose();
			Ms = new MemoryStream();
			Formatter.Serialize(Ms, Packet);
			PacketBuilder Builder = new PacketBuilder();
			Builder.ID = 0x6;
			Builder.Data = ZipManager.ZipBytes(Ms.ToArray());
			
			Socket.Send(Builder.Bytes, Builder.Count, IP);
		}
		
		private static void SendPacket0x2(BinaryFormatter Formatter, MemoryStream Ms, IPEndPoint IP){
			Packet0x2 Packet = Packet0x2.FromHuman(LocalPlayer.Instance);
			
			Ms.Dispose();
			Ms = new MemoryStream();
			Formatter.Serialize(Ms, Packet);
			PacketBuilder Builder = new PacketBuilder();
			Builder.ID = 0x2;
			Builder.Data = ZipManager.ZipBytes(Ms.ToArray());
			
			Socket.Send(Builder.Bytes, Builder.Count, IP);
		}
		
		public static void SendChatMessage(string Message){
			long MessageId = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			PacketBuilder Builder = new PacketBuilder();
			Builder.ID = 0x9;
			Builder.Data = ZipManager.ZipBytes(Encoding.ASCII.GetBytes(Message));
			
			foreach(IPEndPoint Peer in Peers.Keys){
				Socket.Send(Builder.Bytes, Builder.Count, Peer);
			}
		}
		
		public static List<Humanoid> Humans{
			get{
				List<Humanoid> Humans = new List<Humanoid>();
				foreach(PeerData Peer in Peers.Values){
					Humans.Add(Peer.Human);
				}
				return Humans;
			}
		}
		
		public static void Disconnect(bool GotoMenu = false){
			if(ListeningThread != null && SendingThread != null){
				ListeningThread.Abort();
				SendingThread.Abort();
			}
			if(IsHost){
				for(int i = 0; i < 4; i++)
					Socket.Send(new byte[]{0x3}, 1, ConnectorIP);
				//hacky but kind of makes sure it arrives
			}
			foreach(IPEndPoint Peer in Peers.Keys){
				Socket.Send(new byte[]{0x11}, 1, Peer);
			}
			Peers.Clear();
			WorldSeed = -1;
			WorldTime = -1;
			IsHost = false;
			IsConnected = false;
			ServerIP = null;
			CanSend = false;
			JoinCode = string.Empty;
			if(GotoMenu)
				GameManager.LoadMenu();
			
		}
		
		public static void Exit(){
			Socket.Close();
		}
	}
}
