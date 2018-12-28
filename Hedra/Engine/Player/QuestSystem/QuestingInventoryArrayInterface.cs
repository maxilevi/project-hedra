using System.Drawing;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.QuestSystem
{
    public class QuestingInventoryArrayInterface : CraftingInventoryArrayInterface
    {
        private readonly Texture[] _descriptions;
        
        public QuestingInventoryArrayInterface(IPlayer Player, InventoryArray Array, int Rows, int Columns) : base(Player, Array, Rows, Columns)
        {
            var barScale = new Vector2(DefaultSize.X * Rows, DefaultSize.Y * 2) * .75f;
            var realScale = Graphics2D.SizeFromAssets("Assets/UI/InventoryBackground.png") * barScale;
            _descriptions = new Texture[Textures.Length];
            for (var i = 0; i < Textures.Length; ++i)
            {
                Textures[i].Position -= Vector2.UnitX * realScale.X;
                Buttons[i].Position -= Vector2.UnitX * realScale.X;
                _descriptions[i] = 
                    new Texture(
                        "Assets/UI/InventoryBackground.png",
                        Textures[i].Position + Vector2.UnitX * (Textures[i].Scale.X + realScale.X),
                        barScale
                    );
                _descriptions[i].SendBack();
                ButtonsText[i].TextFont = FontCache.Get(AssetManager.BoldFamily, 14f, FontStyle.Bold);
                ButtonsText[i].Position = _descriptions[i].Position;
                Panel.AddElement(_descriptions[i]);
            }
        }
        
        protected override Translation TitleTranslation => Translation.Create("quests");
        
        protected override Item[] ArrayObjects => new Item[0];
    }
}