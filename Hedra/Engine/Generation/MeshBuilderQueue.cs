/*
 * Author: Zaphyk
 * Date: 18/02/2016
 * Time: 05:11 p.m.
 *
 */
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.Management;

namespace Hedra.Engine.Generation
{
	/// <summary>
	/// Description of MeshBuilderQueue.
	/// </summary>
	public class MeshBuilderQueue
	{
		public List<Chunk> Queue = new List<Chunk>();
		public bool Stop {get; set;}
		private ClosestChunk ClosestChunkComparer = new ClosestChunk();
		public const int SleepTime = 5;
		public const int ThreadCount = 1;
		
		public MeshBuilderQueue(){
			var Threads = new Thread[ThreadCount];
			for(int i = 0; i < ThreadCount; i++){
				Threads[i] = new Thread(Start);
				Threads[i].Start();
			}
		}
		
		public bool Contains(Chunk ChunkToCheck){
			lock(Queue)
				return Queue.Contains(ChunkToCheck);
		}
		
		public void Add(Chunk ChunkToBuild){
			lock(Queue){
				if(!Queue.Contains(ChunkToBuild))
					Queue.Add(ChunkToBuild);
			}
		}
		public bool Discard;
		public void SafeDiscard(){
			Discard = true;
		}
		
		public void Start(){
			try{
		    	while(true){
					Thread.Sleep(SleepTime * ThreadCount);
					if(!Program.GameWindow.Exists || Stop)
						break;
					
					lock(Queue){
						if(Discard){
							Queue.Clear();
							Discard = false;
							continue;
						}
						if(Queue.Count != 0){
							
							if(Scenes.SceneManager.Game.Player != null){
								ClosestChunkComparer.PlayerPos = Scenes.SceneManager.Game.Player.Position;
								lock(Queue)
									Queue.Sort(ClosestChunkComparer);
							}
							
							if(Queue[0] != null){
								Queue[0].BuildMesh();
								if(Queue.Count != 0)
									Queue.RemoveAt(0);
							}else
								Queue.RemoveAt(0);
						}
					}
				}
			}catch(Exception e){
				Log.WriteLine(e.ToString());
			}
		}
	}
}
