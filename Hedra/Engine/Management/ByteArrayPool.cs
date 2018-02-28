/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/09/2017
 * Time: 02:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of ByteArray.
	/// </summary>
	public static class ByteArrayPool
	{
		private static List<ByteArray> Arrays = new List<ByteArray>();
		
		public static byte[] Grab(int size){
			lock(Arrays){
				for(int i = Arrays.Count-1; i > -1; i--){
					if(!Arrays[i].Locked && Arrays[i].Array.Length == size){
						Arrays[i].Locked = true;
						return Arrays[i].Array;
					}
					
				}
				ByteArray Array = new ByteArrayPool.ByteArray();
				Array.Array = new byte[size];
				Arrays.Add(Array);
				
				return Array.Array;
			}
		}
		
		public static void Release(byte[] array){
			lock(Arrays){
				for(int i = Arrays.Count-1; i > -1; i--){
					if(Arrays[i].Array == array){
						Arrays[i].Locked = false;
						return;
					}
				}
			}
			
			throw new ArgumentOutOfRangeException("Byte array doesn't exist in pool");
		}
		
		
		private class ByteArray{
			public byte[] Array;
			public bool Locked = false;
		}
	}
}
