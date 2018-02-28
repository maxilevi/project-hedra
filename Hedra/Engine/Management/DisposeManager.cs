/*
 * Author: Zaphyk
 * Date: 11/02/2016
 * Time: 10:07 p.m.
 *
 */
using System;
using System.Collections.Generic;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of CleanupManager.
	/// </summary>
	public static class DisposeManager
	{
		private static List<IDisposable> DisposeFunctions = new List<IDisposable>();
		
		public static void Add(IDisposable c){
			DisposeFunctions.Add(c);
		}
		
		public static void Remove(IDisposable c){
			DisposeFunctions.Remove(c);
		}
		
		public static void DisposeAll(){
			for(int i = 0; i<DisposeFunctions.Count; i++){
				DisposeFunctions[i].Dispose();
			}
		}
	}
}
