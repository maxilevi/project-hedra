/*
 * Author: Zaphyk
 * Date: 04/02/2016
 * Time: 05:35 p.m.
 *
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.Enviroment;
using Hedra.Engine.Scenes;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// A static class which gathers all update functions
	/// </summary>
	/// TODO: impletem update distance
	public static class UpdateManager
	{
		private static readonly List<IUpdatable> UpdateFunctions;
	    private static readonly TickSystem Ticker;

	    static UpdateManager()
	    {
	        UpdateFunctions = new List<IUpdatable>();
	        Ticker = new TickSystem();

        }

        public static void Add(IUpdatable Updatable)
	     {
	        var tickable = Updatable as ITickable;
	         if (tickable != null)
	         {
	            Ticker.Add(tickable);
                return;
	         }
	         UpdateFunctions.Add(Updatable);
        }
		
		public static void Remove(IUpdatable Updatable){
		    var tickable = Updatable as ITickable;
		    if (tickable != null)
		    {
		        Ticker.Remove(tickable);
                return;
		    }
		    UpdateFunctions.Remove(Updatable);
        }
		
		public static bool Contains(IUpdatable Func){
			return UpdateFunctions.Contains(Func);
		}
	
	     
		public static void Update()
	     {
			
	     	for(int i = UpdateFunctions.Count-1;i>-1;i--)
	        {
	     		if(UpdateFunctions[i] == null){
	     			UpdateFunctions.RemoveAt(i);
	     			continue;
	     		}
	     		
	     		UpdateFunctions[i].Update();
	        }
            Ticker.Tick();
	     	SkyManager.Update();
	     }
	  	
		public static void CenterMouse(){
			System.Windows.Forms.Cursor.Position = new System.Drawing.Point(GameSettings.Width / 2, GameSettings.Height / 2);
		}

	    public static CursorState CursorState { get; set; }

	    private static bool _isShown = true;
	    public static bool CursorShown
	    {
	        get{return _isShown;}
	        set
	        {
	            if (value == _isShown)
	            {
	                return;
	            }
	            Program.GameWindow.CursorVisible = value;
	
	            _isShown = value;
	        }
	    }
	}
}

public enum CursorState{
	NORMAL,
	CLICK,
	DRAG
}