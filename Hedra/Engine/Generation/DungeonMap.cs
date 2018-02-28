/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 17/08/2016
 * Time: 08:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Hedra.Engine.Generation
{
	/// <summary>
	/// Description of DungeonMap.
	/// </summary>
	public static class DungeonMap
	{
		public static bool Created = false;
		public const int MapWidth = 256;
		public const int MapHeight = 256;
		public const int MinimumRooms = 32;
		private static Random Rng = new Random();
		public static byte[][] Data = new byte[MapWidth][];
		
		public static void CreateMap(){
			
			PointF[] Back;
			RectangleF[] Rectangles = MakeRooms2();
			PointF[] Points = MakePath(Rectangles, out Back);
			
			
			Bitmap Bmp = new Bitmap(MapWidth, MapHeight);
			using(Graphics G = Graphics.FromImage(Bmp)){
				G.FillRectangle(new SolidBrush(Color.FromArgb(255,89,89,89)), new RectangleF(0,0,MapWidth, MapWidth));
				
				for(int i = 0; i < Points.Length; i++){
				//	Bmp.SetPixel( (int) Mathf.Clamp(Points[i].X, 0, MapWidth-1), (int) Mathf.Clamp(Points[i].Y, 0, MapHeight-1) , Color.WhiteSmoke);
				}
				for(int i = 0; i < Back.Length; i++){
				//	Bmp.SetPixel( (int) Mathf.Clamp(Back[i].X, 0, MapWidth-1), (int) Mathf.Clamp(Back[i].Y, 0, MapHeight-1), Color.DarkRed);
				}
				G.FillRectangles(new SolidBrush(Color.FromArgb(255,89,89,89)), Rectangles);
				AStar(Bmp, Rectangles);
				G.FillRectangles(new SolidBrush(Color.White), Rectangles);
				for(int i = 0; i < Points.Length; i++){
				//if(!IsInsideRectangle(Points[i], Rectangles))
				//	Bmp.SetPixel( (int) Mathf.Clamp(Points[i].X, 0, MapWidth-1), (int) Mathf.Clamp(Points[i].Y, 0, MapHeight-1),Color.FromArgb(255,89,89,89) );
				}
			}
			for(int i = 0; i < Data.Length; i++){
				Data[i] = new byte[MapHeight];
				for(int j = 0; j < Data.Length; j++){
					Data[i][j] = (byte) ( (Bmp.GetPixel(i,j) == Color.FromArgb(255,89,89,89)) ? 0 : 1);
				}
			}
			
			//Bmp.Serialize("Heightmap.png");
			
			//Process.Start("Heightmap.png");
			//Program.GameWindow.Exit();
		}
		
		private static void AStar(Bitmap Bmp, RectangleF[] Rooms){
			Dictionary<PointF, bool> AvailablePoints = new Dictionary<PointF, bool>();
			for(int x = 0; x < Bmp.Width; x++){
				for(int y = 0; y < Bmp.Height; y++){
					if(Bmp.GetPixel(x,y) == Color.FromArgb(255,89,89,89))
						AvailablePoints.Add(new PointF(x,y), true);
				}
			}
			
			for(int i = 0; i < Rooms.Length-1; i++){
				RectangleF StartRoom = Rooms[i];
				RectangleF EndRoom = Rooms[i+1];
			
				PointF CurrentCell = new PointF((int) (StartRoom.X + StartRoom.Width / 2), (int) (StartRoom.Y + StartRoom.Height / 2) );
				PointF EndCell = new PointF((int) (EndRoom.X + EndRoom.Width / 2), (int) (EndRoom.Y + EndRoom.Height / 2) );

				Dictionary<PointF, bool> MarkedCells = new Dictionary<PointF, bool>();
				List<PointF> Stack = new List<PointF>();
				Stack.Add(CurrentCell);
				int Tries = -1;
				while(Stack.Count != 0){
					Tries++;
					float MinHeuristic = float.MaxValue;
					CurrentCell = Stack[Stack.Count-1];
					PointF NextCell = CurrentCell;
					PointF[] Neighbours = AvailableNeighbourCellsTrace(CurrentCell, AvailablePoints);
					
					int MarkedCount = 0;
					for(int j = 0; j < Neighbours.Length; j++){					
						if(MarkedCells.ContainsKey(Neighbours[j]))
							MarkedCount++;
					}
					
					if((Neighbours.Length == 0 || MarkedCount == Neighbours.Length) && !IsInsideRectangle(NextCell, EndRoom)){
						Stack.RemoveAt(Stack.Count-1);
						if(!MarkedCells.ContainsKey(CurrentCell))
							MarkedCells.Add(CurrentCell, true);
						continue;
					}
					
					
					for(int j = 0; j < Neighbours.Length; j++){
						
						if(MarkedCells.ContainsKey(Neighbours[j]))continue;
						
						float Heuristic = Neighbours[j].Distance(CurrentCell) + Neighbours[j].Distance(EndCell);
						if(Heuristic < MinHeuristic){
							MinHeuristic = Heuristic;
							NextCell =  Neighbours[j];
						}
					}
					
					if(IsInsideRectangle(NextCell, EndRoom))
						break;
					
					if(!MarkedCells.ContainsKey(CurrentCell))
						MarkedCells.Add(CurrentCell, true);
					
					if(!IsInsideRectangle(NextCell, Rooms))
						Bmp.SetPixel( (int) NextCell.X, (int) NextCell.Y, Color.ForestGreen);
					
					
					Stack.Add(NextCell);
				}
			}
		}
		
		private static PointF[] MakePath(RectangleF[] Rooms, out PointF[] BackPoints){
			List<PointF> PointList = new List<PointF>();
			//For speed increase
			Dictionary<PointF, bool> Cells = new Dictionary<PointF, bool>();
			List<PointF> Stack = new List<PointF>();
			List<PointF> BackList = new List<PointF>();
			
			
			Cells.Add( new PointF(0,0), true);
			Stack.Add( new PointF(0,0) );
			PointList.Add( new PointF(0,0) );
			
			int UnvisitedCells = 1;
			int MapArea = ( MapWidth * MapHeight);
			while(Stack.Count != 0){
				UnvisitedCells = (int) ( MapArea - Cells.Count);
				
				PointF[] AvailableCells = AvailableNeighbourCells(Stack[Stack.Count-1], Cells);
				
				if(AvailableCells.Length != 0){
					Stack.Add( AvailableCells[ Math.Min(1, Rng.Next(0, AvailableCells.Length))] );
					
					
					/*if(IsInsideRectangle(Stack[Stack.Count-1], Rooms)){
						if(!Cells.ContainsKey(Stack[Stack.Count-1])){
							Cells.Add(Stack[Stack.Count-1], true);
							//PointF BackPoint = new PointF( Math.Min(Stack[Stack.Count-1].X * 2f, MapWidth-1), Math.Min(Stack[Stack.Count-1].Y*2f, MapHeight-1) );
							//BackList.Add( BackPoint );
						}
						Stack.RemoveAt(Stack.Count-1);
						continue;
					}*/
					
					if(!Cells.ContainsKey(Stack[Stack.Count-1])){
						Cells.Add( Stack[Stack.Count-1], true);
						PointF NewPoint = new PointF( Math.Min(Stack[Stack.Count-1].X * 2f, MapWidth-1), Math.Min(Stack[Stack.Count-1].Y*2f, MapHeight-1) );
						PointList.Add( NewPoint );
					}
					
					PointF WallRemoval = new PointF( Stack[Stack.Count-1].X * 2f + (Stack[Stack.Count-1].X - Stack[Stack.Count-2].X),
					                                 Stack[Stack.Count-1].Y * 2f + (Stack[Stack.Count-1].Y - Stack[Stack.Count-2].Y) );
					PointList.Add( WallRemoval );
				}else{
					if(Stack.Count == 1)
					break;
					
					if(!Cells.ContainsKey(Stack[Stack.Count-1])){
						Cells.Add(Stack[Stack.Count-1], true);
					}  
					PointF NewPoint = new PointF( Stack[Stack.Count-1].X * 2f, Stack[Stack.Count-1].Y*2f );
					BackList.Add( NewPoint );
				
					PointF WallRemoval = new PointF( Stack[Stack.Count-1].X * 2f + (Stack[Stack.Count-1].X - Stack[Stack.Count-2].X),
				                                 Stack[Stack.Count-1].Y * 2f + (Stack[Stack.Count-1].Y - Stack[Stack.Count-2].Y) );
					BackList.Add( WallRemoval );
					
					Stack.RemoveAt(Stack.Count-1);
				}
				
			}
			BackPoints = BackList.ToArray();
			return PointList.ToArray();
		}
		
		private static RectangleF[] MakeRooms(int Count){
			List<RectangleF> RectangleList = new List<RectangleF>();
			
			for(int i = 0; i < Count; i++){
				Point Size = new Point(Rng.Next(8, 24), Rng.Next(8, 24));
				RectangleF NewRectangle = new RectangleF(Rng.Next(0, MapWidth - Size.X), Rng.Next(0, MapHeight - Size.Y ), Size.X, Size.Y);
				
				bool Collides = false;
				for(int j = 0; j < RectangleList.Count; j++){
					if( DungeonMap.RoomsIntersect(RectangleList[j], NewRectangle) ){
						Collides = true;
						break;
					}
				}
				
				if(Collides) continue;
				
				RectangleList.Add(NewRectangle);
			}
			return RectangleList.ToArray();
		}
		
		private static RectangleF[] MakeRooms2(){
			List<RectangleF> RectangleList = new List<RectangleF>();
			
			Point Size1 = new Point(Rng.Next(24, 48), Rng.Next(24, 48));
			RectangleF NewRectangle1 = new RectangleF(MapWidth/2 - Size1.X / 2, MapHeight/2 - Size1.Y / 2, Size1.X, Size1.Y);
			RectangleList.Add(NewRectangle1);
			
			while(RectangleList.Count < MinimumRooms){
				Point Size = new Point(Rng.Next(8, 24), Rng.Next(8, 24));
				
				RectangleF NearRoom = RectangleList[Rng.Next(0, RectangleList.Count)];
				RectangleF NewRectangle = new RectangleF(NearRoom.X + (int) Rng.Next((int) -NearRoom.Width+4, (int) NearRoom.Width+4), NearRoom.Y + (int) Rng.Next((int)-NearRoom.Height+4, (int) NearRoom.Height+4), Size.X, Size.Y);
				
				bool Collides = false;
				for(int j = 0; j < RectangleList.Count; j++){
					if( DungeonMap.RoomsIntersect(RectangleList[j], NewRectangle) ){
						Collides = true;
						break;
					}
				}
				
				if(Collides) continue;
				
				RectangleList.Add(NewRectangle);
			}
			return RectangleList.ToArray();
		}
		
		private static bool IsInsidePoint(PointF P, PointF[] Points){
			bool Inside = false;
			for(int i = 0; i < Points.Length; i++){
				if(P == Points[i]){
					Inside = true;
					break;
				}
			}
			return Inside;
		}
		
		private static bool IsInsideRectangle(PointF P, RectangleF[] Rects){
			bool Inside = false;
			for(int i = 0; i < Rects.Length; i++){
				if(IsInsideRectangle(P, Rects[i])){
					Inside = true;
					break;
				}
			}
			return Inside;
		}
		
		private static bool IsInsideRectangle(PointF R1, RectangleF R2){
			return R1.X < R2.X + R2.Width &&
				   R1.X > R2.X &&
				   R1.Y < R2.Y + R2.Height &&
				   R1.Y > R2.Y;
		}
		
		private static bool RoomsIntersect(RectangleF R1, RectangleF R2){
			return R1.X < R2.X + R2.Width &&
				   R1.X + R1.Width > R2.X &&
				   R1.Y < R2.Y + R2.Height &&
				   R1.Height + R1.Y > R2.Y;
		}
		
		private static PointF[] AvailableNeighbourCells(PointF Cell, Dictionary<PointF, bool> Cells){
			List<PointF> Neighbours = new List<PointF>();
			
			if(!Cells.ContainsKey(new PointF(Cell.X, Cell.Y +1 )) && Cell.Y < MapHeight -1)
				Neighbours.Add(new PointF(Cell.X, Cell.Y +1 ));
			
			if(!Cells.ContainsKey(new PointF(Cell.X, Cell.Y -1 )) && Cell.Y > 0)
				Neighbours.Add(new PointF(Cell.X, Cell.Y -1 ));
			
			if(!Cells.ContainsKey(new PointF(Cell.X+1, Cell.Y )) && Cell.X < MapWidth -1)
				Neighbours.Add(new PointF(Cell.X+1, Cell.Y ));
			
			if(!Cells.ContainsKey(new PointF(Cell.X-1, Cell.Y )) && Cell.X > 0)
				Neighbours.Add(new PointF(Cell.X-1, Cell.Y ));
			
			
			return Neighbours.ToArray();
		}
		
		private static PointF[] AvailableNeighbourCellsTrace(PointF Cell, Dictionary<PointF, bool> Cells){
			List<PointF> Neighbours = new List<PointF>();
			
			if(Cells.ContainsKey(new PointF(Cell.X+1, Cell.Y )) )
				Neighbours.Add(new PointF(Cell.X+1, Cell.Y ));
			
			if(Cells.ContainsKey(new PointF(Cell.X-1, Cell.Y )) )
				Neighbours.Add(new PointF(Cell.X-1, Cell.Y ));
			
			if(Cells.ContainsKey(new PointF(Cell.X, Cell.Y +1 )) )
				Neighbours.Add(new PointF(Cell.X, Cell.Y +1 ));
			
			if(Cells.ContainsKey(new PointF(Cell.X, Cell.Y -1 )) )
				Neighbours.Add(new PointF(Cell.X, Cell.Y -1 ));
			
			return Neighbours.ToArray();
		}
		
		private static int AreaOfRooms(RectangleF[] Rects){
			int Area = 0;
			for(int i = 0; i < Rects.Length; i++)
				Area += (int)(Rects[i].Width * Rects[i].Height);
			return Area;
		}
	}
}
