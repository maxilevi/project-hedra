using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using OpenTK.Input;
using Panel = Hedra.Engine.Rendering.UI.Panel;

namespace Hedra.Engine.Testing.AutomatedTests
{
    public class MenuTest : BaseTest
    {
        private const string _characterName = "_test";

        [AutomatedTest]
        public void TestCharacterSelectionWorks()
        {
            this.GoToCreator();

            var chooser = this.FindFirst<OptionChooser>(GameManager.Player.UI.ChrCreator.Elements);
            chooser.RightArrow.ForceClick();
            Thread.Sleep(100);
            chooser.RightArrow.ForceClick();
            Thread.Sleep(100);
            chooser.RightArrow.ForceClick();
            Thread.Sleep(100);
            chooser.RightArrow.ForceClick();
            Thread.Sleep(100);
            chooser.LeftArrow.ForceClick();
            Thread.Sleep(100);
            chooser.LeftArrow.ForceClick();
            Thread.Sleep(100);
            chooser.LeftArrow.ForceClick();
            Thread.Sleep(100);
            chooser.LeftArrow.ForceClick();

            this.GoToMenu();
        }

        [AutomatedTest]
        public void TestCanCreateNewCharacter()
        {
            this.GoToCreator();

            var textbox = this.FindFirst<TextField>(GameManager.Player.UI.ChrCreator.Elements);
            textbox.Text = _characterName;
            this.Find<Button>(GameManager.Player.UI.ChrCreator.Elements).First( B => B.Text.Text == "Create").ForceClick();

            this.GoToMenu();
        }

        //[AutomatedTest]
        public void TestCanLoadWithNewCharacter()
        {
            Thread.Sleep(2000);
            this.GoToChooser();
            var information = DataManager.PlayerFiles[
                Array.IndexOf(DataManager.PlayerFiles, DataManager.PlayerFiles.First( F => F.Name == _characterName))
            ];
            GameManager.MakeCurrent(information);
        }

        private void GoToChooser()
        {
            GameManager.Player.UI.ChrChooser.Disable();
            GameManager.Player.UI.ChrCreator.Enable();
            Thread.Sleep(500);
        }

        private void GoToCreator()
        {
            GameManager.Player.UI.Menu.Disable();
            GameManager.Player.UI.ChrChooser.Enable();
            Thread.Sleep(500);
            GameManager.Player.UI.ChrChooser.Disable();
            GameManager.Player.UI.ChrCreator.Enable();
            Thread.Sleep(500);
        }

        private void GoToMenu()
        {
            GameManager.Player.UI.ChrCreator.Disable();
            GameManager.Player.UI.ChrChooser.Enable();
            Thread.Sleep(500);
            GameManager.Player.UI.ChrChooser.Disable();
            GameManager.Player.UI.Menu.Enable();
            Thread.Sleep(500);
        }

        private T[] Find<T>(IEnumerable<object> Elements)
        {
            return Elements.OfType<T>().ToArray();
        }

        private T FindFirst<T>(IEnumerable<object> Elements)
        {
            return this.Find<T>(Elements).First();
        }
    }
}
