using System;
using System.Drawing;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using OpenTK;

namespace HedraTests
{
    public class PlayerMock : Entity, IPlayer
    {
        public Vector3 Position { get; set; }
        public IMessageDispatcher MessageDispatcher { get; }
        public float Health { get; set; }
        public float Mana { get; set; }
        public float XP { get; set; }
        public int Level => 1;        
        public SimpleMessageDispatcherMock MessageMock => MessageDispatcher as SimpleMessageDispatcherMock;

        public PlayerMock()
        {
            MessageDispatcher = new SimpleMessageDispatcherMock();
        }
    }
}