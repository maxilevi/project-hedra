using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering
{
    public class VertexAttributeHandler
    {
        private readonly Dictionary<uint, bool> _states;

        public VertexAttributeHandler()
        {
            _states = new Dictionary<uint, bool>
            {
                {0, false},
                {1, false},
                {2, false},
                {3, false},
                {4, false},
                {5, false},
                {6, false},
                {7, false},
                {8, false},
                {9, false},
                {10, false},
                {11, false},
                {12, false},
                {13, false},
                {14, false},
                {15, false}
            };
            Renderer.ShaderChanged += OnShaderChanged;
        }
        
        public void Enable(uint Index)
        {
            EnsureValidity();
            DoEnable(Index);
        }

        public void Disable(uint Index)
        {
            EnsureValidity();
            DoDisable(Index);
        }

        public void OnShaderChanged()
        {
            foreach (var state in _states)
            {
                if(state.Value)
                    throw new ArgumentException($"Vertex attribute '{state.Key}' was enabled but not disabled after drawing.");
            }
        }

        private void EnsureValidity()
        {
            if (Renderer.ShaderBound == 0)
                throw new ArgumentException("A shader needs to be bound before using vertex array objects.");
        }
        
        private void DoEnable(uint Index)
        {
            Renderer.Provider.EnableVertexAttribArray(Index);
            _states[Index] = true;
        }
        
        private void DoDisable(uint Index)
        {
            Renderer.Provider.EnableVertexAttribArray(Index);
            _states[Index] = false;
        }
    }
}
