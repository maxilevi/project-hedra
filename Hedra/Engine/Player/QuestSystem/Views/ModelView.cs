using Hedra.Engine.Player.Inventory;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Player.QuestSystem.Views
{
    public class ModelView : QuestView
    {
        private readonly ObjectMesh _currentItemMesh;
        private readonly Vector3 _currentItemMeshSize;
  
        public ModelView(VertexData PreviewMesh)
        {
            _currentItemMesh = 
                InventoryItemRenderer.BuildModel(PreviewMesh, out _currentItemMeshSize);
            _currentItemMesh.ApplyNoiseTexture = true;
        }

        public override uint GetTextureId()
        {
            return InventoryItemRenderer.Draw(_currentItemMesh, false, false, _currentItemMeshSize);
        }

        public override void Dispose()
        {
            _currentItemMesh.Dispose();
        }
    }
}