using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.Game
{
    public interface IGameProvider
    {
        bool Exists { get; }
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
    }
}