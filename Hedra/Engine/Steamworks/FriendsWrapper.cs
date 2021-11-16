using Facepunch.Steamworks;

namespace Hedra.Engine.Steamworks
{
    public class FriendsWrapper : SteamObjectWrapper<FriendsWrapper, Friends>
    {
        public bool IsFriend(ulong Id)
        {
            return Source.Get(Id) != null;
        }
    }
}