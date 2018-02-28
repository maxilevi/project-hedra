/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 05/05/2016
 * Time: 09:31 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using VoxelShift.Engine.Generation;
using VoxelShift.Engine.Rendering.Animations;
using VoxelShift.Engine.Rendering;

namespace VoxelShift.Engine.Entity
{
	public class ItemModel : IModel
	{
		public EntityMesh Mesh;
		public Animation IdleAnimation;
		public ItemModel(string Contents, Vector3 Position, Vector3 Rotation, Vector3 Scale, WorldManager World)
		{
			this.IdleAnimation = new Animation(new Vector3[]{new Vector3(0,-0.5f,0), new Vector3(0,0.5f,0)}, new Vector3[]{new Vector3(0,180,0), new Vector3(0,180,0)});
			this.IdleAnimation.Looping = true;
			
			this.Scale = Scale;
			this.Mesh = EntityMesh.Create(Contents, Scale, Position, Rotation, World);
		}
		
		private Vector3 m_Position;
		public Vector3 Position{
			get{
				return m_Position;
			}
			set{
				Mesh.Position = Mesh.Position - m_Position + value;
				m_Position = value;
				
				//Head.Point = Head.Position - m_Position;
			}
		}
		
		private Vector3 m_Rotation;
		public Vector3 Rotation{
			get{
				return m_Rotation;
			}
			set{
				Mesh.Rotation = Mesh.Rotation - m_Rotation + value;
				m_Rotation = value;
			}
		}
		
		private Vector3 m_Scale;
		public Vector3 Scale{
			get{ return m_Scale;}
			set{ m_Scale = value;}
		}
	}
}
