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

namespace Hedra.Engine.PhysicsSystem
{
	/// <summary>
	/// Description of PhysicsThreadManager.
	/// </summary>
	public class PhysicsThreadManager
	{
		private readonly List<Entity> _toUpdate = new List<Entity>();
		private readonly List<MoveCommand> _toMove = new List<MoveCommand>();
		private Thread _physicsThread;
		private bool _sleep = true;
		
		public void Load(){
			_physicsThread = new Thread(this.Start);
			_physicsThread.Start();
		}
		
		public void Add(Entity Member){
            //lock(_toUpdate)
			    _toUpdate.Add(Member);
		}
		
		public void Add(MoveCommand Member){
		    //lock (_toMove)
                _toMove.Add(Member);
		}
		
		public void Update(){
			_sleep = false;
		}

	    public int Count => _toUpdate.Count;

        public void Start(){
			
			while(Program.GameWindow.Exists){
				if(_sleep){
					Thread.Sleep(1);
					continue;
				}



			    for (int i = _toUpdate.Count - 1; i > -1; i--)
			    {
			        try
			        {
			            if (_toUpdate[i] != null)
			                _toUpdate[i].Physics.Update();

			            _toUpdate.RemoveAt(i);
			        }
			        catch (Exception e)
			        {
			            Log.WriteLine(e.ToString());
			        }
			    }

			
			 
			    for (int i = _toMove.Count - 1; i > -1; i--)
			    {
			        try
			        {
			            if (_toMove[i].Parent != null)
			                _toMove[i].Parent.Physics.ProccessCommand(_toMove[i]);

			            _toMove.RemoveAt(i);
			        }
			        catch (Exception e)
			        {
			            Log.WriteLine(e.ToString());
			        }
			    }
			    
			    _sleep = true;
			}
			
		}
	}
}
