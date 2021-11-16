/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/01/2017
 * Time: 02:38 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Game;

namespace Hedra.Engine.Management
{
    /// <summary>
    ///     Description of AutosaveManager.
    /// </summary>
    public static class AutosaveManager
    {
        private const int SecondsTimePerSave = 30;
        private static float _passedTime;

        public static void Update()
        {
            if (!GameSettings.Autosave || GameManager.InStartMenu) return;

            _passedTime += Time.IndependentDeltaTime;
            if (_passedTime >= SecondsTimePerSave)
            {
                Save();
                _passedTime = 0;
            }
        }

        public static void Save()
        {
            GameManager.Unload();

            for (var i = 0; i < GameManager.Player.Toolbar.Skills.Length; i++)
                GameManager.Player.Toolbar.Skills[i].Unload();

            DataManager.SavePlayer(
                DataManager.DataFromPlayer(GameManager.Player)
            );

            for (var i = 0; i < GameManager.Player.Toolbar.Skills.Length; i++)
                GameManager.Player.Toolbar.Skills[i].Load();

            GameManager.Reload();
        }
    }
}