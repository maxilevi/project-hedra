/*
 * Created by SharpDevelop.
 * User: Miguel Levi
 * Date: 24/09/2016
 * Time: 08:02 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Net;

namespace Server
{
    /// <summary>
    /// Description of Configuration.
    /// </summary>
    public class Configuration
    {
        public string IP;
        public string ServerName;
        public int Ports;
        public int Seed;
        
        public void FromFile(string file){
            string[] Lines = File.ReadAllLines(file);
            
            for(int j = 0; j < Lines.Length; j++){
                string[] Entries = Lines[j].Split('=');
                
                if(Entries.Length % 2 != 0) return;
                
                for(int i = 0; i < Entries.Length; i++){
                    if(i+1 % 2 != 0){
                        string Field = Entries[i].Trim().ToLowerInvariant();
                        if(j == 0)
                            Field = Field.Substring(1, Field.Length-1);
                        switch(Field){
                            case "host":
                                IP = Entries[i+1].Trim();
                                continue;
                                
                            case "ports":
                                Ports = int.Parse(Entries[i+1]);
                                continue;
                                
                            case "servername":
                                ServerName = Entries[i+1].Trim();
                                continue;
                                
                            case "seed":
                                Seed = int.Parse(Entries[i+1].Trim());
                                continue;
                        }
                    }
                }
            }
        }
    }
}