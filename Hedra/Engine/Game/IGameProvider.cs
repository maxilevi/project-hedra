using System;
using System.Numerics;
using Hedra.Engine.Input;
using Hedra.Engine.Player;

namespace Hedra.Engine.Game
{
    public interface IGameProvider
    {
        bool Exists { get; }
        bool IsExiting { get; }
        KeyboardManager Keyboard { get; }
        IPlayer Player { get; set; }
        bool IsLoading { get; }
        bool InStartMenu { get; }
        bool InMenu { get; }
        bool SpawningEffect { get; set; }
        bool PlayerExists { get; }
        event EventHandler AfterSave;
        event EventHandler BeforeSave;
        void MakeCurrent(PlayerInformation Information);
        void LoadMenu();
        void Load();
        void LoadCharacter(PlayerInformation Information);
        void NewRun(PlayerInformation Information);
        void Unload();
        void Reload();
        bool NearAnyPlayer(Vector3 Position, float Radius);
    }
}