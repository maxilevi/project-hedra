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
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mime;
using System.Runtime.Remoting.Messaging;

namespace Server
{
    /// <summary>
    /// Description of TcpServer.
    /// </summary>
    public class P2PConnector
    {
        //public TcpListener Listener;
        public List<P2PHost> Hosts = new List<P2PHost>();
        public UdpClient Listener;
        private int PreviousHostCount;

        public P2PConnector(IPAddress IP, int Port)
        {
            Listener = new UdpClient(Port);
        }


        public void Run(){
            Log.Write(Program.Logo);
            Log.WriteLine("Starting Up the Project Hedra server");
            Log.WriteLine("Started the P2P Connector on "+ Listener.Client.LocalEndPoint);

            Log.WriteLine("       Host IP       |  Player Count  | Last Ping | UUID | ID |" + Environment.NewLine
                             +"----------------------------------------------------------------");
            while (true)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] Data = Listener.Receive(ref RemoteIpEndPoint);
                this.ProccessPacket(Data, Data.Length, RemoteIpEndPoint);

                this.UpdateHostTable();
            }
        }

        public void ProccessPacket(byte[] Data, int Size, IPEndPoint IP){
            if(Size == 0) return;
            byte PacketID = Data[0];

            switch (PacketID)
            {
                //Keep alive ping - Finished
                case 0x0:
                    P2PHost PingHost = this.SearchHostByIP(IP);
                    if (PingHost == null)
                        break;
                    PingHost.LastPing = 0;
                    Listener.Send(new byte[] {0x0}, 1, IP);
                    break;

                //register - Finished
                case 0x1:
                    if (this.SearchHostByIP(IP) != null)
                        break;

                    P2PHost NewHost = new P2PHost();
                    NewHost.IP = IP;

                    Hosts.Add(NewHost);
                    NewHost.UUID = P2PHost.Identifier(Hosts.Count - 1 + IP.Port, Hosts.Count - 1);
                    byte[] ToSendCode = Encoding.ASCII.GetBytes(NewHost.UUID);
                    Listener.Send(ToSendCode, ToSendCode.Length, IP);
                    break;

                //updatestate - Finished
                case 0x2:
                    P2PHost Host = this.SearchHostByIP(IP);
                    if (Host != null)
                    {
                        Host.PlayerCount = Data[1];
                    }
                    break;

                //Unregister - Finished
                case 0x3:
                    P2PHost OldHost = this.SearchHostByIP(IP);
                    if (OldHost != null)
                    {
                        Hosts.Remove(OldHost);
                    }
                    break;

                //ask for address (join)
                case 0x4:
                    byte[] CodeBytes = new byte[Data.Length - 1];
                    Array.Copy(Data, 1, CodeBytes, 0, CodeBytes.Length);
                    string Code = Encoding.ASCII.GetString(CodeBytes);

                    P2PHost FoundHost = null;
                    for (int i = 0; i < Hosts.Count; i++)
                    {
                        if (Hosts[i].UUID == Code)
                            FoundHost = Hosts[i];
                    }
                    if (FoundHost != null)
                    {
                        byte[] BytesIP = Encoding.ASCII.GetBytes(FoundHost.IP.ToString());
                        Listener.Send(BytesIP.ToArray(), BytesIP.Length, IP);

                        List<byte> NewBytes = new List<byte>();
                        NewBytes.Add(0x1);
                        NewBytes.AddRange(Encoding.ASCII.GetBytes(IP.ToString()));
                        Listener.Send(NewBytes.ToArray(), NewBytes.Count, FoundHost.IP);
                    }
                    break;

            }
        }

        public P2PHost SearchHostByIP(IPEndPoint IP)
        {
            for (int i = 0; i < Hosts.Count; i++)
            {
                if (Hosts[i].IP.ToString() == IP.ToString())
                    return Hosts[i];
            }
            return null;
        }

        public void UpdateHostTable()
        {
            //ffs
            //string[] Lines = Log.Output.Split(Environment.NewLine);
            //for(int i = 0; i < Lines.Length; i++-PreviousHostCount);
            Log.Clear();

            Log.Write(Program.Logo);
            Log.WriteLine("Starting Up the Project Hedra server");
            Log.WriteLine("Started the P2P Connector on "+ Listener.Client.LocalEndPoint);

            Log.WriteLine("       Host IP       |  Player Count  | Last Ping | UUID | ID |" + Environment.NewLine
                          +"----------------------------------------------------------------");

            for (int i = 0; i < Hosts.Count; i++)
            {
                Log.WriteLine(" " + Hosts[i].IP.ToString() + " |      " + Hosts[i].PlayerCount + "/" +6
                                  + "       |    "+Hosts[i].LastPing+" S   | "+Hosts[i].UUID+" | "+i+" |");
            }
            PreviousHostCount = Hosts.Count;
        }

        public bool CanConnect{
            get{ return true; }
        }
    }
}