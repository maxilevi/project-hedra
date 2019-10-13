using System;
using Hedra.Engine.Player;
using OpenToolkit.Mathematics;


namespace HedraTests.Player
{
    public class SimpleCameraMock : ICamera
    {
        public bool AllowClipping { get; set; }
        public float TargetPitch { get; set; }
        public float TargetYaw { get; set; }
        public float MaxDistance { get; set; }
        public float MinDistance { get; set; }
        public float MaxPitch { get; set; }
        public float MinPitch { get; set; }
        public float AddedDistance { get; set; }
        public float Distance { get; set; }
        public float WheelSpeed { get; set; }
        public bool CaptureMovement { get; set; }
        public bool LockMouse { get; set; }
        public Matrix4 ModelViewMatrix { get; }
        public Func<Vector3> PositionDelegate { get; set; }
        public int XDelta { get; }
        public int YDelta { get; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float StackedYaw { get; }
        public float TargetDistance { get; set; }
        public Vector3 LookingDirection { get; set; }
        public Vector3 Forward { get; }
        public Vector3 Backward { get; }
        public Vector3 Right { get; }
        public Vector3 Left { get; }
        public Vector3 CameraHeight { get; set; }
        public Vector3 CameraEyePosition { get; }
        public Vector3 CrossDirection { get; }
        public Vector3 CrossPosition { get; }
        public Matrix4 ViewMatrix { get; }
        public void Update()
        {
            throw new NotImplementedException();
        }

        public void BuildCameraMatrix()
        {
            throw new NotImplementedException();
        }

        public void OnMouseWheel(object Sender, MouseWheelEventArgs E)
        {
            throw new NotImplementedException();
        }

        public void OnMouseButtonUp(object Sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void OnMouseButtonDown(object Sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void OnMouseMove(object Sender, MouseMoveEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void OnKeyDown(object Sender, KeyboardKeyEventArgs EventArgs)
        {
            throw new NotImplementedException();
        }

        public void OnKeyUp(object Sender, KeyboardKeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void OnKeyPress(object Sender, KeyPressEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}