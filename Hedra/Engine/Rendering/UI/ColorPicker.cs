/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/06/2016
 * Time: 10:30 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.Management;
using System.Drawing;
using System.IO;
using OpenTK;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of ColorPicker.
	/// </summary>
	public delegate void ColorPickedEventHandler(Vector4 Color);
	
	public class ColorPicker : UIElement
	{
		public event ColorPickedEventHandler ColorPickedEvent; 
		public List<UIElement> Elements = new List<UIElement>();
		private static Vector2 _targetResolution = new Vector2(1024, 600);
		
		public ColorPicker(Vector4[] Colors, string Name, Vector2 Position, Vector2 Scale, Panel InPanel, int ColorsPerRow = 3){
			int rowCount = 0;
			Vector2 xOffset = Vector2.Zero;
			Vector2 yOffset = Vector2.Zero;
			Vector2 totalOffset = Vector2.Zero;
			
			for(int i = 0; i < Colors.Length; i++){
				Button background = new Button(Position + Mathf.ScaleGUI(_targetResolution,xOffset+yOffset) * Scale, Mathf.ScaleGUI(new Vector2(600,600),new Vector2(.15f,.15f)) * 0.5f *Scale, string.Empty, GUIRenderer.TransparentTexture);
				int k = i;
				background.Click += delegate { if(ColorPickedEvent != null) ColorPickedEvent.Invoke(Colors[k]); };
				Texture backgroundTex = new Texture(Graphics2D.LoadTexture(Graphics2D.Clone(Graphics2D.RoundedRectangle)),
				                                    Position + Mathf.ScaleGUI(_targetResolution, xOffset+yOffset) * Scale, Mathf.ScaleGUI(new Vector2(600,600),new Vector2(.15f,.15f)) * 0.5f * Scale);
				
				Texture colorTex = new Texture(Graphics2D.LoadTexture(Graphics2D.ReColorMask(Mathf.ToColor(Colors[i]),Graphics2D.Clone(Graphics2D.RoundedRectangle))),
				                              Position +  Mathf.ScaleGUI(_targetResolution, xOffset+yOffset) * Scale, Mathf.ScaleGUI(new Vector2(600,600),new Vector2(.15f,.15f)) * 0.4f * Scale);
				xOffset += new Vector2(0.1f,0);
				if(rowCount == 3){
					xOffset = Vector2.Zero;
					yOffset += new Vector2(0, 0.175f);
					rowCount = -1;
				}
				InPanel.AddElement(background);
				InPanel.AddElement(colorTex);
				InPanel.AddElement(backgroundTex);
				Elements.Add(background);
				Elements.Add(colorTex);
				Elements.Add(backgroundTex);
				rowCount++;
			}
			//Simple hack
			GUIText title = new GUIText(Name,Position + Mathf.ScaleGUI(new Vector2(600,600),new Vector2(.15f,.15f / 8 * Colors.Length)) * .5f * 8 * .5f * Scale - new Vector2(0.1f,0) * (0.25f),
			                            Color.FromArgb(255,39,39,39), FontCache.Get(UserInterface.Fonts.Families[0], 14 * Scale.X));
			
			Elements.Add(title);
			InPanel.AddElement(title);
		}
		
		public void PickRandom(){
			Random:
				int rng = Utils.Rng.Next(0, Elements.Count);
				if(!(Elements[rng] is Button))
					goto Random;
				else
					(Elements[rng] as Button).ForceClick();
		}
		
		public void Enable(){
			for(int i = 0; i < Elements.Count; i++){
				Elements[i].Enable();
			}
		}
		
		public void Disable(){
			for(int i = 0; i < Elements.Count; i++){
				Elements[i].Disable();
			}
		}
		
		private Vector2 _mScale;
		public Vector2 Scale{
			get{ return _mScale; }
			set{
				_mScale = value;
				for(int i = 0; i < Elements.Count; i++){
					Elements[i].Scale = value;
				}
			}
		}
		
		private Vector2 _mPosition;
		public Vector2 Position{
			get{ return _mPosition; }
			set{
				for(int i = 0; i < Elements.Count; i++){
					Elements[i].Position = Elements[i].Position + value - _mPosition;
				}
				_mPosition = value;
			}
		}
		
		
	}
}
