using Hedra.Engine.Player.Inventory;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.Player.QuestSystem.Views
{
    public class ModelView : QuestView
    {
        private readonly ObjectMesh _currentItemMesh;
        private readonly float _currentItemMeshHeight;
  
        public ModelView(QuestObject Quest, VertexData PreviewMesh) : base(Quest)
        {
            _currentItemMesh = 
                InventoryItemRenderer.BuildModel(PreviewMesh, out _currentItemMeshHeight);
            _currentItemMesh.ApplyNoiseTexture = true;
        }

        public override uint GetTextureId()
        {
            return InventoryItemRenderer.Draw(_currentItemMesh, false, false,
                _currentItemMeshHeight * InventoryItemRenderer.ZOffsetFactor);
        }
    }
}