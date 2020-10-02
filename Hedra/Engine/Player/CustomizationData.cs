using System.IO;
using System.Numerics;
using Hedra.Engine.ClassSystem;
using Hedra.Rendering;

namespace Hedra.Engine.Player
{
    public class CustomizationData
    {
        public HumanGender Gender { get; set; }
        public Vector4 FirstHairColor { get; set; }
        public Vector4 SecondHairColor { get; set; }
        public Vector4 SkinColor { get; set; }

        public static CustomizationData FromClass(ClassDesign Design, HumanGender Gender)
        {
            return new CustomizationData
            {
                Gender = Gender,
                FirstHairColor = Gender == HumanGender.Male ? Design.DefaultFirstHairColor : Design.FemaleDefaultFirstHairColor,
                SecondHairColor = Gender == HumanGender.Male ? Design.DefaultSecondHairColor : Design.FemaleDefaultSecondHairColor,
                SkinColor = Gender == HumanGender.Male ? Design.DefaultSkinColor : Design.FemaleDefaultSkinColor,
            };
        }
        
        public void Write(BinaryWriter Writer)
        {
            Writer.Write((int)Gender);
            Writer.Write(FirstHairColor);
            Writer.Write(SecondHairColor);
            Writer.Write(SkinColor);
        }

        public static CustomizationData Read(BinaryReader Reader)
        {
            return new CustomizationData
            {
                Gender = (HumanGender) Reader.ReadInt32(),
                FirstHairColor = Reader.ReadVector4(),
                SecondHairColor = Reader.ReadVector4(),
                SkinColor = Reader.ReadVector4(),
            };
        }
    }
}