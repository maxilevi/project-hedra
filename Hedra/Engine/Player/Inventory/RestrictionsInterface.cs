using System.Linq;

namespace Hedra.Engine.Player.Inventory
{
    public class RestrictionsInterface
    {
        private readonly InventoryArray _array;

        public RestrictionsInterface(InventoryArray Array)
        {
            this._array = Array;
        }

        public void AddRestriction(int Index, string Type)
        {
            if(HasRestriction(Index, Type)) return;
            _array.AppendRestriction(Index, Type);
        }
        
        public void RemoveRestriction(int Index, string Type)
        {
            if(!HasRestriction(Index, Type)) return;
            _array.RemoveRestriction(Index, Type);
        }

        public bool HasRestriction(int Index, string Type)
        {
            var restrictions = _array.GetRestrictions(Index);
            if (restrictions == null) return false;
            return restrictions.Contains(Type);
        }

        public void SetRestrictions(int Index, string[] Types)
        {
            _array.SetRestrictions(Index, Types);
        }

        public string[] GetRestrictions(int Index)
        {
            return _array.GetRestrictions(Index);
        }
    }
}
