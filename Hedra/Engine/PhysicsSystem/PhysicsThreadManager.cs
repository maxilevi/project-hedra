/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/09/2017
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.PhysicsSystem
{
    /// <summary>
    /// Description of PhysicsThreadManager.
    /// </summary>
    internal delegate void OnCommandProcessedEventHandler(MoveCommand Command);
    internal delegate void OnBatchProcessedEventHandler();

    internal class PhysicsThreadManager
	{
	    public const int ThreadCount = 2;
	    public OnCommandProcessedEventHandler OnCommandProcessedEvent;
	    public OnBatchProcessedEventHandler OnBatchProcessedEvent;
        private PhysicsLoadBalancer _loadBalancer;
	    private PhysicsThread[] _threads;
		private bool _sleep = true;
		
		public void Load()
        {
            if(ThreadCount % 2 != 0) throw new ArgumentOutOfRangeException($"Physics thread count is {ThreadCount} and it should be a multiple of 2");
            _threads = new PhysicsThread[ThreadCount];
            for (var i = 0; i < ThreadCount; i++)
		    {
		        _threads[i] = new PhysicsThread((PhysicsThreadType) i);
		        _threads[i].OnBatchProcessedEvent += () => OnBatchProcessedEvent?.Invoke();
                _threads[i].OnCommandProcessedEvent += M => OnCommandProcessedEvent?.Invoke(M);
		    }
		    _loadBalancer = new PhysicsLoadBalancer(_threads);
        }
		
		public void AddCommand(Entity Member)
        {
		    _loadBalancer.Add(Member);
		}
		
		public void AddCommand(MoveCommand Member)
        {
		    _loadBalancer.Add(Member);
		}
		
		public void Update()
		{
		    _loadBalancer.Wakeup();
		}

	    public int Count => _loadBalancer.Count;
	}
}
