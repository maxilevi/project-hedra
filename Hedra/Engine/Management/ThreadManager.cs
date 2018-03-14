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
	public static class ThreadManager
	{
		private static readonly List<KeyValuePair<Action, Action>> Functions = new List<KeyValuePair<Action, Action>>();
		
		/// <summary>
		/// Executes the give method on the main thread after a frame has passed.
		/// </summary>
	     public static void ExecuteOnMainThread(Action func)
	     {
	     	lock(Functions){
	     		Functions.Add( new KeyValuePair<Action, Action>(func, NullCallBack) );
	     	}
	     }
	     
	      public static void ExecuteOnMainThread(Action func, Action Callback)
	     {
	     	lock(Functions){
	      		Functions.Add( new KeyValuePair<Action, Action>(func, Callback));
	     	}
	     }
	      
	     private static void NullCallBack(){}

	    public static void Update()
	    {
	        lock (Functions)
	        {
	            for (var i = 0; i < Functions.Count; i++)
	            {
	                Functions[i].Key();
	                Functions[i].Value();
	            }
	            Functions.Clear();
	        }
	    }
	}
}
