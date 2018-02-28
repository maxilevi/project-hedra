/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 26/04/2016
 * Time: 10:11 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using VoxelShift.Engine.Rendering.Animations;
using VoxelShift.Engine.Rendering;
using OpenTK;

namespace VoxelShift.Engine.Item
{
	/// <summary>
	/// Description of Item.
	/// </summary>
	public class Item
	{
		private EntityMesh ItemMesh;
		public static Animation RotationAnimation = new Animation(new Vector3[]{}, new Vector3[]{new Vector3(0,180,0), new Vector3(0,180,0)});
		
		public Item(string VOXData)
		{
			ItemMesh = EntityMesh.Create(VOXData, new Vector3(0.15f,0.15f,0.15f), new Vector3(0,2,0),Vector3.Zero, Scenes.SceneManager.Game.World);
			ItemMesh.PlayAnimation(RotationAnimation);
		}
	}
}
