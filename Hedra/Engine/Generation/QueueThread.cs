/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 14/10/2016
 * Time: 12:02 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Threading;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;

namespace Hedra.Engine.Generation
{
	/// <summary>
	/// Description of QueueThread.
	/// </summary>
	public class QueueThread
	{
		public Thread WorkingThread;
		public bool IsWorking{ get; set;}
		public bool Stop;
		public bool DoMesh;
		
		public Chunk CurrentChunk = null;
		public QueueThread()
		{
			this.IsWorking = false;
			this.WorkingThread = new Thread(Start);
			this.WorkingThread.Start();
		}
		
		public void Process(Chunk c, bool DoMesh){
			IsWorking = true;
			CurrentChunk = c;
			this.DoMesh = DoMesh;
		}
		
		public void Start(){
			try{
				while(Program.GameWindow.Exists && !Stop){
					Thread.Sleep(GenerationQueue.ThreadTime);
					if(CurrentChunk != null && !CurrentChunk.Disposed){
						
						if(DoMesh){
							CurrentChunk.BuildMesh();
						}else{
							if(!CurrentChunk.Mesh.IsGenerated || CurrentChunk.Landscape.BlocksSetted)
								CurrentChunk.Generate();
							else
								CurrentChunk.Landscape.PlaceStructures();
						}
						CurrentChunk = null;
						IsWorking = false;
					}
				}
			}catch(Exception e){
				Log.WriteLine(e.ToString());
			}
		}
	}
}
