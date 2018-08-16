using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace HedraTests
{
    public class SimpleGameProviderMock : IGameProvider
    {
        public bool Exists { get; set; } = true;
        
        public KeyboardManager Keyboard => null;
        
        public IPlayer Player { get; set; }
        
        public bool IsLoading => false;
        
        public bool InStartMenu => false;
        
        public bool InMenu => false;
        
        public bool SpawningEffect { get; set; }
        
        public void MakeCurrent(PlayerInformation Information)
        {
            throw new System.NotImplementedException();
        }

        public void LoadMenu()
        {
            throw new System.NotImplementedException();
        }

        public void Load()
        {
            throw new System.NotImplementedException();
        }

        public void NewRun(PlayerInformation Information)
        {
            throw new System.NotImplementedException();
        }
    }
}