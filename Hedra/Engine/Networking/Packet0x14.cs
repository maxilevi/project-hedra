/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 12/02/2017
 * Time: 08:04 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;

namespace Hedra.Engine.Networking
{
    /// <summary>
    /// Description of Packet0x14.
    /// </summary>
    public class Packet0x14
    {
        public float XP;
        
        public static Packet0x14 From(float XP){
            Packet0x14 Packet = new Packet0x14();
            Packet.XP = XP;
            return Packet;
        }
        
        public static void SetValues(Humanoid Human, Packet0x14 Packet){
            Human.XP += Packet.XP;
        }
    }
}
