using Hedra.EntitySystem;
using Newtonsoft.Json;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public class GiverTemplate
    {
        public int Seed { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        [JsonIgnore]
        public Vector3 Position
        {
            get => new Vector3(X,Y,Z);
            private set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        public static GiverTemplate FromHumanoid(IHumanoid Humanoid)
        {
            return new GiverTemplate
            {
                Seed = Humanoid.Seed,
                Position = Humanoid.Position
            };
        }
    }
}