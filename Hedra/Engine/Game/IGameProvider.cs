using System;
using Hedra.Engine.Input;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using OpenToolkit.Mathematics;

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
        void LoadCharacter(PlayerInformation Information);
        void NewRun(PlayerInformation Information);
        void Unload();
        void Reload();
        bool NearAnyPlayer(Vector3 Position, float Radius);
        bool PlayerExists { get; }
    }
}