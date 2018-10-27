/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/06/2016
 * Time: 09:10 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Server
{
    /// <summary>
    /// Description of TcpServer.
    /// </summary>
    public class UdpServer
    {
        //public TcpListener Listener;
        public Dictionary<byte,IPEndPoint> Clients = new Dictionary<byte,IPEndPoint>();
        public UdpClient Listener;
        
        public UdpServer(IPAddress IP, int Port)
        {
            Listener = new UdpClient(Port);//new IPEndPoint(IP,Port));
            //Listener = new TcpListener(IP, Port);
            
        }
        
        public void Run(){
            Console.WriteLine("Starting Up the Project Hedra server");
            Console.WriteLine("Started server on "+ Listener.Client.LocalEndPoint + " with Seed "+ Program.ServerCfg.Seed);
            while (true)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] Data = Listener.Receive(ref RemoteIpEndPoint);
                this.ProccessPacket(Data, Data.Length, RemoteIpEndPoint);
            }
        }
        
        public void ProccessPacket(byte[] Data, int Size, IPEndPoint IP){
            if(Size == 0) return;
            byte PacketID = Data[0];
            
            switch(PacketID){
                case 0x0:
                    Console.WriteLine("New client connected from IP "+IP.Address);
                    Console.WriteLine(IP.Address + " Request Access, Access = "+CanConnect);
                    byte SenderID = (byte)Clients.Count;
                    
                    List<byte> Bytes = new List<byte>();
                    
                    Bytes.Add(SenderID);
                    Bytes.Add(Convert.ToByte(CanConnect));
                    Bytes.AddRange(Program.ServerCfg.Seed.ToByteArray());

                    if(CanConnect)
                        Clients.Add(SenderID,IP);
                    
                    Listener.Send(Bytes.ToArray(), Bytes.Count, IP);
                    break;
                    
                case 0x1:
                    byte[] Msg = new byte[ (int) Math.Max(Size-2,0) ];
                    Array.Copy(Data,2,Msg,0, Math.Max(Size-2,0) );
                    Console.WriteLine("Broadcasting Message: "+Encoding.ASCII.GetString(Msg));
                    this.Broadcast(Encoding.ASCII.GetString(Msg));
                    break;
                    
                case 0x2:
                    Console.WriteLine("Received Packet of ID "+PacketID+" From "+IP + " With a Size of "+ Size+" bytes.");
                    this.Broadcast(Data, Data[1]);
                    break;
                    
                case 0x3:
                    Console.WriteLine("Received Packet of ID "+PacketID+" From "+IP + " With a Size of "+ Size+" bytes.");
                    this.Broadcast(Data, Data[1]);
                    break;
                    
                case 0x4:
                    List<byte> NameData = new List<byte>();
                    byte[] NameBytes = Encoding.ASCII.GetBytes(Program.ServerCfg.ServerName);
                    
                    NameData.Add(0x4);
                    NameData.AddRange(NameBytes);
                    
                    Listener.Send(NameData.ToArray(), NameData.Count, IP);
                    break;
                    
                default:
                    Console.WriteLine("Received Packet of ID "+PacketID+" From "+IP + " With a Size of "+ Size+" bytes.");
                    this.Broadcast(Data, Data[1]);
                    break;
            }
        }
        
        public void Broadcast(string Message){
            List<byte> ByteList = new List<byte>();
            ByteList.Add(0x1);
            ByteList.AddRange(Encoding.ASCII.GetBytes(Message));
            
            this.Broadcast(ByteList.ToArray(), 255);
        }
        
        public void Broadcast(byte[] Bytes, byte Sender){
            foreach(byte Key in Clients.Keys){
                if(Key != Sender)
                    Listener.Send(Bytes, Bytes.Length, Clients[Key]);
            }
        }
        
        
        public bool CanConnect{
            get{ return true; }
        }
    }
}