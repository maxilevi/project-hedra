
using System.Linq;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.PhysicsSystem
{
    public class PhysicsLoadBalancer
    {
        private readonly PhysicsThread[] _threads;
        private readonly PhysicsThread[] _loadCommandArray;
        private readonly PhysicsThread[] _loadUpdateArray;

        public PhysicsLoadBalancer(PhysicsThread[] Threads)
        {
            _threads = Threads;
            _loadCommandArray = _threads.Where(T => T.Type == PhysicsThreadType.ProcessCommand).ToArray();
            _loadUpdateArray = _threads.Where(T => T.Type == PhysicsThreadType.ProcessUpdate).ToArray();
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
            LowestLoadThread(_loadUpdateArray).Add(Item);
        }

        public void Add(MoveCommand Item)
        {
            LowestLoadThread(_loadCommandArray).Add(Item);    
        }

        private static PhysicsThread LowestLoadThread(PhysicsThread[] LoadArray)
        {
            var next = LoadArray[0];
            for (var i = 0; i < LoadArray.Length-1; i++)
            {
                LoadArray[i] = LoadArray[i + 1];
            }
            LoadArray[LoadArray.Length - 1] = next;
            return next;
        }
    }
}
