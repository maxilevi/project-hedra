/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/01/2017
 * Time: 02:37 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Hedra.Engine.Networking
{
    /// <summary>
    /// Description of PacketBuilder.
    /// </summary>
    public class PacketBuilder
    {
        public byte ID = 0x0;
        public byte[] Data;
        
        public int Count{
            get{ return 1 + ( (Data != null) ? Data.Length : 0 ); }
        }
        
        public byte[] Bytes{
            get{
                List<byte> BytesList = new List<byte>();
                BytesList.Add(ID);
                BytesList.AddRange(Data);
                return BytesList.ToArray();
            }
        }
    }
}
