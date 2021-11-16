namespace Hedra.Engine.Player
{
    public interface IObjectWithMovement
    {
        bool CaptureMovement { set; }
        void Orientate();
    }
}