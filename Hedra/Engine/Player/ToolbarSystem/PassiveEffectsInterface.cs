using System.Linq;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using OpenTK;

namespace Hedra.Engine.Player.ToolbarSystem
{
    public class PassiveEffectsInterface : Panel
    {
        private const int MaxEffects = 8;
        private readonly Texture[] _textures;
        private Vector2 _previousOffset;
        
        public PassiveEffectsInterface()
        {
            _textures = new Texture[MaxEffects];
            var size = InventoryArrayInterface.DefaultSize * .25f;
            var padding = Vector2.UnitX * size.X * .35f;
            var offset = size.X * .5f * Vector2.UnitX * 0f;
            for (var i = 0; i < _textures.Length; ++i)
            {
                _textures[i] = new Texture(GUIRenderer.TransparentTexture, offset, size)
                {
                    TextureElement =
                    {
                        MaskId = InventoryArrayInterface.DefaultId,
                        Opacity = .85f
                    }
                };
                offset += Vector2.UnitX * _textures[i].Scale.X * 2 + padding;
                AddElement(_textures[i]);
            }
        }

        public void UpdateView(BaseSkill[] Skills)
        {
            for (var i = 0; i < _textures.Length; ++i)
            {
                _textures[i].TextureElement.TextureId = GUIRenderer.TransparentTexture;
            }
            var skills = Skills.Take(_textures.Length).ToArray();
            for (var i = 0; i < skills.Length; ++i)
            {
                _textures[i].TextureElement.TextureId = skills[i].TextureId;
            }
            var size = InventoryArrayInterface.DefaultSize * .25f;
            var padding = Vector2.UnitX * size.X * .35f;
            var sum = _textures.Sum(T => T.TextureElement.TextureId != GUIRenderer.TransparentTexture ? T.TextureElement.Scale.X : 0 + padding.X * -.5f);
            var offset = -(sum) * Vector2.UnitX;
            for (var i = 0; i < _textures.Length; ++i)
            {
                _textures[i].Position -= _previousOffset;
                _textures[i].Position += offset;
            }

            _previousOffset = offset;
        }
    }
}