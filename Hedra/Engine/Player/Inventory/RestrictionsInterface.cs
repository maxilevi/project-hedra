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
            if(this.HasRestriction(Index, Type)) return;
            _array.AppendRestriction(Index, Type);
        }

        public bool HasRestriction(int Index, string Type)
        {
            var restrictions = _array.GetRestrictions(Index);
            if (restrictions == null) return false;
            return restrictions.Contains(Type);
        }
    }
}
