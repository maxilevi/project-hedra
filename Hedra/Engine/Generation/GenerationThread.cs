/*
 * Author: Zaphyk
 * Date: 07/04/2016
 * Time: 01:41 p.m.
 *
 */
using System;
using System.Threading;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;

namespace Hedra.Engine.Generation
{
	/// <summary>
	/// Description of GenerationThread.
	/// </summary>
	public class GenerationThread
	{
		public Thread WorkingThread;
		public bool IsWorking{ get; set;}
		public bool Stop;
		
		public Chunk CurrentChunk = null;
		public GenerationThread()
		{
			this.IsWorking = false;
			this.WorkingThread = new Thread(Start);
			this.WorkingThread.Start();
		}
		
		public void Generate(Chunk c){
			IsWorking = true;
			CurrentChunk = c;
		}
		
		public void Start(){
			try{
				while(Program.GameWindow.Exists && !Stop){
					Thread.Sleep(GenerationQueue.ThreadTime * GenerationQueue.ThreadCount);
					if(CurrentChunk != null && !CurrentChunk.Disposed){
						
						if(!CurrentChunk.Mesh.IsGenerated || CurrentChunk.Landscape.BlocksSetted){
							CurrentChunk?.Generate();
						}else{
							CurrentChunk?.Landscape?.PlaceStructures();
						}
						CurrentChunk = null;
						IsWorking = false;
					}
					CurrentChunk = null;
					IsWorking = false;
				}
			}catch(Exception e){
				Log.WriteLine(e.ToString());
			}
		}
	}
}
