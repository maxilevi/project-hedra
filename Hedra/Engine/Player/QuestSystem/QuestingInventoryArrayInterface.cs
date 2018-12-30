using System.Drawing;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.PagedInterface;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.QuestSystem
{
    public sealed class QuestingInventoryArrayInterface : PagedInventoryArrayInterface
    {
        private readonly Texture _mapRender;
        private readonly Texture _descriptionBackground;
        
        public QuestingInventoryArrayInterface(IPlayer Player, InventoryArray Array, int Rows, int Columns) 
            : base(Player, Array, Rows, Columns, Vector2.One * 1.5f)
        {
            var barScale = new Vector2(DefaultSize.X * 5, DefaultSize.Y) * .5f;
            for (var i = 0; i < Textures.Length; ++i)
            {
                //Textures[i].Position -= Vector2.UnitX * barScale.X;
                //Buttons[i].Position -= Vector2.UnitX * barScale.X;
                //Textures[i].Position += Vector2.UnitX * Textures[i].Scale.X;
                //Buttons[i].Position += Vector2.UnitX * Textures[i].Scale.X;
                //ButtonsText[i].TextFont = FontCache.Get(AssetManager.BoldFamily, 9f, FontStyle.Bold);
                //SelectedTextures[i].Position = Textures[i].Position;
            }
            _mapRender = new Texture("Assets/UI/InventoryItemInfo.png", Vector2.UnitY * DefaultSize * 3f, new Vector2(.5f, .4f));
            _descriptionBackground = new Texture("Assets/UI/InventoryBackground.png", Vector2.UnitY * DefaultSize * -2, Vector2.One * .5f);
            Panel.AddElement(_mapRender);
            Panel.AddElement(_descriptionBackground);
            Scale *= 1.1f;
        }

        public override void UpdateView()
        {
            base.UpdateView();
        }

        protected override void ResetSlot(int Index)
        {
            base.ResetSlot(Index);
        }

        protected override void SetSlot(int Index)
        {
            base.SetSlot(Index);
        }

        protected override Translation TitleTranslation => Translation.Create("quests");

        protected override Item[] ArrayObjects => Player.Questing.ActiveQuests.Select(Q => Q.ToItem()).ToArray();
    }
}