using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Objectives
{
    public class RecoverBlacksmithHammerObjective : Objective
    {
        public override string Description => "";
        public override bool ShouldDisplay => true;

        public override uint QuestLogIcon
        {
            get
            {
                return UserInterface.QuestFbo.TextureID[0];
            }
        }

        public RecoverBlacksmithHammerObjective()
        {
            base.CenterRadius = 768;
            base.CenterHeight = 800;

        }

        public override void Setup(Chunk UnderChunk){}

        public override void Recreate()
        {
            base.Recreate();
            CoroutineManager.StartCoroutine(Update);

            World.Highlighter.HighlightArea(this.ObjectivePosition, Vector4.One * .2f, base.CenterRadius * .8f, -1);
        }

        private IEnumerator Update()
        {
            while (!this.Disposed)
            {
                yield return null;
            }
        }

        public override void Dispose()
        {

        }
    }
}
   