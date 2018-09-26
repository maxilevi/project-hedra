/*
 * Author: Zaphyk
 * Date: 31/01/2016
 * Time: 08:12 p.m.
 *
 */
using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Rendering
{
	public class ChunkMesh : ICullable, IDisposable
	{
		private IMeshBuffer _buffer;
		public List<InstanceBatch> InstanceBatches = new List<InstanceBatch>();
		public List<InstanceData> InstanceElements = new List<InstanceData>();
		public List<ICollidable> CollisionBoxes = new List<ICollidable>();
		public List<VertexData> Elements = new List<VertexData>();
		public VertexData ModelData { get; set; }

		public bool IsBuilded;
		public bool IsGenerated;
		public bool Enabled { get; set; }
		public bool BuildedOnce { get; set; }

		public Vector3 Position { get; set; }
		public Box CullingBox { get; set; }


		public ChunkMesh(Vector3 Position, IMeshBuffer Buffer)
		{
			this.Position = Position;
			CullingBox = new Box(Vector3.Zero, new Vector3(Chunk.Width, 768, Chunk.Width));
			_buffer = Buffer;
		}

		public void BuildFrom(VertexData Data, bool ExtraData)
		{
			try
			{
				if (Data?.Colors == null)
					return;

				Vector4[] ColorBuffer;
				if (ExtraData)
				{
					ColorBuffer = new Vector4[Data.Colors.Count];
					for (int i = 0; i < ColorBuffer.Length; i++)
					{
						ColorBuffer[i] = new Vector4(Data.Colors[i].Xyz, Data.Extradata[i]);
					}
				}
				else
				{
					ColorBuffer = Data.Colors.ToArray();
				}

				int ColorBufferSize = (ColorBuffer.Length * Vector4.SizeInBytes);
				int VertexBufferSize = (Data.Vertices.Count * Vector3.SizeInBytes);
				int IndexBufferSize = (Data.Indices.Count * sizeof(int));
				int NormalBufferSize = (Data.Normals.Count * Vector3.SizeInBytes);

				if (_buffer.Vertices == null)
					_buffer.Vertices = new VBO<Vector3>(Data.Vertices.ToArray(), VertexBufferSize,
						VertexAttribPointerType.Float);
				else
					_buffer.Vertices.Update(Data.Vertices.ToArray(), VertexBufferSize);

				if (_buffer.Indices == null)
					_buffer.Indices = new VBO<uint>(Data.Indices.ToArray(), IndexBufferSize,
						VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
				else
					_buffer.Indices.Update(Data.Indices.ToArray(), IndexBufferSize);


				if (_buffer.Colors == null)
					_buffer.Colors = new VBO<Vector4>(ColorBuffer, ColorBufferSize, VertexAttribPointerType.Float);
				else
					_buffer.Colors.Update(ColorBuffer, ColorBufferSize);

				if (_buffer.Normals == null)
					_buffer.Normals = new VBO<Vector3>(Data.Normals.ToArray(), NormalBufferSize,
						VertexAttribPointerType.Float);
				else
					_buffer.Normals.Update(Data.Normals.ToArray(), NormalBufferSize);

				if (_buffer.Data == null)
					_buffer.Data =
						new VAO<Vector3, Vector4, Vector3>(_buffer.Vertices, _buffer.Colors, _buffer.Normals);

				//Data.Dispose();
				//Data = null;
				IsBuilded = true;
				Enabled = true;
				BuildedOnce = true;
			}
			catch (Exception e)
			{
				Log.WriteLine(e.ToString());
			}
		}

		public void Draw()
		{
			if (GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
			DrawMesh(_buffer);
			if (GameSettings.Wireframe) Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
		}

		private void DrawMesh(IMeshBuffer MeshBuffer)
		{
			if (IsBuilded && IsGenerated && Enabled && MeshBuffer.Data != null)
			{
				MeshBuffer.Draw();
			}
		}

		public void AddInstance(InstanceData Data)
		{
			InstanceElements.Add(Data);
		}

		public void Dispose()
		{
			_buffer?.Dispose();
		}
	}
}
