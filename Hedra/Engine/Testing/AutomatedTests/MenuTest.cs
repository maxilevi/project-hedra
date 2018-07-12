using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hedra.Engine.Events;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Testing.AutomatedTests
{
    internal class MenuTest : BaseAutomatedTest
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

        [AutomatedTest]
        public void TestCanLoadWithNewCharacter()
        {
            Thread.Sleep(2000);
            this.GoToChooser();
            Executer.ExecuteOnMainThread(delegate
            {
                GameManager.MakeCurrent(DataManager.PlayerFiles.First(F => F.Name == _characterName));
                CoroutineManager.StartCoroutine(this.TestGameUIWorks);
            });
        }

        private IEnumerator TestGameUIWorks()
        {
            EventDispatcher.OnKeyDown(this, this.SimulateKeyEvent(Key.I));
            yield return this.WaitSeconds(1f);
            EventDispatcher.OnKeyDown(this, this.SimulateKeyEvent(Key.I));
            yield return this.WaitSeconds(1f);
            EventDispatcher.OnKeyDown(this, this.SimulateKeyEvent(Key.X));
            yield return this.WaitSeconds(1f);
            EventDispatcher.OnKeyDown(this, this.SimulateKeyEvent(Key.X));
            yield return this.WaitSeconds(1f);
            EventDispatcher.OnKeyDown(this, this.SimulateKeyEvent(Key.M));
            yield return this.WaitSeconds(2f);
            EventDispatcher.OnMouseButtonDown(this, this.SimulateMouseButtonEvent(MouseButton.Left, Vector2.Zero));
            yield return this.WaitSeconds(1f);
            EventDispatcher.OnKeyDown(this, this.SimulateKeyEvent(Key.M));
        }

        private IEnumerator WaitSeconds(float Seconds)
        {
            var passedTime = 0f;
            while (passedTime < Seconds)
            {
                passedTime += (float)Time.DeltaTime;
                yield return null;
            }
        }

        private void GoToChooser()
        {
            this.GoToMenu();
            this.Find<Button>(GameManager.Player.UI.Menu.Elements).First(B => B.Text.Text == "Load World").ForceClick();
            Thread.Sleep(500);
        }

        private void GoToCreator()
        {
            this.GoToMenu();
            this.GoToChooser();
            this.Find<Button>(GameManager.Player.UI.ChrChooser.Elements).First(B => B.Text.Text == "New Character").ForceClick();
            Thread.Sleep(500);
        }

        private void GoToMenu()
        {
            GameManager.Player.UI.ChrCreator.Disable();
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
