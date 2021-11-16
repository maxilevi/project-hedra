using System;
using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Rendering;

namespace Hedra.Engine.Player.MapSystem
{
    public class MapItem : IDisposable
    {
        public MapItem(VertexData Data)
        {
            Mesh = ObjectMesh.FromVertexData(Data, false);
            DrawManager.RemoveObjectMesh(Mesh);
        }

        public ObjectMesh Mesh { get; }

        public bool Enabled
        {
            get => Mesh.Enabled;
            set => Mesh.Enabled = value;
        }

        public Vector3 Position
        {
            get => Mesh.Position;
            set => Mesh.Position = value;
        }

        public Vector3 Scale
        {
            get => Mesh.Scale;
            set => Mesh.Scale = value;
        }

        public Vector3 Rotation
        {
            get => Mesh.LocalRotation;
            set => Mesh.LocalRotation = value;
        }

        public void Dispose()
        {
            Mesh.Dispose();
        }

        public void Draw()
        {
            Mesh.Draw();
        }
    }
}