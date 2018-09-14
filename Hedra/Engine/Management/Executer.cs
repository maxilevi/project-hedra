/*
 * Author: Zaphyk
 * Date: 13/02/2016
 * Time: 07:04 p.m.
 *
 */
using System;
using System.Collections.Generic;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// A static class which manages invokes on the main thread
	/// </summary>
	public static class Executer
	{
		private static readonly List<KeyValuePair<Action, Action>> Functions = new List<KeyValuePair<Action, Action>>();
	    private static readonly List<KeyValuePair<Action, Action>> StandBy = new List<KeyValuePair<Action, Action>>();
        private static object _lock = new object();
		
		/// <summary>
		/// Executes the give method on the main thread after a frame has passed.
		/// </summary>
	     public static void ExecuteOnMainThread(Action Func)
	     {
            StandBy.Add( new KeyValuePair<Action, Action>(Func, delegate {}) );
	     }
	     
	     public static void ExecuteOnMainThread(Action Func, Action Callback)
	     {
            StandBy.Add( new KeyValuePair<Action, Action>(Func, Callback));
	     }

	    public static void Update()
	    {
		    Functions.AddRange(StandBy.ToArray());
	        for (var i = 0; i < Functions.Count; i++)
	        {
	            Functions[i].Key();
	            Functions[i].Value();
	        }
		    Functions.Clear();
		    StandBy.Clear();
	    }
	}
}
