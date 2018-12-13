using System;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player
{
    public interface ICamera
    {
        float TargetPitch { get; set; }
        float TargetYaw { get; set; }
        float MaxDistance { get; set; }
        float MinDistance { get; set; }
        float MaxPitch { get; set; }
        float MinPitch { get; set; }
        float AddedDistance { get; set; }
        float Distance { get; set; }
        float WheelSpeed { get; set; }
        bool CaptureMovement { get; set; }
        bool LockMouse { get; set; }
        Matrix4 ModelViewMatrix { get; }
        Func<Vector3> PositionDelegate { get; set; }
        float Pitch { get; set; }
        float Yaw { get; set; }
        float StackedYaw { get; }
        float TargetDistance { get; set; }
        Vector3 LookingDirection { get; }
        Vector3 Forward { get; }
        Vector3 Backward { get; }
        Vector3 Right { get; }
        Vector3 Left { get; }
        Vector3 CameraHeight { get; set; }
        Vector3 CameraEyePosition { get; }
        Vector3 CrossDirection { get; }
        void Update();
        void BuildCameraMatrix();
        void Reset();
        void OnMouseWheel(object Sender, MouseWheelEventArgs E);
        void OnMouseButtonUp(object Sender, MouseButtonEventArgs e);
        void OnMouseButtonDown(object Sender, MouseButtonEventArgs e);
        void OnMouseMove(object Sender, MouseMoveEventArgs e);
        void OnKeyDown(object Sender, KeyboardKeyEventArgs EventArgs);
        void OnKeyUp(object Sender, KeyboardKeyEventArgs e);
        void OnKeyPress(object Sender, KeyPressEventArgs e);
    }
}