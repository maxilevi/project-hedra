/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/09/2017
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Threading;
using Hedra.Engine.EntitySystem;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
	/// <summary>
	/// Description of PhysicsThreadManager.
	/// </summary>
	public class PhysicsThreadManager
	{
	    public const int ThreadCount = 2;
	    private PhysicsLoadBalancer _loadBalancer;
	    private PhysicsThread[] _threads;
		private bool _sleep = true;
		
		public void Load(){
            _threads = new PhysicsThread[ThreadCount];
            for (var i = 0; i < ThreadCount; i++)
		    {
		        _threads[i] = new PhysicsThread();
		    }
		    _loadBalancer = new PhysicsLoadBalancer(_threads);
        }
		
		public void AddCommand(Entity Member){
		    _loadBalancer.Add(Member);
		}
		
		public void AddCommand(MoveCommand Member){
		    _loadBalancer.Add(Member);
		}
		
		public void Update()
		{
		    _loadBalancer.Wakeup();
		}

	    public int Count => _loadBalancer.Count;
	}
}
