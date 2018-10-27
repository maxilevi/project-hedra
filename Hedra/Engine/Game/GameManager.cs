/*
 * Author: Zaphyk
 * Date: 08/02/2016
 * Time: 02:19 a.m.
 *
 */

using System;
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.Game
{

    public static class GameManager
    {
        public static event EventHandler AfterSave;
        public static event EventHandler BeforeSave;
        public static IGameProvider Provider { get; set; } = new GameProvider();

        public static void Load()
        {
            Provider.Load();
            Provider.BeforeSave += (S, E) => BeforeSave?.Invoke(S, E);
            Provider.AfterSave += (S, E) => AfterSave?.Invoke(S, E);
        }

        public static void LoadMenu()
        {
            Provider.LoadMenu();
        }
        
        public static void MakeCurrent(PlayerInformation Information)
        {
            Provider.MakeCurrent(Information);
        }

        public static void NewRun(IPlayer User)
        {
            Provider.NewRun(DataManager.DataFromPlayer(User));
        }

        public static void Unload()
        {
            Provider.Unload();
        }

        public static void Reload()
        {
            Provider.Reload();
        }
        
        public static bool Exists => Provider.Exists;
        
        public static KeyboardManager Keyboard => Provider.Keyboard;
        
        public static bool IsLoading => Provider.IsLoading;
        
        public static bool InStartMenu => Provider.InStartMenu;
        
        public static bool InMenu => Provider.InMenu;

        public static bool IsExiting => Provider.IsExiting;
        
        public static bool SpawningEffect
        {
            set
            {
                if(Provider == null) return;
                Provider.SpawningEffect = value;
            }
        }

        public static IPlayer Player
        {
            get => Provider.Player;
            set => Provider.Player = value;
        }
    }
}
