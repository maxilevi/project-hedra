/*
 * Author: Zaphyk
 * Date: 04/02/2016
 * Time: 05:35 p.m.
 *
 */
using System.Collections.Generic;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Game;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// A static class which gathers all update functions
	/// </summary>
	public static class UpdateManager
	{
		private static readonly HashSet<IUpdatable> UpdateFunctions;
		private static readonly List<IUpdatable> UpdateFunctionsList;
	    private static readonly TickSystem Ticker;
		private static bool _isShown = true;
		private static readonly object Lock = new object();

	    static UpdateManager()
	    {
	        UpdateFunctions = new HashSet<IUpdatable>();
		    UpdateFunctionsList = new List<IUpdatable>();
	        Ticker = new TickSystem();
        }

        public static void Add(IUpdatable Updatable)
	    {
		    lock (Lock)
		    {
			    if (Updatable is ITickable tickable)
			    {
				    Ticker.Add(tickable);
				    return;
			    }
			    UpdateFunctions.Add(Updatable);
			    UpdateFunctionsList.Add(Updatable);
		    }
	    }
		
		public static void Remove(IUpdatable Updatable)
		{
			lock (Lock)
			{
				if (Updatable is ITickable tickable)
				{
					Ticker.Remove(tickable);
					return;
				}
				UpdateFunctions.Remove(Updatable);
				UpdateFunctionsList.Remove(Updatable);
			}
		}
	     
		public static void Update()
	    {
		    lock (Lock)
		    {
			    for (var i = UpdateFunctionsList.Count - 1; i > -1; i--)
			    {
				    if (UpdateFunctionsList[i] == null)
				    {
					    UpdateFunctions.Remove(UpdateFunctionsList[i]);
					    UpdateFunctionsList.RemoveAt(i);
					    continue;
				    }

				    UpdateFunctionsList[i].Update();
			    }

			    Ticker.Tick();
			    SkyManager.Update();
		    }
	    }
	  	
		public static void CenterMouse()
		{
			System.Windows.Forms.Cursor.Position = new System.Drawing.Point(GameSettings.Width / 2, GameSettings.Height / 2);
		}

	    public static CursorState CursorState { get; set; }

	    public static bool CursorShown
	    {
	        get => _isShown;
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