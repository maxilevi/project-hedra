
using System.Linq;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.PhysicsSystem
{
    public class PhysicsLoadBalancer
    {
        private readonly PhysicsThread[] _threads;
        private readonly PhysicsThread[] _loadArray;

        public PhysicsLoadBalancer(PhysicsThread[] Threads)
        {
            _threads = Threads;
            _loadArray = _threads;
        }

        public void Wakeup()
        {
            for (var i = 0; i < _threads.Length; i++)
            {
                _threads[i].Wakeup();
            }
        }

        public int Count => _threads.Sum( P => P.Count );

        public void Add(Entity Item)
        {
            this.LowestLoadThread().Add(Item);
        }

        public void Add(MoveCommand Item)
        {
            this.LowestLoadThread().Add(Item);    
        }

        private PhysicsThread LowestLoadThread()
        {
            var next = _loadArray[0];
            for (var i = 0; i < _loadArray.Length-1; i++)
            {
                _loadArray[i] = _loadArray[i + 1];
            }
            _loadArray[_loadArray.Length - 1] = next;
            return next;
        }
    }
}
