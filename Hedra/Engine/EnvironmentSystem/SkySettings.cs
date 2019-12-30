namespace Hedra.Engine.EnvironmentSystem
{
    public class SkySettings
    {
        public float DayTime { get; set; } = 12000;
        public float DaytimeSpeed { get; set; } = 1f;
        public bool Enabled { get; set; } = true;
        public bool UpdateDayColors { get; set; } = true;
    }
}