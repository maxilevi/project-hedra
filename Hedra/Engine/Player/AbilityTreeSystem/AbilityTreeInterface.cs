using System.Drawing;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public class AbilityTreeInterface : InventoryArrayInterface
    {
        private readonly LocalPlayer _player;
        private readonly Panel _panel;
        private readonly RenderableTexture[] _skillPointsBackgrounTextures;

        public AbilityTreeInterface(LocalPlayer Player, InventoryArray Array, int Offset, int Length, int SlotsPerLine, Vector2 Spacing)
            : base(Array, Offset, Length, SlotsPerLine, Spacing, null)
        {
            _player = Player;
            _panel = new Panel();
            _skillPointsBackgrounTextures = new RenderableTexture[this.Buttons.Length];
            for (var i = 0; i < this.Buttons.Length; i++)
            {
                this.Buttons[i].Scale *= 1.25f;
                this.Buttons[i].Texture.IdPointer = null;
                this.ButtonsText[i].TextFont = FontCache.Get(AssetManager.Fonts.Families[0], 10f, FontStyle.Bold);
                this.ButtonsText[i].Position = this.Buttons[i].Position +
                                               new Vector2(0, -InventoryArrayInterface.DefaultSize.Y) * .65f;
                _skillPointsBackgrounTextures[i] = 
                    new RenderableTexture(new Texture("Assets/UI/InventoryBackground.png",
                    this.ButtonsText[i].Position, new Vector2(.05f, .075f)), DrawOrder.Before);

                _panel.AddElement(_skillPointsBackgrounTextures[i]);
            }
        }

        public override void UpdateView()
        {
            for (var i = 0; i < this.Buttons.Length; i++)
            {
                if (!this.Array[i + Offset].HasAttribute("Enabled")) continue;

                if (!this.Array[i + Offset].GetAttribute<bool>("Enabled"))
                {
                    if (this.Buttons[i].Scale == Vector2.Zero || this.Textures[i].Scale == Vector2.Zero) continue;
                    this.Array[i + Offset].SetAttribute("ButtonScale", this.Buttons[i].Scale);
                    this.Array[i + Offset].SetAttribute("TextureScale", this.Textures[i].Scale);
                    this.Array[i + Offset].SetAttribute("BackgroundScale", this._skillPointsBackgrounTextures[i].Scale);
                    this.Buttons[i].Scale = Vector2.Zero;
                    this.Textures[i].Scale = Vector2.Zero;
                    this.ButtonsText[i].Text = string.Empty;
                    this._skillPointsBackgrounTextures[i].Scale = Vector2.Zero;
                }
                else
                {
                    var level = this.Array[i + Offset].HasAttribute("Level")
                        ? this.Array[i + Offset].GetAttribute<int>("Level")
                        : 0;

                    this.ButtonsText[i].Text = level > 0 ? level.ToString() : string.Empty;
                    if (level > 0 && this.Array[i + Offset].HasAttribute("BackgroundScale"))
                    {
                        this._skillPointsBackgrounTextures[i].Scale =
                            this.Array[i + Offset].GetAttribute<Vector2>("BackgroundScale");
                    }
                    else if(!this.Array[i + Offset].HasAttribute("BackgroundScale"))
                    {
                        this.Array[i + Offset].SetAttribute("BackgroundScale", this._skillPointsBackgrounTextures[i].Scale);
                        this._skillPointsBackgrounTextures[i].Scale = Vector2.Zero;
                    }
                    var isLocked = this.SetGrayscale(i);

                    if (!this.Array[i + Offset].HasAttribute("ButtonScale") ||
                        !this.Array[i + Offset].HasAttribute("TextureScale")) continue;
                    this.Buttons[i].Scale = this.Array[i + Offset].GetAttribute<Vector2>("ButtonScale");
                    this.Textures[i].Scale = this.Array[i + Offset].GetAttribute<Vector2>("TextureScale");
                }
            }
        }


        private bool SetGrayscale(int Index)
        {
            var decomposedIndexY = Index % AbilityTreeSystem.AbilityTree.Layers;
            var decomposedIndexX = AbilityTreeSystem.AbilityTree.AbilityCount / AbilityTreeSystem.AbilityTree.Layers - 1 - (Index - decomposedIndexY) / AbilityTreeSystem.AbilityTree.Layers;
            this.Buttons[Index].Texture.Grayscale = decomposedIndexX * 5 > _player.Level || !this.PreviousUnlocked(Index);
            return this.Buttons[Index].Texture.Grayscale;
        }

        private bool PreviousUnlocked(int Index)
        {
            var decomposedIndexY = Index % AbilityTreeSystem.AbilityTree.Layers;
            var decomposedIndexX = AbilityTreeSystem.AbilityTree.AbilityCount / AbilityTreeSystem.AbilityTree.Layers - 1 - (Index - decomposedIndexY) / AbilityTreeSystem.AbilityTree.Layers;
            if (decomposedIndexX == 0) return true;
            else if (!this.Array[Index + AbilityTreeSystem.AbilityTree.Layers].GetAttribute<bool>("Enabled"))
                return this.PreviousUnlocked(Index + AbilityTreeSystem.AbilityTree.Layers);
            return this.Array[Index + AbilityTreeSystem.AbilityTree.Layers].GetAttribute<int>("Level") > 0;
        }

        public override bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                if (this.Enabled) _panel.Enable();
                else _panel.Disable();
            }
        }

        public virtual Vector2 Scale
        {
            get { return base.Scale; }
            set
            {
                for (var i = 0; i < _skillPointsBackgrounTextures.Length; i++)
                {
                    _skillPointsBackgrounTextures[i].Scale = new Vector2(_skillPointsBackgrounTextures[i].Scale.X / base.IndividualScale.X,
                                                        _skillPointsBackgrounTextures[i].Scale.Y / base.IndividualScale.Y) * value;

                    var relativePosition = _skillPointsBackgrounTextures[i].Position - Position;
                    _skillPointsBackgrounTextures[i].Position = new Vector2(relativePosition.X / base.Scale.X,
                        relativePosition.Y / base.Scale.Y) * value + Position;
                }
                base.Scale = value;
            }
        }

        public virtual Vector2 IndividualScale
        {
            get { return base.IndividualScale; }
            set
            {
                for (var i = 0; i < _skillPointsBackgrounTextures.Length; i++)
                {
                    _skillPointsBackgrounTextures[i].Scale = new Vector2(_skillPointsBackgrounTextures[i].Scale.X / base.IndividualScale.X,
                                                                 _skillPointsBackgrounTextures[i].Scale.Y / base.IndividualScale.Y) * value;
                }
                base.IndividualScale = value;
            }
        }

        public virtual Vector2 Position
        {
            get { return base.Position; }
            set
            {
                for (var i = 0; i < _skillPointsBackgrounTextures.Length; i++)
                {
                    _skillPointsBackgrounTextures[i].Position = _skillPointsBackgrounTextures[i].Position - base.Position + value;
                }
                base.Position = value;
            }
        }
    }
}
