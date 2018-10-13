using System;
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.Game
{
    public interface IGameProvider
    {
        event EventHandler AfterSave;
        event EventHandler BeforeSave;
        bool Exists { get; }
        bool IsExiting { get; }
        KeyboardManager Keyboard { get; }
        IPlayer Player { get; set; }
        bool IsLoading { get; }
        bool InStartMenu { get; }
        bool InMenu { get; }
        bool SpawningEffect { get; set; }
        void MakeCurrent(PlayerInformation Information);
        void LoadMenu();
        void Load();
        void NewRun(PlayerInformation Information);
        void Unload();
        void Reload();
    }
}