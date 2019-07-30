using System;
using System.Collections.Generic;
using Hedra.Engine.Rendering.UI;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering.Core
{
    public class TextureHandler
    {
        private readonly Dictionary<TextureUnit, TextureState> _states;
        private TextureUnit _currentUnit = TextureUnit.Texture0;
        public int Skipped { get; private set; }

        public TextureHandler()
        {
            _states = new Dictionary<TextureUnit, TextureState>
            {
                {_currentUnit, new TextureState()}
            };
        }

        public void ResetStats()
        {
            Skipped = 0;
        }

        public uint Create()
        {
            var id = Renderer.Provider.GenTexture();
            if (!Program.IsDummy)
            {
                TextureRegistry.Register(id);
            }
            return id;
        }
        
        public void Bind(TextureTarget Target, uint Id)
        {
#if DEBUG
            if (!Program.IsDummy && Target == TextureTarget.Texture2D && !TextCache.Exists(Id) && !TextureRegistry.IsKnown(Id))
            {
                var a = TextureRegistry.IsKnown(Id);
                //throw new ArgumentOutOfRangeException($"Found an unregistered texture '{Id}' that is being used.");
            }
#endif
            
            if(!_states[_currentUnit].States.ContainsKey(Target))
                _states[_currentUnit].States.Add(Target, uint.MaxValue);

            if (_states[_currentUnit].States[Target] == Id)
            {
                ++Skipped;
                return;
            }
            _states[_currentUnit].States[Target] = Id;
            Renderer.Provider.BindTexture(Target, Id);
        }

        public void Active(TextureUnit Unit)
        {
            if(Unit == _currentUnit) return;
            _currentUnit = Unit;
            if(!_states.ContainsKey(_currentUnit))
                _states.Add(_currentUnit, new TextureState());
            Renderer.Provider.ActiveTexture(_currentUnit);
        }

        public void Delete(uint Id)
        {
            var found = false;
            var stateToModify = default(TextureUnit);
            var keyToModify = default(TextureTarget);
            foreach (var state in _states)
            {
                foreach (var textureState in state.Value.States)
                {
                    if (textureState.Value == Id)
                    {
                        stateToModify = state.Key;
                        keyToModify = textureState.Key;
                        found = true;
                        break;
                    }
                }
            }

            if (found)
            {
                _states[stateToModify].States[keyToModify] = uint.MaxValue;
            }

            Renderer.Provider.DeleteTexture(Id);
        }

        private class TextureState
        {
            public Dictionary<TextureTarget, uint> States { get; }

            public TextureState()
            {
                States = new Dictionary<TextureTarget, uint>();
            }
        }
    }
}