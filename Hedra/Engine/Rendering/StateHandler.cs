using System.Collections.Generic;

namespace Hedra.Engine.Rendering
{
    public abstract class StateHandler<T>
    {
        private readonly Dictionary<T, bool> _state;

        protected StateHandler()
        {
            _state = new Dictionary<T, bool>();
        }

        public void Enable(T Index)
        {
            HandleRegistration(Index);
            if (!_state[Index])
            {
                DoEnable(Index);
                _state[Index] = true;
            }
        }

        public void Disable(T Index)
        {
            HandleRegistration(Index);
            if (_state[Index])
            {
                DoDisable(Index);
                _state[Index] = false;
            }
        }

        private void HandleRegistration(T Index)
        {
            if (!_state.ContainsKey(Index)) _state.Add(Index, false);
        }

        protected abstract void DoEnable(T Index);

        protected abstract void DoDisable(T Index);
    }
}