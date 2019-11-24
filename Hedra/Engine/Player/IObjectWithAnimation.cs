using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.Player
{
    public interface IObjectWithAnimation
    {
        Animation AnimationBlending { get; }
        void ResetModel();
        void PlayAnimation(Animation Animation);
        void BlendAnimation(Animation Animation);
    }
}