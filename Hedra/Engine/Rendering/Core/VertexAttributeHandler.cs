using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedra.Engine.Rendering.Core
{
    public class VertexAttributeHandler
    {
        private const int MaxAttributes = 16;
        private readonly Dictionary<uint, bool> _states;

        public VertexAttributeHandler()
        {
            _states = new Dictionary<uint, bool>();
            for (var i = 0u; i < MaxAttributes; ++i) _states.Add(i, false);
            Renderer.ShaderChanged += OnShaderChanged;
        }

        public int Count => _states.Count(S => S.Value);

        public uint Id { get; private set; }

        public bool IsEnabled(uint Index)
        {
            return _states[Index];
        }

        public void Bind(uint Id)
        {
            Renderer.Provider.BindVertexArray(Id);
            this.Id = Id;
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

        private void OnShaderChanged()
        {
            foreach (var state in _states)
                if (state.Value)
                    throw new ArgumentException(
                        $"Vertex attribute '{state.Key}' was enabled but not disabled after drawing.");
        }

        private static void EnsureValidity()
        {
            if (Renderer.ShaderBound == 0)
                throw new ArgumentException("A shader needs to be bound before using vertex array objects.");
        }

        private void DoEnable(uint Index)
        {
            if (Index > MaxAttributes)
                throw new ArgumentOutOfRangeException($"A shader can only have up to '{MaxAttributes}'");
            Renderer.Provider.EnableVertexAttribArray(Index);
            _states[Index] = true;
        }

        private void DoDisable(uint Index)
        {
            if (Index > MaxAttributes)
                throw new ArgumentOutOfRangeException($"A shader can only have up to '{MaxAttributes}'");
            Renderer.Provider.EnableVertexAttribArray(Index);
            _states[Index] = false;
        }
    }
}