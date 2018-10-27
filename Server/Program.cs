/*
 * Author: Zaphyk
 * Date: 25/02/2016
 * Time: 12:17 a.m.
 *
 */
using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Reflection;

namespace Server
{
    class Program
    {
        public static P2PConnector Server;
        public static Configuration ServerCfg = new Configuration();
        public static string AppPath;
        public static Thread LogicThread;
        
        public static void Main(string[] args)
        {
            AppPath =  Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/";
            Server = new P2PConnector(IPAddress.Parse("127.0.0.1"), 23075);
            LogicThread = new Thread(Server.Run);
            LogicThread.Start();

            while (true)
            {
                Thread.Sleep(5000);
                for (int i = Server.Hosts.Count-1; i > -1; i--)
                {
                    Server.Hosts[i].LastPing += 5;
                    if(Server.Hosts[i].LastPing > 15)
                        Server.Hosts.RemoveAt(i);
                }
                Server.UpdateHostTable();
            }
        }
        
        public static string Logo = "" +
            @"  _____           _           _     _    _          _           " + Environment.NewLine +
            @" |  __ \         (_)         | |   | |  | |        | |          " + Environment.NewLine +
            @" | |__) | __ ___  _  ___  ___| |_  | |__| | ___  __| |_ __ __ _ " + Environment.NewLine +
            @" |  ___/ '__/ _ \| |/ _ \/ __| __| |  __  |/ _ \/ _` | '__/ _` |" + Environment.NewLine +
            @" | |   | | | (_) | |  __/ (__| |_  | |  | |  __/ (_| | | | (_| |" + Environment.NewLine +
            @" |_|   |_|  \___/| |\___|\___|\__| |_|  |_|\___|\__,_|_|  \__,_|" + Environment.NewLine +
            @"                _/ |                                            " + Environment.NewLine +
            @"               |__/                                             " + Environment.NewLine;
        
    }
}