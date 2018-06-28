/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 04:05 p.m.
 *
 */
using System;
using System.Threading;
using System.Collections.Generic;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.Generation
{
/// <summary>
/// Ported from the unity script i wrote
/// </summary>
	internal class GenerationQueue
	{
		public List<Chunk> Queue = new List<Chunk>();
		public List<GenerationThread> Threads = new List<GenerationThread>();
		public static int ThreadCount = 1;
		public const int ThreadTime = 15;
		public bool Stop {get; set;}
		private ClosestChunk ClosestChunkComparer = new ClosestChunk();
	
		public GenerationQueue(){
			var MainLoop = new Thread(ProccessQueueThread);
			MainLoop.Start();
			for(int i = 0; i < ThreadCount; i++){
				Threads.Add(new GenerationThread());
			}
		}
		
		public bool Discard;
		public void SafeDiscard(){
			Discard = true;
		}
		
		private void ProccessQueueThread(){
			while(true)
            {		    
				Thread.Sleep(25);
				if(!Program.GameWindow.Exists || Stop)
					break;
				
				if(Discard){
					Queue.Clear();
					Discard = false;
					continue;
				}

			    if (Queue.Count == 0) continue;
			    if(GameManager.Player != null){
			        ClosestChunkComparer.PlayerPos = GameManager.Player.Position;
						
			        lock(Queue){
			            try{
			                Queue.Sort(ClosestChunkComparer);
			            }catch(Exception e){
			                Log.WriteLine("Error while sorting chunks. Retrying...");
			            }
			        }
			    }
                
			    try
			    {
			        if (Queue[0] == null)
			        {
			            Queue.RemoveAt(0);
			            continue;
			        }
			    }
			    catch (Exception e)
			    {
			        Log.WriteLine(e);
			    }
			    try
			    {
			        for (int i = 0; i < Threads.Count; i++)
			        {
			            if (!Threads[i].IsWorking && Queue.Count != 0)
			            {
			                Threads[i].Generate(Queue[0]);
			                Queue[0].IsGenerated = true;
			                Queue.RemoveAt(0);
			            }
			        }
			    }
			    catch (ArgumentOutOfRangeException e)
			    {
			        Log.WriteLine(e);
			    }
            }
		}
	}
}
