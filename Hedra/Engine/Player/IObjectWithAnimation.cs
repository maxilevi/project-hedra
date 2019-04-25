using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.Player
{
    public interface IObjectWithAnimation
    {
        Animation AnimationBlending { get; }
        void ResetModel();
        void BlendAnimation(Animation Animation);
    }
}