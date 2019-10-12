using System;
using Hedra.Engine.Game;
using Hedra.Engine.Input;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using OpenToolkit.Mathematics;

namespace HedraTests
{
    public class SimpleGameProviderMock : IGameProvider
    {    
        public event EventHandler AfterSave;
        
        public event EventHandler BeforeSave;
        
        public bool IsExiting { get; set; } = false;

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

        public void LoadCharacter(PlayerInformation Information)
        {
            throw new NotImplementedException();
        }

        public void NewRun(PlayerInformation Information)
        {
            throw new System.NotImplementedException();
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
            throw new NotImplementedException();
        }

        public bool NearAnyPlayer(Vector3 Position, float Radius)
        {
            throw new NotImplementedException();
        }

        public bool PlayerExists { get; }
    }
}