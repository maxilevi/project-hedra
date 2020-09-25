using System.IO;
using System.Numerics;

namespace Hedra.Engine.Player
{
    public class CustomizationData
    {
        public HumanGender Gender { get; set; }
        public Vector4 HairColor { get; set; }
        public Vector4 SkinColor { get; set; }
        
        public void Write(BinaryWriter Writer)
        {
            Writer.Write((int)Gender);
            Writer.Write(HairColor);
            Writer.Write(SkinColor);
        }

        public static CustomizationData Read(BinaryReader Reader)
        {
            return new CustomizationData
            {
                Gender = (HumanGender) Reader.ReadInt32(),
                HairColor = Reader.ReadVector4(),
                SkinColor = Reader.ReadVector4(),
            };
        }
    }
}