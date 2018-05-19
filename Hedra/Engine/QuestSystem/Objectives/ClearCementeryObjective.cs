using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scenes;
using OpenTK;
using OpenTK.Platform.Windows;

namespace Hedra.Engine.QuestSystem.Objectives
{
    public class ClearCementeryObjective : Objective
    {

        private ObjectMesh _cementeryModel;
        private bool _arrivedCementery;
        private int _enemiesRemaining;
        private Graveyard _graveyard;

        public override bool ShouldDisplay => true;
        public override string Description =>  _arrivedCementery ? "Slay the "+ _enemiesRemaining+" remaining abominations which protect the mausoleum." : "Restore the peace at the enchated grageyard.";

        public override Vector3 IconPosition
        {
            get
            {
                if (_arrivedCementery && _graveyard.Enemies.Length > 0)
                {
                    for (var i = 0; i < _graveyard.Enemies.Length; i++)
                    {
                        if (_graveyard.Enemies[i] != null && !_graveyard.Enemies[i].IsDead)
                            return _graveyard.Enemies[i].Position;
                    }
                }
                return this.ObjectivePosition;
            }
        }
        public override uint QuestLogIcon
        {
            get
            {
                //GameManager.Player.UI.DrawPreview(_cementeryModel, UserInterface.QuestFbo);
                return UserInterface.QuestFbo.TextureID [0];
            }
        }

        public override void Recreate()
        {
            base.Recreate();
            base.NoTreesRadius = 12;
            //_cementeryModel = VillageGenerator.GenerateCementeryIcon(Vector3.One * .15f , -Vector3.UnitY * 1.5f);
        }

        public override void SetOutObjectives()
        {
            AvailableOuts.Add(new VillageObjective());
        }

        public override void Setup(Chunk UnderChunk)
        {
            Vector3 blockSpace = World.ToBlockSpace(ObjectivePosition);
            //_graveyard = World.StructureGenerator.GenerateCementery((int) blockSpace.X, (int) blockSpace.Y, (int) blockSpace.Z, UnderChunk);
            CoroutineManager.StartCoroutine(Update);
        }

        private IEnumerator Update()
        {
            while (!Disposed)
            {
                var enemyCount = 0;
                for (var i = 0; i < _graveyard.Enemies.Length; i++)
                {
                    if (_graveyard.Enemies[i] == null || _graveyard.Enemies[i].Model.Disposed ||
                        _graveyard.Enemies[i].IsDead) continue;
                    enemyCount++;
                }
                _enemiesRemaining = enemyCount;

                if (_enemiesRemaining != enemyCount)
                {
                    _enemiesRemaining = enemyCount;
                    if (LocalPlayer.Instance.QuestLog.Show)
                        LocalPlayer.Instance.QuestLog.UpdateText();
                }

                if ((_graveyard.Position.Xz - LocalPlayer.Instance.Position.Xz).LengthSquared
                    < _graveyard.Radius * .5f * _graveyard.Radius * .5f)
                {
                    _arrivedCementery = true;
                    if(LocalPlayer.Instance.QuestLog.Show)
                        LocalPlayer.Instance.QuestLog.UpdateText();
                }

                if (_graveyard.Enemies != null && _graveyard.Restored)
                {
                    this.NextObjective();
                }

                yield return null;
            }
        }

        public override void Dispose()
        {
            this.Disposed = true;
        }
    }
}
