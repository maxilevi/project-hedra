using System;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Inventory;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Rendering.UI
{
    public class CompanionProfileUIElement : ProfileUIElement
    {
        private const string CompanionModelAttribute = "CompanionModel";
        private readonly IPlayer _player;
        private readonly RenderableTexture _companion;
        private IEntity _currentPet;
        private ObjectMesh _petMesh;
        
        public CompanionProfileUIElement(IPlayer Player, Vector2 Position, float Scale) : base(Player, Position, Scale)
        {
            _player = Player;
            ClassLogo.BaseTexture.TextureElement.TextureId = Graphics2D.LoadFromAssets("Assets/UI/EmptyLogo.png");
            ClassLogo.Scale = Graphics2D.SizeFromAssets("Assets/UI/EmptyLogo.png").As1920x1080() * Scale;
            _companion = new RenderableTexture(new BackgroundTexture(GUIRenderer.TransparentTexture, ClassLogo.Position, ClassLogo.Scale * .5f), DrawOrder.After);
            AddElement(_companion);
            
            _manaBar.Dispose();
            _manaBackground.Dispose();
            RemoveElement(_manaBackground);
            RemoveElement(_manaBar);
        }

        public void Update()
        {
            UpdatePet();
            if (Enabled)
            {
                base.Update();
                _companion.Position = ClassLogo.Position;
            }
        }

        private void UpdatePet()
        {
            if (_currentPet != _player.Companion.Entity)
            {
                Func<uint> idPointer;
                if (_player.Companion.Entity == null || _player.Companion.Entity.IsDead)
                    idPointer = null;
                else
                    idPointer = SetModel(_player.Companion.Item);
                _companion.BaseTexture.TextureElement.IdPointer = idPointer;
                _currentPet = _player.Companion.Entity;
            }

            if (_player.Companion.Entity == null)
            {
                Disable();
            }
            else if(_player.UI.GamePanel.Enabled)
            {
                Enable();
            }
        }

        private Func<uint> SetModel(Item Companion)
        {
            var data = Companion.GetAttribute<VertexData>(CompanionModelAttribute).Clone();
            _petMesh?.Dispose();
            _petMesh = InventoryItemRenderer.BuildModel(data, out var size);
            return () => InventoryItemRenderer.Draw(_petMesh, false, false, size);
        }
        
        protected override float MaxHealth => _currentPet?.MaxHealth ?? 0;
        protected override float Health => _currentPet?.Health ?? 0;
        protected override float MaxXP => _currentPet?.SearchComponent<CompanionStatsComponent>().MaxXP ?? 0;
        protected override float XP => _currentPet?.SearchComponent<CompanionStatsComponent>().XP ?? 0;
    }
}