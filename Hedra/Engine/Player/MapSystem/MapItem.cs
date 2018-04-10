using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using System;
using OpenTK;

namespace Hedra.Engine.Player.MapSystem
{
    public class MapItem : IDisposable
    {
        private readonly ObjectMesh _mesh;

        public MapItem(VertexData Data)
        {
            _mesh = ObjectMesh.FromVertexData(Data);    
            DrawManager.Remove(_mesh);
        }

        public void Draw()
        {
            _mesh.Draw();
        }

        public bool Enabled
        {
            get { return _mesh.Enabled; }
            set { _mesh.Enabled = value; }
        }

        public Vector3 Position
        {
            get { return _mesh.Position; }
            set { _mesh.Position = value; }
        }

        public Vector3 Scale
        {
            get { return _mesh.Scale; }
            set { _mesh.Scale = value; }
        }

        public Vector3 Rotation
        {
            get { return _mesh.Rotation; }
            set { _mesh.Rotation = value; }
        }

        public void Dispose()
        {
            _mesh.Dispose();
        }
    }
}
